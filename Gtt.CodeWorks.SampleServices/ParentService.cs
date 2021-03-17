using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.StateMachines;
using Stateless;

namespace Gtt.CodeWorks.SampleServices
{
    public class ParentService : BaseStatefulServiceInstance<ParentRequest, ParentResponse, ParentService.State, ParentService.Trigger, ParentService.Data>
    {
        public enum State
        {
            NotStarted,
            InProgress,
            Completed
        }
        public enum Trigger
        {
            Continue
        }

        public class Data : BaseStateDataModel<State>
        {

        }

        public ParentService(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies, statefulDependencies)
        {
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        protected override void Rules(StateMachine<State, Trigger> machine)
        {
            machine.Configure(State.NotStarted).Permit(Trigger.Continue, State.InProgress);
            machine.Configure(State.InProgress).Permit(Trigger.Continue, State.Completed);
        }
    }

    public class ParentRequest : BaseStatefulRequest<ParentService.Trigger>, IHasParentIdentifier
    {
        public string ParentIdentifier { get; set; }
    }

    public class ParentResponse : BaseStatefulResponse<ParentService.State, ParentService.Trigger, ParentService.Data>, IHasParentIdentifier
    {
        public string ParentIdentifier { get; set; }
    }
}
