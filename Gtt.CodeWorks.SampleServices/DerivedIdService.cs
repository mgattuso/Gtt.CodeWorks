using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        protected override (Trigger Trigger, Func<DerivedIdRequest, string> Func)? DeriveIdentifier()
        {
            return (Trigger.AnEvent, request => request.AnEvent.Message + Guid.NewGuid());
        }
    }

    public class DerivedIdRequest : BaseStatefulRequest<DerivedIdService.Trigger>
    {
        public AnEventData AnEvent { get; set; }
        public class AnEventData
        {
            [Required]
            public string Message { get; set; }
        }
    }

    public class DerivedIdResponse : BaseStatefulResponse<DerivedIdService.State, DerivedIdService.Trigger,
        DerivedIdService.Data>
    {

    }
}
