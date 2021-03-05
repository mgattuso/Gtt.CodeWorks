using System;
using System.Collections.Generic;
using System.Text;
using Gtt.CodeWorks.StateMachines;
using Stateless;

namespace Gtt.CodeWorks.SampleServices
{
    public class DerivedIdService : BaseStatefulServiceInstance<DerivedIdRequest, DerivedIdResponse, DerivedIdService.State, DerivedIdService.Trigger, DerivedIdService.Data>
    {
        public enum State
        {
            Initial
        }

        public enum Trigger
        {
            AnEvent
        }

        public class Data : BaseStateDataModel<State>
        {

        }

        public DerivedIdService(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies, statefulDependencies)
        {
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        protected override void Rules(StateMachine<State, Trigger> machine)
        {
            machine.Configure(State.Initial)
                .PermitReentry(Trigger.AnEvent);
        }

        protected override string DeriveIdentifier(DerivedIdRequest request)
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class DerivedIdRequest : BaseStatefulRequest<DerivedIdService.Trigger>
    {

    }

    public class DerivedIdResponse : BaseStatefulResponse<DerivedIdService.State, DerivedIdService.Trigger,
        DerivedIdService.Data>
    {

    }
}
