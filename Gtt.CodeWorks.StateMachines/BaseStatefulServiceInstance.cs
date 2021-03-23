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
using Microsoft.Extensions.Logging;
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
        static BaseStatefulServiceInstance()
        {
            var properties = typeof(TRequest).GetTypeInfo().DeclaredProperties.ToArray();
            List<PropertyInfo> unexpected = new List<PropertyInfo>();
            foreach (var p in properties)
            {
                if (p.Name.Equals("debug", StringComparison.InvariantCultureIgnoreCase))
                {
                    _debugProperty = p;
                    continue;
                }

                if (p.Name.Equals("parentIdentifier", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (typeof(TRequest).GetInterfaces().Contains(typeof(IHasParentIdentifier)))
                    {
                        _parentIdentifierProperty = p;
                        continue;
                    }
                }

                bool found = false;
                foreach (TTrigger trigger in Enum.GetValues(typeof(TTrigger)))
                {
                    if (p.Name.Equals(trigger.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        _triggerProperties[trigger] = p;
                        found = true;
                    }
                }

                if (!found)
                {
                    unexpected.Add(p);
                }
            }

            _unexpectedProperties = unexpected.ToArray();
        }

        private static readonly PropertyInfo[] _unexpectedProperties;
        private static readonly Dictionary<TTrigger, PropertyInfo> _triggerProperties = new Dictionary<TTrigger, PropertyInfo>();
        private static PropertyInfo _debugProperty;
        private static PropertyInfo _parentIdentifierProperty;

        protected BaseStatefulServiceInstance(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies)
        {
            _stateRepository = statefulDependencies.StateRepository;
            _currentEnvironment = coreDependencies.EnvironmentResolver.Environment;
            CreatedDate = ServiceClock.CurrentTime();
            ModifiedDate = CreatedDate;

            var existingValidation = PipeLine.FirstOrDefault(x => typeof(ValidationMiddleware) == x.GetType());
            var idx = PipeLine.IndexOf(existingValidation);
            PipeLine.RemoveAt(idx);
            PipeLine.Insert(idx, new StateMachineValidatorMiddleware<TTrigger>(coreDependencies.RequestValidator));

            _data = new TData();
            Machine = new StateMachine<TState, TTrigger>(() => _data.State, s => _data.State = s);
            Machine.OnTransitionCompletedAsync(OnTransitionAction);
        }

        private readonly RegistrationFactory _registrationFactory = new RegistrationFactory();
        private int? _forceErrorCode = null;
        private ErrorAction? _forceErrorAction = null;
        private readonly CodeWorksEnvironment _currentEnvironment;

        private void SetupParameterData()
        {
            foreach (TTrigger trigger in Enum.GetValues(typeof(TTrigger)))
            {
                if (_triggerProperties.ContainsKey(trigger))
                {
                    Machine.SetTriggerParameters((TTrigger)trigger, typeof(TRequest), _triggerProperties[trigger].PropertyType);
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
                throw new BusinessLogicException($"The {request.Trigger} trigger is not permitted.", $"Cannot call {request.Trigger} on {FullName}:{_identifier}", ServiceResult.ConflictingRequest);
            }

            var hasKey = _triggerProperties.ContainsKey(request.Trigger.Value);
            var p = _triggerProperties.GetValueOrDefault(request.Trigger.Value);
            if (p != null)
            {
                var data = p.GetValue(request);

                if (hasKey && data == null)
                {
                    return ValidationError(p.Name, $"{p.Name} payload is required");
                }

                var registrationResponse = await ExecuteRegistrations(request, cancellationToken);
                if (registrationResponse != null) return registrationResponse;

                if (_forceErrorAction == null || _forceErrorAction == ErrorAction.AllowTrigger)
                {
                    await Machine.FireAsync(
                        new StateMachine<TState, TTrigger>.TriggerWithParameters<TRequest, object>(request.Trigger.Value), request, data);
                }
            }
            else
            {
                var registrationResponse = await ExecuteRegistrations(request, cancellationToken);
                if (registrationResponse != null) return registrationResponse;

                if (_forceErrorAction == null || _forceErrorAction == ErrorAction.AllowTrigger)
                {
                    await Machine.FireAsync(
                        new StateMachine<TState, TTrigger>.TriggerWithParameters<TRequest>(request.Trigger.Value),
                        request);
                }
            }

            var response = new TResponse();
            ServiceResult result = ServiceResult.Successful;

            ErrorCodeData errorData = null;

            if (_forceErrorCode != null)
            {
                result = ServiceResult.ValidationError;
                errorData = GetErrorData(_forceErrorCode.Value);
                if (errorData == null)
                {
                    Debug.Assert(_forceErrorCode != null, nameof(_forceErrorCode) + " != null");
                    return ErrorCode(_forceErrorCode.Value);
                }
            }

            Dictionary<int, string> errorDictionary = null;
            if (errorData != null)
            {
                errorDictionary = new Dictionary<int, string> { [errorData.ErrorCode] = errorData.Description };
            }

            response.Model = CurrentData;
            response.StateMachine = GetStateData();
            return new ServiceResponse<TResponse>(response, new ResponseMetaData(this, result, errorCodes: errorDictionary));
        }

        private readonly IStateRepository _stateRepository;
        private TData _data;
        private string _identifier;
        private string _parentIdentifier;
        public long SerialNumber { get; set; }
        public MachineStatus Status { get; private set; }

        public async Task ReadState(TRequest request)
        {
            var result = await LoadData(request.Identifier, FullName, _data, _stateRepository, request?.Get?.Version, _parentIdentifier);
            SerialNumber = result.sequenceNumber;
            if (result.data != null) _data = result.data;
            CreatedDate = result.created;
            ModifiedDate = result.modified;
            if (Status == MachineStatus.Stopped)
            {
                Status = MachineStatus.DataLoaded;
            }
        }

        public sealed override Task<ServiceResponse<TResponse>> Execute(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken)
        {
            return base.Execute(request, startTime, cancellationToken);
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
                CorrelationId = CorrelationId,
                MachineName = FullName,
                Identifier = identifier,
                Source = source,
                Destination = destination,
                Trigger = trigger,
                IsReentry = isReentry,
                CreatedBy = FullName,
                ModifiedBy = FullName,
                Created = CreatedDate,
                Username = User?.Username,
                UserIdentifier = User?.UserIdentifier,
                ParentIdentifier = _parentIdentifier
            }, SerialNumber, _data, saveHistory: SaveStateHistory, parentIdentifier: _parentIdentifier);
        }

        protected override Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken)
        {
            var key = (request?.Identifier ?? "").Trim();
            if (request is IHasParentIdentifier parent)
            {
                var parentKey = ((parent?.ParentIdentifier ?? "") + "-").Trim() + key;
                return Task.FromResult(parentKey);
            }
            else
            {

                return Task.FromResult(key);
            }

        }

        protected virtual void RegisterTriggerActions(RegistrationFactory register)
        {

        }

        protected virtual string DefineParentIdentifier(TRequest request)
        {
            return null;
        }

        public override ServiceAction Action => ServiceAction.Stateful;

        protected void SetErrorCode(int errorCode, ErrorAction errorAction)
        {
            _forceErrorCode = errorCode;
            _forceErrorAction = errorAction;
        }

        protected abstract void Rules(StateMachine<TState, TTrigger> machine);
        protected async Task<(TData data, long sequenceNumber, DateTimeOffset created, DateTimeOffset modified)> LoadData(
            string identifier,
            string machineName,
            TData currentData,
            IStateRepository repository,
            long? version,
            string parentIdentifier)
        {
            var stateInformation = await repository.RetrieveStateData<TData, TState>(identifier, machineName, version, parentIdentifier);
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

        protected virtual void ModifyResponse(ServiceResponse<TResponse> response)
        {

        }

        protected sealed override void BeforeResponse(ServiceResponse<TResponse> response)
        {
            if (response.Data == null)
            {
                response.Data = new TResponse();
            }

            if (response.Data is IHasParentIdentifier pr)
            {
                pr.ParentIdentifier = _parentIdentifier;
            }

            response.Data.Model = CurrentData;
            response.Data.StateMachine = GetStateData();
            if (response.MetaData.Result == ServiceResult.ResourceNotFound)
            {
                response.Data = null;
            }
            ModifyResponse(response);
            base.BeforeResponse(response);
        }

        protected override async Task<ServiceResponse<TResponse>> BeforeImplementation(TRequest request, CancellationToken cancellationToken)
        {
            if (_unexpectedProperties != null && _unexpectedProperties.Any())
            {
                throw new Exception($"Unexpected properties found on {request.GetType().Name}. Unexpected: {string.Join(",", _unexpectedProperties.Select(x => x.Name))}");
            }
            try
            {
                if (_currentEnvironment == CodeWorksEnvironment.Production && _debugProperty != null &&
                    !_debugProperty.PropertyType.IsValueType && _debugProperty.CanWrite)
                {
                    _debugProperty.SetValue(request, null);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"Cannot remove debug data for {request.CorrelationId}");
            }

            try
            {
                if (_parentIdentifierProperty != null && !(request is IHasParentIdentifier))
                {
                    throw new Exception($"Unexpected properties found on {request.GetType().Name}. " +
                                        $"Unexpected property found. To add a property called ParentIdentifier " +
                                        $"this request type (and response) should implement IHasParentIdentifier");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"Cannot set parent Identifier for {request.CorrelationId}");
            }


            _identifier = request.Identifier;

            if (request is IHasParentIdentifier pr && !string.IsNullOrWhiteSpace(pr.ParentIdentifier))
            {
                _parentIdentifier = pr.ParentIdentifier;
            }

            var derivedIdFunction = DeriveIdentifier();
            if (derivedIdFunction != null && request.Trigger != null && request.Trigger.Equals(derivedIdFunction.Value.Trigger))
            {
                try
                {
                    string derivedIdentifier = derivedIdFunction.Value.Func(request);
                    if (!string.IsNullOrWhiteSpace(derivedIdentifier))
                    {
                        if (!string.IsNullOrWhiteSpace(request.Identifier))
                        {
                            Logger.LogInformation($"Request Identifier {request.Identifier} is being replaced by derived identifier {derivedIdentifier} for request correlationId: {request.CorrelationId}");
                        }

                        _identifier = derivedIdentifier;
                        request.Identifier = derivedIdentifier;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Could not derive identifier for correlationId:{request.CorrelationId}");

                    return new ServiceResponse<TResponse>(
                        default(TResponse),
                        new ResponseMetaData(this, ServiceResult.PermanentError,
                           exceptionMessage: "Could not derive identifier from provided data"));
                }
            }

            if (string.IsNullOrWhiteSpace(_identifier))
            {
                var idRequired = ValidationError("Identifier", "The Identifier field is required");
                return idRequired;
            }

            try
            {

                var pid = DefineParentIdentifier(request);
                if (!string.IsNullOrWhiteSpace(pid))
                {
                    _parentIdentifier = DefineParentIdentifier(request);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError("Cannot defineParentIdentifier", ex);
            }

            if (request.Trigger != null)
            {
                await Start(request);
            }
            else
            {
                await ReadState(request);
                if (Status == MachineStatus.Stopped)
                {
                    return NotFound();
                }
            }

            Rules(Machine);
            RegisterTriggerActions(_registrationFactory);
            SetupParameterData();

            if (request.Trigger == null)
            {
                if (!IsNew())
                {
                    var response = new TResponse();
                    return Successful(response);
                }

                return NotFound();
            }

            return null;
        }

        protected StateMachineData<TState, TTrigger> GetStateData()
        {
            return new StateMachineData<TState, TTrigger>
            {
                SerialNumber = SerialNumber,
                Identifier = _identifier,
                CurrentState = _data.State,
                AllowedTriggers = Machine?.PermittedTriggers?.ToArray() ?? new TTrigger[0],
                ActiveStates = GetAllCurrentStates(),
                Created = CreatedDate,
                Modified = ModifiedDate
            };
        }

        protected Task FireAsync(TTrigger trigger)
        {
            if (!CanCallAction(trigger))
            {
                throw new BusinessLogicException($"The {trigger} trigger is not permitted.", $"Cannot call {trigger} on {FullName}:{_identifier}", ServiceResult.ConflictingRequest);
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

        protected virtual bool SaveStateHistory => true;

        protected virtual (TTrigger Trigger, Func<TRequest, string> Func)? DeriveIdentifier()
        {
            return null;
        }

        protected string Identifier => _identifier;
        protected string ParentIdentifier => _parentIdentifier;

        protected TData CurrentData => _data;

        private TState[] GetAllCurrentStates()
        {
            if (Machine == null)
            {
                return new TState[0];
            }

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
            var data = _triggerProperties[request.Trigger.Value];
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
            Rules(Machine);
            var d = UmlDotGraph.Format(Machine.GetInfo());
            return (d, "text/vnd.graphviz");
        }

        public Type StateType => typeof(TState);
        public Type TriggerType => typeof(TTrigger);
        public Type DataType => typeof(TData);

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

    public enum ErrorAction
    {
        PreventTrigger,
        AllowTrigger
    }
}
