using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stateless;
using Stateless.Reflection;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class BaseStatefulServiceInstance<TRequest, TResponse, TState, TTrigger, TData>
        : BaseServiceInstance<TRequest, TResponse>
        where TRequest : BaseStatefulRequest<TTrigger>, new()
        where TResponse : BaseStatefulResponse<TState, TTrigger, TData>, new()
        where TState : struct, IConvertible
        where TTrigger : struct, IConvertible
        where TData : BaseStateDataModel<TState>, new()
    {
        protected BaseStatefulServiceInstance(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies)
        {
            _stateRepository = statefulDependencies.StateRepository;

            _machine = new StateMachine<TState, TTrigger>(() => _data.State, s => _data.State = s);
            _machine.OnTransitionedAsync(OnTransitionAction);
        }

        private async Task OnTransitionAction(StateMachine<TState, TTrigger>.Transition transition)
        {
            if (Status != MachineStatus.Started)
            {
                throw new Exception("Machine must be started before it can run. Call Start()");
            }

            await StoreState(_identifier, transition.Source.ToString(), transition.Destination.ToString(),
                transition.Trigger.ToString(), transition.IsReentry);
        }

        private readonly IStateRepository _stateRepository;
        private readonly StateMachine<TState, TTrigger> _machine;
        private TData _data;
        private string _identifier;
        public long SerialNumber { get; set; }
        public MachineStatus Status { get; private set; }

        public async Task ReadState(TRequest request)
        {
            Rules(_machine);

            var result = await LoadData(request.Identifier, FullName, _data, _stateRepository);
            SerialNumber = result.Item2;
            _data = result.Item1;
            CreatedDate = result.Item3;
            ModifiedDate = result.Item4;
            if (Status == MachineStatus.Stopped)
            {
                Status = MachineStatus.DataLoaded;
            }
        }

        public override async Task<ServiceResponse<TResponse>> Execute(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken)
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

            var r = await base.Execute(request, startTime, cancellationToken);
            return r;
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
            return Task.FromResult(request.Identifier);
        }

        public override ServiceAction Action => ServiceAction.Stateful;

        protected abstract void Rules(StateMachine<TState, TTrigger> machine);
        protected async Task<(TData data, long sequenceNumber, DateTimeOffset created, DateTimeOffset modified)> LoadData(
            string identifier,
            string machineName,
            TData currentData,
            IStateRepository repository)
        {
            var stateInformation = await repository.RetrieveStateData<TData, TState>(identifier, machineName);
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

        protected override Task<ServiceResponse<TResponse>> BeforeImplementation(TRequest request, CancellationToken cancellationToken)
        {
            if (request.Trigger == null)
            {
                if (!IsNew())
                {
                    var response = new TResponse
                    {
                        Data = _data,
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
                AllowedTriggers = _machine.PermittedTriggers.ToArray(),
                ActiveStates = GetAllCurrentStates()
            };
        }

        protected Task FireAsync(TTrigger trigger)
        {
            if (!CanCallAction(trigger))
            {
                throw new BusinessLogicException($"Cannot call {trigger} on {FullName}:{_identifier}", ServiceResult.ConflictingRequest);
            }

            return _machine.FireAsync(trigger);
        }

        protected bool CanCallAction(TTrigger action)
        {
            return _machine.CanFire(action);
        }

        protected bool IsInState(TState state)
        {
            return _machine.IsInState(state);
        }

        protected TData CurrentData => _data;

        private TState[] GetAllCurrentStates()
        {
            try
            {
                List<TState> states = new List<TState>();
                StateInfo si;
                // LOAD INITIAL STATE FROM THE CURRENT STATE
                si = _machine.GetInfo().States.FirstOrDefault(x => x.UnderlyingState.Equals(_machine.State));

                if (si == null)
                {
                    states.Add(_machine.State);
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
                return new[] { _machine.State };
            }
        }

        public DateTimeOffset CreatedDate { get; private set; }
        public DateTimeOffset ModifiedDate { get; private set; }
    }

    public enum MachineStatus
    {
        Stopped,
        DataLoaded,
        Started
    }
}
