using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Middleware;
using Gtt.CodeWorks.StateMachines.Middleware;
using Microsoft.Extensions.Logging;
using Stateless;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class BaseStatefulServiceInstance2<TRequest, TResponse, TState, TTrigger, TData>
        : BaseServiceInstance<TRequest, TResponse>, IStatefulServiceInstance
        where TRequest : BaseStatefulRequest<TTrigger>, new()
        where TResponse : BaseStatefulResponse<TState, TTrigger, TData>, new()
        where TState : struct, IConvertible
        where TTrigger : struct, IConvertible
        where TData : BaseStateDataModel<TState>, new()
    {
        protected BaseStatefulServiceInstance2(
            CoreDependencies coreDependencies,
            StatefulDependencies statefulDependencies) : base(coreDependencies)
        {
            _stateRepository = statefulDependencies.StateRepository;
            _currentEnvironment = coreDependencies.EnvironmentResolver.Environment;
            CreatedDate = ServiceClock.CurrentTime();
            ModifiedDate = CreatedDate;

            // REMOVE THE DEFAULT VALIDATION AND REPLACE WITH THE STATEFUL VALIDATION SERVICE
            var existingValidation = PipeLine.FirstOrDefault(x => typeof(ValidationMiddleware) == x.GetType());
            var idx = PipeLine.IndexOf(existingValidation);
            PipeLine.RemoveAt(idx);
            PipeLine.Insert(idx, new StateMachineValidatorMiddleware<TTrigger>(coreDependencies.RequestValidator));

            _data = new TData();
            Machine = new StateMachine<TState, TTrigger>(() => _data.State, s => _data.State = s);
            Machine.OnTransitionCompletedAsync(OnTransitionAction);
        }

        public (string diagram, string contentType) Diagram()
        {
            throw new NotImplementedException();
        }

        #region Services
        private readonly IStateRepository _stateRepository;
        #endregion

        #region public members
        public Type StateType => typeof(TState);
        public Type TriggerType => typeof(TTrigger);
        public Type DataType => typeof(TData);
        public MachineStatus Status { get; private set; }
        public long SerialNumber { get; private set; }
        public DateTimeOffset CreatedDate { get; private set; }
        public DateTimeOffset ModifiedDate { get; private set; }
        #endregion

        #region protected members
        protected StateMachine<TState, TTrigger> Machine { get; }
        #endregion

        #region private members
        private TData _data;
        #endregion

        #region abstract implementations
        protected abstract void Rules(StateMachine<TState, TTrigger> machine);
        #endregion

        private (int ErrorCode, ErrorAction Action)? _forcedError = null;

        private StatefulIdentifier _identifiers = null;
        private CodeWorksEnvironment _currentEnvironment;
        private readonly RegistrationFactory _registrationFactory = new RegistrationFactory();


        private async Task<StatefulIdentifier> GetIdentifiers(TRequest request, CancellationToken ct)
        {
            if (_identifiers != null) return _identifiers;

            var id = await DeriveIdentifier(request, ct);
            var pid = "";
            if (request is IHasParentIdentifier)
            {
                pid = await DeriveParentIdentifier(request, ct);
            }

            _identifiers = new StatefulIdentifier(id, pid);
            return _identifiers;
        }

        public async Task Start(TRequest request, CancellationToken ct)
        {
            if (Status == MachineStatus.Started)
            {
                throw new Exception($"Machine {FullName}:{request.Identifier} is already started");
            }

            if (Status == MachineStatus.Stopped)
            {
                await ReadState(request, ct);
            }

            Status = MachineStatus.Started;
        }

        public async Task ReadState(TRequest request, CancellationToken ct)
        {
            var ids = await GetIdentifiers(request, ct);
            var result = await LoadData(
                ids.Identifier,
                ids.ParentIdentifier,
                FullName,
                _data,
                _stateRepository,
                request?.Get?.Version);
            SerialNumber = result.SequenceNumber;
            if (result.Data != null) _data = result.Data;
            CreatedDate = result.Created;
            ModifiedDate = result.Modified;
            if (Status == MachineStatus.Stopped)
            {
                Status = MachineStatus.DataLoaded;
            }
        }

        protected void SetErrorCode(int errorCode, ErrorAction errorAction)
        {
            _forcedError = (errorCode, errorAction);
        }

        protected async Task<LoadDataResult> LoadData(
            string identifier,
            string parentIdentifier,
            string machineName,
            TData currentData,
            IStateRepository repository,
            long? version)
        {

            var stateInformation = await repository.RetrieveStateData<TData, TState>(_identifiers.Identifier, machineName, version, parentIdentifier);
            if (stateInformation == null)
            {
                return new LoadDataResult
                {
                    Data = currentData ?? new TData(),
                    SequenceNumber = 0L,
                    Created = ServiceClock.CurrentTime(),
                    Modified = ServiceClock.CurrentTime()
                };
            }

            return new LoadDataResult
            {
                Data = stateInformation.Data,
                SequenceNumber = stateInformation.StateMetaData.SequenceNumber,
                Created = stateInformation.StateMetaData.Created,
                Modified = stateInformation.StateMetaData.Modified
            };
        }

        protected virtual Task<string> DeriveIdentifier(TRequest request, CancellationToken ct)
        {
            return Task.FromResult(request.Identifier);
        }

        public sealed override Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken)
        {
            return base.Execute(request, cancellationToken);
        }

        protected virtual Task<string> DeriveParentIdentifier(TRequest request, CancellationToken ct)
        {
            if (request is IHasParentIdentifier pi)
            {
                return Task.FromResult(pi.ParentIdentifier);
            }
            return Task.FromResult("");
        }

        protected override async Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken)
        {
            var ids = await GetIdentifiers(request, cancellationToken);
            return ids.ConsolidatedIdentifier;
        }

        protected override Task<ServiceResponse<TResponse>> BeforeImplementation(TRequest request, CancellationToken cancellationToken)
        {
            return base.BeforeImplementation(request, cancellationToken);
        }

        protected override async Task<ServiceResponse<TResponse>> Implementation(TRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task OnTransitionAction(StateMachine<TState, TTrigger>.Transition transition)
        {
            if (Status != MachineStatus.Started)
            {
                throw new Exception("Machine must be started before it can run. Call Start()");
            }

            await StoreState(
                transition.Source.ToString(),
                transition.Destination.ToString(),
                transition.Trigger.ToString(),
                transition.IsReentry);
        }

        public bool IsNew()
        {
            return SerialNumber == 0;
        }

        protected virtual bool SaveStateHistory => true;

        private async Task StoreState(string source, string destination, string trigger, bool isReentry)
        {
            SerialNumber = await _stateRepository.StoreStateData<TData, TState>(new StateDto
            {
                CorrelationId = CorrelationId,
                MachineName = FullName,
                Identifier = _identifiers.Identifier,
                Source = source,
                Destination = destination,
                Trigger = trigger,
                IsReentry = isReentry,
                CreatedBy = FullName,
                ModifiedBy = FullName,
                Created = CreatedDate,
                Username = User?.Username,
                UserIdentifier = User?.UserIdentifier,
                ParentIdentifier = _identifiers.ParentIdentifier,
            }, SerialNumber, _data, saveHistory: SaveStateHistory, parentIdentifier: _identifiers.ParentIdentifier);
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

        public class LoadDataResult
        {
            public TData Data { get; set; }
            public long SequenceNumber { get; set; }
            public DateTimeOffset Created { get; set; }
            public DateTimeOffset Modified { get; set; }
        }
    }
}
