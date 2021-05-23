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
        protected BaseStatefulServiceInstance(
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

        #region static constructor and members

        /// <summary>
        /// Stores the debug property if it is defined on the request object
        /// </summary>
        private static PropertyInfo _debugProperty;

        /// <summary>
        /// Stores an array of unexpected properties. An unexpected property is a property name on the request object that does
        /// not match the name of one of the triggers for the service. Only properties named after a trigger or the "debug" property
        /// are allowed.
        /// </summary>
        private static readonly PropertyInfo[] _unexpectedProperties;

        /// <summary>
        /// Stores a lookup of the associated data property for a trigger.
        /// </summary>
        private static readonly Dictionary<TTrigger, PropertyInfo> _triggerProperties = new Dictionary<TTrigger, PropertyInfo>();

        /// <summary>
        /// Stores whether the parent ID has been configured correctly.
        /// </summary>
        private static bool _hasIncorrectDefinedParentIdentifierProperty = false;

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
                    if (!typeof(TRequest).GetInterfaces().Contains(typeof(IHasParentIdentifier)))
                    {
                        _hasIncorrectDefinedParentIdentifierProperty = true;
                    }
                    continue;
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

        #endregion



        public (string diagram, string contentType) Diagram()
        {
            Rules(Machine);
            var d = UmlDotGraph.Format(Machine.GetInfo());
            return (d, "text/vnd.graphviz");
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
        protected TData CurrentData => _data;
        #endregion

        #region private members
        private TData _data;
        #endregion

        #region abstract and unimplemented virtual methods
        protected abstract void Rules(StateMachine<TState, TTrigger> machine);

        protected virtual void RegisterTriggerActions(RegistrationFactory register)
        {
        }

        protected virtual void ModifyResponse(ServiceResponse<TResponse> response)
        {

        }

        protected virtual DistributedLockStrategy LockStrategy { get; set; } = DistributedLockStrategy.InstanceStrategy;
        #endregion

        private (int ErrorCode, ErrorAction Action)? _forcedError = null;

        private StatefulIdentifier _identifiers = null;
        private CodeWorksEnvironment _currentEnvironment;
        private readonly RegistrationFactory _registrationFactory = new RegistrationFactory();

        /// <summary>
        /// Method to retrieve the current identifiers for the service implementation. Should be available whenever a sub-class
        /// accesses it.
        /// </summary>
        /// <returns></returns>
        protected Task<StatefulIdentifier> GetIdentifiers()
        {
            if (_identifiers != null)
            {
                return Task.FromResult(_identifiers);
            }

            throw new Exception("Identifiers are not available");
        }

        protected async Task<StatefulIdentifier> GetIdentifiers(TRequest request, CancellationToken ct)
        {
            if (_identifiers != null) return _identifiers;

            var derivedIds = DeriveIdentifier() ?? new DerivedIdAction[0];
            var deriveParentIds = DeriveParentIdentifier() ?? new DerivedIdAction[0];

            string id = request.Identifier;
            string parentId = "";

            // default strategy
            foreach (var derivedId in derivedIds)
            {
                if (derivedId.Trigger.Equals(request.Trigger))
                {
                    id = await derivedId.Strategy(request, ct);
                    break;
                }
            }

            if (request is IHasParentIdentifier pr)
            {
                parentId = pr.ParentIdentifier;
                foreach (var derivedId in deriveParentIds)
                {
                    if (derivedId.Trigger.Equals(request.Trigger))
                    {
                        parentId = await derivedId.Strategy(request, ct);
                        break;
                    }
                }
            }

            _identifiers = new StatefulIdentifier(id, parentId);
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

        protected virtual DerivedIdAction[] DeriveIdentifier()
        {
            return new DerivedIdAction[0];
        }

        protected virtual DerivedIdAction[] DeriveParentIdentifier()
        {
            return new DerivedIdAction[0];
        }
        public sealed override Task<ServiceResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken)
        {
            return base.Execute(request, cancellationToken);
        }

        protected sealed override void BeforeResponse(ServiceResponse<TResponse> response)
        {
            if (_identifiers == null)
                throw new NullReferenceException(
                    $"{nameof(_identifiers)} is null when it should be populated for this is ever executed");

            if (response.Data == null)
            {
                response.Data = new TResponse();
            }

            if (response.Data is IHasParentIdentifier pr)
            {
                pr.ParentIdentifier = _identifiers.ParentIdentifier;
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

        protected override async Task<string> CreateDistributedLockKey(TRequest request, CancellationToken cancellationToken)
        {
            var ids = await GetIdentifiers(request, cancellationToken);
            Logger.LogTrace($"Calling {nameof(CreateDistributedLockKey)} with LockStrategy={LockStrategy} for {request.CorrelationId}");
            switch (LockStrategy)
            {
                case DistributedLockStrategy.InstanceStrategy:
                    return ids.ConsolidatedIdentifier;

                case DistributedLockStrategy.ParentStrategy:
                    if (ids.HasParentIdentifier())
                        return ids.ParentIdentifier;

                    return ids.ConsolidatedIdentifier;
                default:
                    throw new ArgumentOutOfRangeException(nameof(LockStrategy), $"No implementation defined for LockStrategy={LockStrategy}");
            }

        }

        protected sealed override async Task<ServiceResponse<TResponse>> BeforeImplementation(TRequest request, CancellationToken cancellationToken)
        {
            // 1. handle unexpected properties
            // search for unexpected properties on the request object. Ideally this would be caught in a unit test but this
            // code will prevent a request object that has unexpected properties.
            if (_unexpectedProperties != null && _unexpectedProperties.Any())
            {
                throw new Exception($"Unexpected properties found on {request.GetType().Name}. Unexpected: {string.Join(",", _unexpectedProperties.Select(x => x.Name))}");
            }

            // 2. handle debug data
            try
            {
                // if mode is production and debug property is populated in the request then remove the debug data
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


            // 2. Check for a parentIdentifier without the IHasParentIdentifier interface
            try
            {
                if (_hasIncorrectDefinedParentIdentifierProperty)
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

            // 3. Ensure identifiers are setup
            var ids = await GetIdentifiers(request, cancellationToken);
            if (!ids?.HasValidIdentifiers() ?? false)
            {
                var idRequired = ValidationError("Identifier", "The Identifier field is required");
                return idRequired;
            }

            // 4. Check for trigger in the request. If found execute trigger otherwise just read the state machine
            if (request.Trigger != null)
            {
                await Start(request, cancellationToken);
            }
            else
            {
                await ReadState(request, cancellationToken);
                if (Status == MachineStatus.Stopped)
                {
                    return NotFound();
                }
            }

            // 5. execute the rules for the machine
            Rules(Machine);

            // 6. Register the trigger action and setup the request data for the specific trigger being requested
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

            // if not issues found then return null so that the code will continue and not shortcircuit in this method
            return null;
        }

        protected StateMachineData<TState, TTrigger> GetStateData()
        {
            return new StateMachineData<TState, TTrigger>
            {
                SerialNumber = SerialNumber,
                Identifier = _identifiers.ConsolidatedIdentifier,
                CurrentState = _data.State,
                AllowedTriggers = Machine?.PermittedTriggers?.ToArray() ?? new TTrigger[0],
                ActiveStates = GetAllCurrentStates(),
                Created = CreatedDate,
                Modified = ModifiedDate
            };
        }

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

        protected static T As<T>(StateMachine<TState, TTrigger>.Transition transition)
        {
            if (transition.Parameters == null || transition.Parameters.Length < 2)
            {
                return default(T);
            }

            var v = transition.Parameters[1];
            return (T)v;
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

        protected override async Task<ServiceResponse<TResponse>> Implementation(TRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request.Trigger != null, "request.Trigger != null");
            var ids = await GetIdentifiers(request, cancellationToken);

            // CHECK THAT THE TRIGGER CAN BE CALLED ON THE CURRENT STATE
            if (!CanCallAction(request.Trigger.Value))
            {
                throw new BusinessLogicException($"The {request.Trigger} trigger is not permitted.", $"Cannot call {request.Trigger} on {FullName}:{ids.ConsolidatedIdentifier}", ServiceResult.ConflictingRequest);
            }

            PropertyInfo triggerDataProperty = _triggerProperties.GetValueOrDefault(request.Trigger.Value);
            var triggerData = triggerDataProperty?.GetValue(request);

            // IF THE TRIGGER PROPERTY IS DEFINED BUT THE TRIGGER PAYLOAD IS NOT POPULATED THEN CREATE A VALIDATION ERROR
            if (triggerDataProperty != null && triggerData == null)
            {
                return ValidationError(triggerDataProperty.Name, $"{triggerDataProperty.Name} payload is required");
            }

            // EXECUTE REGISTRATIONS. IF ANY REGISTRATION RETURNS A RESPONSE THEN SHORT-CIRCUIT AND RETURN THE RESPONSE
            var registrationResponse = await ExecuteRegistrations(request, cancellationToken);
            if (registrationResponse != null) return registrationResponse;

            if (_forcedError == null || _forcedError?.Action == ErrorAction.AllowTrigger)
            {
                if (triggerData != null)
                {
                    await Machine.FireAsync(
                        new StateMachine<TState, TTrigger>.TriggerWithParameters<TRequest, object>(request.Trigger.Value), request, triggerData);
                }
                else
                {
                    await Machine.FireAsync(
                        new StateMachine<TState, TTrigger>.TriggerWithParameters<TRequest>(request.Trigger.Value),
                        request);
                }

            }

            var response = new TResponse();
            ServiceResult result = ServiceResult.Successful;

            ErrorCodeData errorData = null;

            if (_forcedError != null)
            {
                result = ServiceResult.ValidationError;
                errorData = GetErrorData(_forcedError.Value.ErrorCode);
                if (errorData == null)
                {
                    Debug.Assert(_forcedError != null, nameof(_forcedError) + " != null");
                    return ErrorCode(_forcedError.Value.ErrorCode);
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

        private void SetupParameterData()
        {
            // setup stateless machine trigger parameters if the trigger has associated data
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

        private async Task<ServiceResponse<TResponse>> ExecuteRegistrations(TRequest request, CancellationToken cancellationToken)
        {
            // IF NO TRIGGER PROVIDED THEN NO REGISTRATION CAN BE EXECUTED .... CONTINUE
            if (request.Trigger == null) return null;

            foreach (var trigger in _registrationFactory.Triggers)
            {
                // IF REQUEST DOESN'T MATCH THE CURRENT TRIGGER THEN CONTINUE
                if (!trigger.Trigger.Equals(request.Trigger.Value)) continue;
                // IF THE REGISTRATION IS EXPECTING A SPECIFIC STATE AND THE MACHINE ISN'T IN THAT STATE THEN CONTINUE
                if (trigger.States.Length != 0 && !trigger.States.Any(IsInState)) continue;

                Task t = trigger.Execute(request, cancellationToken);

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

            return null;
        }

        public bool IsNew()
        {
            return SerialNumber == 0;
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

        public class DerivedIdAction
        {
            public TTrigger Trigger { get; }
            public Func<TRequest, CancellationToken, Task<string>> Strategy { get; }

            public DerivedIdAction(TTrigger trigger, Func<TRequest, CancellationToken, Task<string>> strategy)
            {
                Trigger = trigger;
                Strategy = strategy;
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

        public enum DistributedLockStrategy
        {
            /// <summary>
            /// Create lock based on a unique instance of the service.
            /// </summary>
            InstanceStrategy,

            /// <summary>
            /// Create lock based on only one child instance of a parent service
            /// </summary>
            ParentStrategy
        }
    }
}
