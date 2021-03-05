using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gtt.CodeWorks.StateMachines;
using Stateless;

namespace Gtt.CodeWorks.SampleServices
{
    public class NonSavedStateService : BaseStatefulServiceInstance<NonSavedStateRequest, NonSavedStateResponse, NonSavedStateService.State, NonSavedStateService.Trigger, NonSavedStateService.Data>
    {
        public enum State
        {
            Active
        }

        public enum Trigger
        {
            UpdateTime
        }

        public class Data : BaseStateDataModel<State>
        {
            public DateTimeOffset Now { get; set; }
        }

        public NonSavedStateService(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies, statefulDependencies)
        {
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        protected override void RegisterTriggerActions(RegistrationFactory register)
        {
            register.OnTrigger(Trigger.UpdateTime)
                .Do((request, token) =>
                {
                    CurrentData.Now = ServiceClock.CurrentTime();
                    return Task.CompletedTask;
                });
        }

        protected override void Rules(StateMachine<State, Trigger> machine)
        {
            machine.Configure(State.Active)
                .PermitReentry(Trigger.UpdateTime);
        }

        protected override bool SaveStateHistory => false;
    }

    public class NonSavedStateRequest : BaseStatefulRequest<NonSavedStateService.Trigger>
    {

    }

    public class NonSavedStateResponse : BaseStatefulResponse<NonSavedStateService.State, NonSavedStateService.Trigger, NonSavedStateService.Data>
    {

    }
}
