using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
            Continue,
            Finalize
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
            machine.Configure(State.InProgress).Permit(Trigger.Finalize, State.Completed);
        }

        protected override DerivedIdAction[] DeriveIdentifier()
        {
            return new[]
            {
                new DerivedIdAction(Trigger.Continue, (request, ct) => Task.FromResult(Guid.NewGuid().ToString()))
            };
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
