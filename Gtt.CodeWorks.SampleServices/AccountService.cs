using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.StateMachines;
using Gtt.CodeWorks.Tokenizer;
using Stateless;

namespace Gtt.CodeWorks.SampleServices
{
    public class AccountService : BaseStatefulServiceInstance<AccountRequest, AccountResponse, AccountService.State, AccountService.Trigger, AccountService.Data>
    {
        public enum State
        {
            Pending = 0,
            Opened = 10,
            Closed = 20,
            Paused = 50,
            Transactable = 30,
            NonTransactable = 40
        }

        public enum Trigger
        {
            Open = 10,
            Reopen = 20,
            Update = 30,
            Close = 40,
            Pause = 50
        }

        public class Data : BaseStateDataModel<State>
        {
            public DateTimeOffset OpenDate { get; set; }
            public DateTimeOffset? ClosedDate { get; set; }
            public string Name { get; set; }
            public string ClosureNote { get; set; }
            public decimal InitialBalance { get; set; }
            public TokenString Ssn { get; set; }
        }

        public AccountService(CoreDependencies coreDependencies, StatefulDependencies statefulDependencies) : base(coreDependencies, statefulDependencies)
        {
        }

        protected override IDictionary<int, string> DefineErrorCodes()
        {
            return NoErrorCodes();
        }

        protected override void Rules(StateMachine<State, Trigger> machine)
        {
            machine.Configure(State.Pending)
                .SubstateOf(State.NonTransactable)
                .Permit(Trigger.Open, State.Opened);

            machine.Configure(State.Opened)
                .SubstateOf(State.Transactable)
                .Permit(Trigger.Close, State.Closed)
                .Permit(Trigger.Pause, State.Paused)
                .PermitReentry(Trigger.Update)
                .OnEntryFromAsync(Trigger.Open, x =>
                {
                    var data = As<AccountRequest.OpenData>(x);
                    CurrentData.Name = data.Name;
                    CurrentData.OpenDate = ServiceClock.CurrentTime();
                    CurrentData.Ssn = data.Ssn;
                    return Task.CompletedTask;
                }, "Open Account")
                .OnEntryFromAsync(new StateMachine<State, Trigger>.TriggerWithParameters<AccountRequest, AccountRequest.UpdateData>(Trigger.Update), (request, data) =>
                {
                    CurrentData.Name = data.Name;
                    return Task.CompletedTask;
                }, "Update Account");

            machine.Configure(State.Paused)
                .SubstateOf(State.NonTransactable)
                .Permit(Trigger.Reopen, State.Opened);

            machine.Configure(State.Closed)
                .SubstateOf(State.NonTransactable)
                .Permit(Trigger.Reopen, State.Opened);
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
            [Sensitive(Reveal.Last, lastChars: 4)]
            public TokenString Ssn { get; set; }
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
