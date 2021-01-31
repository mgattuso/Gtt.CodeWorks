using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gtt.CodeWorks.StateMachines;
using Microsoft.Extensions.Logging;
using Stateless;

namespace Gtt.CodeWorks.SampleServices
{
    public interface IStateMachineAction<in TActionRequest, TActionResponse>
    {
        Task<TActionResponse> CallAction(TActionRequest request, CancellationToken cancellationToken);
    }

    public interface IForAction<out TAction> where TAction : struct, IConvertible
    {
        TAction Action { get; }
    }

    public class AccountSmService
        : BaseStateMachine<AccountSmService.State, AccountSmService.Action, AccountSmService.DataModel, AccountSmService.ReferenceModel>
        , IStateMachineAction<AccountSmService.CloseRequest, AccountSmService.CloseResponse>
        , IStateMachineAction<AccountSmService.OpenRequest, AccountSmService.OpenResponse>

    {
        public enum State
        {
            Active,
            Closed
        }

        public enum Action
        {
            Open,
            Close
        }

        public class DataModel : BaseStateDataModel<State>
        {
            public Guid AccountIdentifier { get; set; }
        }

        public class ReferenceModel : BaseReferenceModel<Guid>
        {
            public ReferenceModel(Guid identifier, Guid correlationId) : base(identifier, correlationId)
            {
            }
        }

        public AccountSmService(ReferenceModel reference, IStateRepository stateRepository, ILogger logger) : base(reference, stateRepository, logger)
        {
        }

        public AccountSmService(DataModel data, ReferenceModel reference, IStateRepository stateRepository, ILogger logger) : base(data, reference, stateRepository, logger)
        {
        }

        protected override void Rules(StateMachine<State, Action> machine)
        {
            machine.Configure(State.Active)
                .Permit(Action.Close, State.Closed);

            machine.Configure(State.Closed)
                .Permit(Action.Open, State.Active);
        }

        public Task<CloseResponse> CallAction(CloseRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OpenResponse> CallAction(OpenRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public class CloseRequest : BaseRequest
        {
        }

        public class CloseResponse
        {
        }

        public class OpenRequest : BaseRequest
        {
        }

        public class OpenResponse
        {
        }
    }
}
