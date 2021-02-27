using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Middleware;
using Gtt.CodeWorks.StateMachines.Middleware;
using Gtt.CodeWorks.Validation;
using Stateless;
using Stateless.Graph;
using Stateless.Reflection;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class BaseStatefulServiceInstance<TRequest, TResponse, TState, TTrigger, TData>
        : BaseServiceInstance<TRequest, TResponse>, IStatefulServiceInstance
        where TRequest : BaseStatefulRequest<TTrigger>, new()
        where TResponse : BaseStatefulResponse<TState, TTrigger, TData>, new()
        where TState : struct, IConvertible
        where TTrigger : struct, IConvertible
        where TData : BaseStateDataModel<TState>, new()
    {
        protected BaseStatefulServiceInstance(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies)
        {
            _stateRepository = statefulDependencies.StateRepository;

            var existingValidation = PipeLine.FirstOrDefault(x => typeof(ValidationMiddleware) == x.GetType());
            var idx = PipeLine.IndexOf(existingValidation);
            PipeLine.RemoveAt(idx);
            PipeLine.Insert(idx, new StateMachineValidatorMiddleware<TTrigger>(coreDependencies.RequestValidator));

            _data = new TData();
            Machine = new StateMachine<TState, TTrigger>(() => _data.State, s => _data.State = s);
            Machine.OnTransitionCompletedAsync(OnTransitionAction);
            Rules(Machine);
            RegisterTriggerActions(_registrationFactory);
            SetupParameterData();
            
        }

        private readonly Dictionary<TTrigger, PropertyInfo> _propertyDict = new Dictionary<TTrigger, PropertyInfo>();
        private readonly RegistrationFactory _registrationFactory = new RegistrationFactory();

        private void SetupParameterData()
        {
            Type t = typeof(TRequest);
            var props = t.GetProperties();

            foreach (var trigger in Enum.GetValues(typeof(TTrigger)))
            {
                var match = props.FirstOrDefault(x => x.Name.Equals(trigger.ToString(), StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                {
                    _propertyDict[(TTrigger)trigger] = match;
                    Machine.SetTriggerParameters((TTrigger)trigger, typeof(TRequest), match.PropertyType);
                }
                else
                {
                    Machine.SetTriggerParameters((TTrigger)trigger, typeof(TRequest));
                }
            }
        }

        protected StateMachine<TState, TTrigger> Machine { get; }

        private async Task OnTransitionAction(StateMachine<TState, TTrigger>.Transition transition)
        {
            if (Status != MachineStatus.Started)
            {
                throw new Exception("Machine must be started before it can run. Call Start()");
            }

            await StoreState(_identifier, transition.Source.ToString(), transition.Destination.ToString(),
                transition.Trigger.ToString(), transition.IsReentry);
        }

        protected sealed override async Task<ServiceResponse<TResponse>> Implementation(TRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request.Trigger != null, "request.Trigger != null");

            if (!CanCallAction(request.Trigger.Value))
            {
                throw new BusinessLogicException($"Cannot call {request.Trigger} on {FullName}:{_identifier}", ServiceResult.ConflictingRequest);
            }

            var hasKey = _propertyDict.ContainsKey(request.Trigger.Value);
            var p = _propertyDict.GetValueOrDefault(request.Trigger.Value);
            if (p != null)
            {
                var data = p.GetValue(request);

                if (hasKey && data == null)
                {
                    return ValidationError(new ValidationErrorData
                    {
                        ErrorMessage = $"{p.Name} payload is required",
                        Members = new[] { p.Name }
                    });
                }

                var registrationResponse = await ExecuteRegistrations(request, cancellationToken);
                if (registrationResponse != null) return registrationResponse;

                await Machine.FireAsync(
                    new StateMachine<TState, TTrigger>.TriggerWithParameters<TRequest, object>(request.Trigger.Value), request, data);
            }
            else
            {
                var registrationResponse = await ExecuteRegistrations(request, cancellationToken);
                if (registrationResponse != null) return registrationResponse;

                await Machine.FireAsync(
                    new StateMachine<TState, TTrigger>.TriggerWithParameters<TRequest>(request.Trigger.Value), request);
            }

            var response = new TResponse
            {
                StateMachine = GetStateData(),
                Model = CurrentData
            };

            ModifyResponse(response);
            return Successful(response);
        }

        private readonly IStateRepository _stateRepository;
        private TData _data;
        private string _identifier;
        public long SerialNumber { get; set; }
        public MachineStatus Status { get; private set; }

        public async Task ReadState(TRequest request)
        {
            var result = await LoadData(request.Identifier, FullName, _data, _stateRepository, request?.Get?.Version);
            SerialNumber = result.sequenceNumber;
            if (result.data != null) _data = result.data;
            CreatedDate = result.created;
            ModifiedDate = result.modified;
            if (Status == MachineStatus.Stopped)
            {
                Status = MachineStatus.DataLoaded;
            }
        }

        public sealed override async Task<ServiceResponse<TResponse>> Execute(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.Identifier))
            {

                _identifier = request.Identifier;

                if (request.Trigger != null)
                {
                    await Start(request);
                }
                else
                {
                    await ReadState(request);
                }
            }

            var r = await base.Execute(request, startTime, cancellationToken);
            return r;
        }

        private async Task<ServiceResponse<TResponse>> ExecuteRegistrations(TRequest request, CancellationToken cancellationToken)
        {
            foreach (var trigger in _registrationFactory.Triggers)
            {
                if (request.Trigger != null && trigger.Trigger.Equals(request.Trigger.Value))
                {
                    if (trigger.States.Length == 0 || trigger.States.Any(IsInState))
                    {
                        var t = trigger.Execute(request, cancellationToken);

                        if (t is Task<ServiceResponse<TResponse>> tr)
                        {
                            var sr = await tr;
                            if (sr != null)
                            {
                                return sr;
                            }
                        }

                        await t;
                    }
                }
            }

            return null;
        }

        private async Task StoreState(string identifier, string source, string destination, string trigger, bool isReentry)
        {
            SerialNumber = await _stateRepository.StoreStateData<TData, TState>(new StateDto
            {
                MachineName = FullName,
                Identifier = identifier,
                Source = source,
                Destination = destination,
                Trigger = trigger,
                IsReentry = isReentry,
                CreatedBy = FullName,
                ModifiedBy = FullName,
                Created = CreatedDate
            }, SerialNumber, _data);
        }

        protected override Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken)
        {
            var key = (request?.Identifier ?? "").Trim();
            return Task.FromResult(key);
        }

        protected virtual void RegisterTriggerActions(RegistrationFactory register)
        {

        }

        public override ServiceAction Action => ServiceAction.Stateful;

        protected abstract void Rules(StateMachine<TState, TTrigger> machine);
        protected async Task<(TData data, long sequenceNumber, DateTimeOffset created, DateTimeOffset modified)> LoadData(
            string identifier,
            string machineName,
            TData currentData,
            IStateRepository repository,
            long? version)
        {
            var stateInformation = await repository.RetrieveStateData<TData, TState>(identifier, machineName, version);
            if (stateInformation == null)
            {
                return (currentData ?? new TData(), 0L, ServiceClock.CurrentTime(),
                    ServiceClock.CurrentTime());
            }

            return (stateInformation.Data,
                    stateInformation.StateMetaData.SequenceNumber,
                    stateInformation.StateMetaData.Created,
                    stateInformation.StateMetaData.Modified);
        }

        public async Task Start(TRequest request)
        {
            if (Status == MachineStatus.Started)
            {
                throw new Exception($"Machine {FullName}:{request.Identifier} is already started");
            }

            if (Status == MachineStatus.Stopped)
            {
                await ReadState(request);
            }

            Status = MachineStatus.Started;
        }

        public bool IsNew()
        {
            return SerialNumber == 0;
        }

        protected virtual void ModifyResponse(TResponse response)
        {
            return;
        }

        protected override Task<ServiceResponse<TResponse>> BeforeImplementation(TRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_identifier))
            {
                var idRequired = ValidationError(new ValidationErrorData
                {
                    ErrorMessage = "The Identifier field is required",
                    Members = new[] { "Identifier" }
                });

                return Task.FromResult(idRequired);
            }

            if (request.Trigger == null)
            {
                if (!IsNew())
                {
                    var response = new TResponse
                    {
                        Model = _data,
                        StateMachine = GetStateData()
                    };

                    return SuccessfulTask(response);
                }

                return Task.FromResult(NotFound());
            }

            return Task.FromResult((ServiceResponse<TResponse>)null);
        }

        protected StateMachineData<TState, TTrigger> GetStateData()
        {
            return new StateMachineData<TState, TTrigger>
            {
                SerialNumber = SerialNumber,
                Identifier = _identifier,
                CurrentState = _data.State,
                AllowedTriggers = Machine.PermittedTriggers.ToArray(),
                ActiveStates = GetAllCurrentStates()
            };
        }

        protected Task FireAsync(TTrigger trigger)
        {
            if (!CanCallAction(trigger))
            {
                throw new BusinessLogicException($"Cannot call {trigger} on {FullName}:{_identifier}", ServiceResult.ConflictingRequest);
            }

            return Machine.FireAsync(trigger);
        }

        protected bool CanCallAction(TTrigger action)
        {
            return Machine.CanFire(action);
        }

        protected bool IsInState(TState state)
        {
            return Machine.IsInState(state);
        }

        protected TData CurrentData => _data;

        private TState[] GetAllCurrentStates()
        {
            try
            {
                List<TState> states = new List<TState>();
                StateInfo si;
                // LOAD INITIAL STATE FROM THE CURRENT STATE
                si = Machine.GetInfo().States.FirstOrDefault(x => x.UnderlyingState.Equals(Machine.State));

                if (si == null)
                {
                    states.Add(Machine.State);
                }
                else
                {
                    do
                    {
                        var st = (TState)si.UnderlyingState;
                        states.Add(st);
                        si = si.Superstate;
                    } while (si != null);
                }

                return states.ToArray();
            }
            catch (Exception)
            {
                return new[] { Machine.State };
            }
        }

        public DateTimeOffset CreatedDate { get; private set; }
        public DateTimeOffset ModifiedDate { get; private set; }

        protected static T As<T>(StateMachine<TState, TTrigger>.Transition transition)
        {
            if (transition.Parameters == null || transition.Parameters.Length < 2)
            {
                return default(T);
            }

            var v = transition.Parameters[1];
            return (T)v;
        }
        protected ServiceResponse<TResponse> Continue()
        {
            return null;
        }

        protected T As<T>(TRequest request)
        {
            Debug.Assert(request.Trigger != null, "request.Trigger != null");
            var data = _propertyDict[request.Trigger.Value];
            return (T)data.GetValue(request);
        }

        protected static TRequest Request(StateMachine<TState, TTrigger>.Transition transition)
        {
            if (transition.Parameters == null || transition.Parameters.Length < 1)
            {
                return default(TRequest);
            }

            var v = transition.Parameters[0];
            return (TRequest)v;
        }

        public (string diagram, string contentType) Diagram()
        {
            var d = UmlDotGraph.Format(Machine.GetInfo());
            return (d, "text/vnd.graphviz");
        }

        public class OnTriggerAction
        {
            private Func<TRequest, CancellationToken, Task> _action;
            private readonly List<TState> _states = new List<TState>();

            public OnTriggerAction(TTrigger trigger, TState[] whenInStates = null)
            {
                Trigger = trigger;
                if (whenInStates?.Length > 0)
                {
                    _states = whenInStates.ToList();
                }
            }

            public TTrigger Trigger { get; }
            public TState[] States => _states?.ToArray() ?? new TState[0];

            public OnTriggerAction Do(Func<TRequest, CancellationToken, Task> action)
            {
                _action = action;
                return this;
            }
            public OnTriggerAction DoThen(Func<TRequest, CancellationToken, Task<ServiceResponse<TResponse>>> action)
            {
                _action = action;
                return this;
            }

            public OnTriggerAction WhenInState(params TState[] states)
            {
                if (states?.Length > 0)
                {
                    foreach (var state in states)
                    {
                        _states.Add(state);
                    }
                }

                return this;
            }

            public Task Execute(TRequest request, CancellationToken cancellationToken)
            {
                return _action(request, cancellationToken);
            }
        }

        public class RegistrationFactory
        {
            private readonly List<OnTriggerAction> _triggers = new List<OnTriggerAction>();

            public OnTriggerAction OnTrigger(TTrigger trigger)
            {
                var ta = new OnTriggerAction(trigger);
                _triggers.Add(ta);
                return ta;
            }

            public OnTriggerAction OnTrigger(TTrigger trigger, params TState[] whenInStates)
            {
                var ta = new OnTriggerAction(trigger, whenInStates);
                _triggers.Add(ta);
                return ta;
            }

            internal List<OnTriggerAction> Triggers => _triggers;
        }
    }

    public enum MachineStatus
    {
        Stopped,
        DataLoaded,
        Started
    }
}
