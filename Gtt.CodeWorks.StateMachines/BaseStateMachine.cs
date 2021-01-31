using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Stateless;
using Stateless.Graph;
using Stateless.Reflection;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class BaseStateMachine
    {
        private string _machineName;

        protected static readonly JsonSerializerSettings Settings;


        static BaseStateMachine()
        {
            Settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        protected string MachineName
        {
            get
            {
                if (_machineName == null)
                {
                    var ns = GetType().Namespace?.Split('.') ?? new string[0];
                    var nsl = ns.Length == 0 ? "" : ns[ns.Length - 1] + ".";
                    _machineName = nsl + GetType().Name;
                }

                return _machineName;
            }
        }
    }

    public abstract class BaseStateMachine<TState, TAction, TData, TReference>
        : BaseStateMachine
        where TState : struct, IConvertible
        where TAction : struct, IConvertible
        where TData : BaseStateDataModel<TState>, new()
        where TReference : BaseReferenceModel

    {
        public enum MachineStatus
        {
            Stopped,
            DataLoaded,
            Started
        }

        public MachineStatus Status { get; private set; }

        private readonly IStateRepository _stateRepository;
        private readonly StateMachine<TState, TAction> _machine;

        protected BaseStateMachine(TReference reference, IStateRepository stateRepository, ILogger logger) : this(null,
            reference, stateRepository, logger)
        {
        }

        protected BaseStateMachine(TData data, TReference reference, IStateRepository stateRepository, ILogger logger)
        {
            Data = data;
            Reference = reference;
            Logger = logger;
            _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
            _machine = new StateMachine<TState, TAction>(() => Data.State, s => Data.State = s);
            _machine.OnTransitionedAsync(OnTransitionAction);
        }

        public async Task<BaseStateMachine<TState, TAction, TData, TReference>> ReadState()
        {
            Rules(_machine);

            var result = await LoadData(Reference.Identifier, MachineName, Data, _stateRepository);
            SerialNumber = result.Item2;
            Data = result.Item1;
            Created = result.Item3;
            Modified = result.Item4;
            if (Status == MachineStatus.Stopped)
            {
                Status = MachineStatus.DataLoaded;
            }

            return this;
        }


        public async Task<BaseStateMachine<TState, TAction, TData, TReference>> Start()
        {
            if (Status == MachineStatus.Started)
            {
                throw new Exception($"Machine {MachineName}:{Reference.Identifier} is already started");
            }

            if (Status == MachineStatus.Stopped)
            {
                await ReadState();
            }

            if (IsNew())
            {
                await StoreState("", Data.State.ToString(), "MachineStart", false);
            }

            Status = MachineStatus.Started;
            return this;
        }

        protected Task FireAsync(TAction action)
        {
            return _machine.FireAsync(action);
        }

        protected abstract void Rules(StateMachine<TState, TAction> machine);

        protected TData Data { get; set; }
        protected TReference Reference { get; }
        protected ILogger Logger { get; }

        public TReference GetReferenceData()
        {
            return Reference;
        }

        private async Task OnTransitionAction(StateMachine<TState, TAction>.Transition transition)
        {
            if (Status != MachineStatus.Started)
            {
                throw new Exception("Machine must be started before it can run. Call Start()");
            }

            await StoreState(transition.Source.ToString(), transition.Destination.ToString(),
                transition.Trigger.ToString(), transition.IsReentry);
        }

        private async Task StoreState(string source, string destination, string trigger, bool isReentry)
        {
            var serializedState = JsonConvert.SerializeObject(Data, Settings);
            SerialNumber = await _stateRepository.StoreStateData(new StateDto
            {
                MachineName = MachineName,
                Identifier = Reference.Identifier,
                SerializedState = serializedState,
                Source = source,
                Destination = destination,
                Trigger = trigger,
                IsReentry = isReentry,
                CreatedBy = MachineName,
                ModifiedBy = MachineName,
                Created = Created
            }, SerialNumber);
        }

        protected static async Task<Tuple<TData, long, DateTimeOffset, DateTimeOffset>> LoadData(string identifier,
            string machineName, TData currentData, IStateRepository repository)
        {
            var stateDto = await repository.RetrieveStateData(identifier, machineName);
            if (stateDto == null)
            {
                return Tuple.Create(currentData ?? new TData(), 0L, ServiceClock.CurrentTime(),
                    ServiceClock.CurrentTime());
            }

            var data = JsonConvert.DeserializeObject<TData>(stateDto.SerializedState, Settings);
            return Tuple.Create(data, stateDto.SequenceNumber, stateDto.Created, stateDto.Modified);
        }

        public bool IsNew()
        {
            return SerialNumber == 0;
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }

        public TData CurrentState => Data;
        public TAction[] Actions => _machine.PermittedTriggers.Select(x => x).ToArray();
        public TState[] States => GetAllCurrentStates();
        public long SerialNumber { get; set; }
        public DateTimeOffset Created { get; set; } = ServiceClock.CurrentTime();
        public DateTimeOffset Modified { get; set; } = ServiceClock.CurrentTime();

        public bool CanCallAction(TAction action)
        {
            return _machine.CanFire(action);
        }

        public bool IsInState(TState state)
        {
            return _machine.IsInState(state);
        }

        public T AsStateData<T>()
            where T : IStateData<TState, TAction, TData>, new()
        {
            var n = new T
            {
                SerialNumber = SerialNumber,
                CreatedAgo = (DateTime.UtcNow - Created).TotalSeconds,
                ModifiedAgo = (DateTime.UtcNow - Modified).TotalSeconds,
                CurrentState = CurrentState,
                Actions = Actions,
                ActiveStates = States,
                Machine = MachineName,
                InstanceIdentifier = Reference.Identifier
            };
            return n;
        }

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
                        var st = (TState) si.UnderlyingState;
                        states.Add(st);
                        si = si.Superstate;
                    } while (si != null);
                }

                return states.ToArray();
            }
            catch (Exception)
            {
                return new[] {_machine.State};
            }
        }
    }
}