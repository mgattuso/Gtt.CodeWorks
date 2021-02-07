﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.Validation;
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

            Machine = new StateMachine<TState, TTrigger>(() => _data.State, s => _data.State = s);
            Machine.OnTransitionCompletedAsync(OnTransitionAction);
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

        private readonly IStateRepository _stateRepository;
        private TData _data;
        private string _identifier;
        public long SerialNumber { get; set; }
        public MachineStatus Status { get; private set; }

        public async Task ReadState(TRequest request)
        {
            Rules(Machine);

            var result = await LoadData(request.Identifier, FullName, _data, _stateRepository);
            SerialNumber = result.sequenceNumber;
            if (result.data != null) _data = result.data;
            CreatedDate = result.created;
            ModifiedDate = result.modified;
            if (Status == MachineStatus.Stopped)
            {
                Status = MachineStatus.DataLoaded;
            }
        }

        public override async Task<ServiceResponse<TResponse>> Execute(TRequest request, DateTimeOffset startTime, CancellationToken cancellationToken)
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
    }

    public enum MachineStatus
    {
        Stopped,
        DataLoaded,
        Started
    }
}
