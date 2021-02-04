using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.StateMachines;
using Stateless;

namespace Gtt.CodeWorks.SampleServices
{
    public class AccountService : BaseStatefulServiceInstance<AccountRequest, AccountResponse, AccountService.State, AccountService.Trigger, AccountService.Data>
    {
        public enum State
        {
            Pending = 0,
            Opened = 10,
            Closed = 20
        }

        public enum Trigger
        {
            Open = 10,
            Reopen = 20,
            Update = 30,
            Close = 40
        }

        public class Data : BaseStateDataModel<State>
        {
            public DateTimeOffset OpenDate { get; set; }
            public DateTimeOffset? ClosedDate { get; set; }
            public string Name { get; set; }
            public string ClosureNote { get; set; }
            public decimal InitialBalance { get; set; }
        }

        public AccountService(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies, statefulDependencies)
        {
        }

        protected override async Task<ServiceResponse<AccountResponse>> Implementation(AccountRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request.Trigger != null, "request.Trigger != null");
            switch (request.Trigger.Value)
            {
                case Trigger.Open:
                    CurrentData.Name = request.Open.Name;
                    CurrentData.InitialBalance = request.Open.InitialBalance ?? 0;
                    CurrentData.OpenDate = DateTimeOffset.Now;
                    break;
                case Trigger.Close:
                    CurrentData.ClosureNote = request.Close.ClosureNote;
                    break;
                case Trigger.Reopen:
                    CurrentData.ClosedDate = null;
                    break;
                case Trigger.Update:
                    CurrentData.Name = request.Update.Name;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await FireAsync(request.Trigger.Value);
            var response = new AccountResponse
            {
                StateMachine = GetStateData(),
                Model = CurrentData
            };

            return Successful(response);
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        protected override void Rules(StateMachine<State, Trigger> machine)
        {
            machine.Configure(State.Pending).Permit(Trigger.Open, State.Opened);
            machine.Configure(State.Opened).Permit(Trigger.Close, State.Closed);
            machine.Configure(State.Opened).PermitReentry(Trigger.Update);
            machine.Configure(State.Closed).Permit(Trigger.Reopen, State.Opened);
        }
    }

    public class AccountRequest : BaseStatefulRequest<AccountService.Trigger>
    {
        public OpenData Open { get; set; }
        public CloseData Close { get; set; }
        public UpdateData Update { get; set; }
        public ReopenData Reopen { get; set; }

        public class OpenData
        {
            public string Name { get; set; }
            public decimal? InitialBalance { get; set; }
        }

        public class CloseData
        {
            public string ClosureNote { get; set; }
        }

        public class UpdateData
        {
            public string Name { get; set; }
        }

        public class ReopenData
        {

        }
    }

    public class AccountResponse : BaseStatefulResponse<AccountService.State, AccountService.Trigger, AccountService.Data>
    {
    }
}
