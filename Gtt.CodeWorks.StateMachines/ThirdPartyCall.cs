using Stateless;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Stateless.Graph;

namespace Gtt.CodeWorks.StateMachines
{
    public abstract class ThirdPartyCall<TRequest, TResponse> where TRequest : new() where TResponse : new()
    {
        private readonly IStateRepository _stateRepository;
        private readonly StateMachine<State, Trigger> _machine = new StateMachine<State, Trigger>(State.Pending);
        private DateTimeOffset _created = ServiceClock.CurrentTime();
        private DateTimeOffset _modified = ServiceClock.CurrentTime();
        private StatefulIdentifier _statefulIdentifier;
        private long _sequenceNumber = 0;
        private Guid _correlationId = default(Guid);

        protected ThirdPartyStateData Data { get; private set; } = new ThirdPartyStateData();

        private string MachineName => GetType().FullName?.Replace("+", ".") ?? GetType().Name;

        protected ThirdPartyCall(IStateRepository stateRepository)
        {
            _stateRepository = stateRepository;
            var allowManual = AllowManualOverride();

            _machine.OnTransitionCompletedAsync(async x =>
            {
                var nextSequenceNumber = await _stateRepository.StoreStateData<ThirdPartyStateData, State>(new StateDto
                {
                    Trigger = x.Trigger.ToString(),
                    Destination = x.Destination.ToString(),
                    MachineName = MachineName,
                    IsReentry = x.IsReentry,
                    Source = x.Source.ToString(),
                    Created = _created,
                    Modified = _modified,
                    SequenceNumber = _sequenceNumber,
                    Identifier = _statefulIdentifier.Identifier,
                    CorrelationId = _correlationId
                }, _sequenceNumber, Data, saveHistory: true, _statefulIdentifier.ParentIdentifier);
                _sequenceNumber = nextSequenceNumber;
                _modified = ServiceClock.CurrentTime();
            });

            _machine.Configure(State.BeforeRequest)
                .SubstateOf(State.InProcess);

            _machine.Configure(State.Pending)
                .SubstateOf(State.BeforeRequest)
                .Permit(Trigger.Verify, State.Verification);

            _machine.Configure(State.Verification)
                .SubstateOf(State.BeforeRequest)
                .OnEntryAsync(InternalVerify)
                .Permit(Trigger.ExecuteRequest, State.Requested)
                .Permit(Trigger.SuccessfulResponse, State.Completed)
                .Permit(Trigger.NoResponse, State.Unknown);

            _machine.Configure(State.Requested)
                .SubstateOf(State.BeforeRequest)
                .OnEntryAsync(InternalRequest)
                .Permit(Trigger.SuccessfulResponse, State.Completed)
                .Permit(Trigger.ErrorResponse, State.Error)
                .Permit(Trigger.NoResponse, State.Unknown);

            _machine.Configure(State.Error)
                .SubstateOf(State.AfterRequest)
                .Permit(Trigger.Reset, State.Pending);

            _machine.Configure(State.Unknown)
                .SubstateOf(State.AfterRequest)
                .Permit(Trigger.Reset, State.Pending);

            _machine.Configure(State.Completed)
                .SubstateOf(State.Terminal);

            _machine.Configure(State.Failed)
                .SubstateOf(State.Terminal);

            _machine.Configure(State.Pending).Permit(Trigger.ToFailed, State.Failed);
            _machine.Configure(State.Verification).Permit(Trigger.ToFailed, State.Failed);
            _machine.Configure(State.Error).Permit(Trigger.ToFailed, State.Failed);
            _machine.Configure(State.Unknown).Permit(Trigger.ToFailed, State.Failed);
            _machine.Configure(State.Completed).Permit(Trigger.ToFailed, State.Failed);

            if (allowManual)
            {
                _machine.Configure(State.Manual)
                    .SubstateOf(State.AfterRequest)
                    .Permit(Trigger.SuccessfulResponse, State.Completed)
                    .Permit(Trigger.ErrorResponse, State.Failed);

                _machine.Configure(State.Pending).Permit(Trigger.ToManual, State.Manual);
                _machine.Configure(State.Verification).Permit(Trigger.ToManual, State.Manual);
                _machine.Configure(State.Error).Permit(Trigger.ToManual, State.Manual);
                _machine.Configure(State.Unknown).Permit(Trigger.ToManual, State.Manual);
                _machine.Configure(State.Completed).Permit(Trigger.ToManual, State.Manual);
                _machine.Configure(State.Failed).Permit(Trigger.ToManual, State.Manual);
            }
        }

        protected abstract Task<ExecuteAttempt> Execute(TRequest request, int attempt, CancellationToken cancellationToken);
        protected abstract Task<VerificationAttempt> Verify(TRequest request, int attempt, CancellationToken cancellationToken);
        protected abstract int MaxAttempts();
        protected abstract int ExecuteCallTimeoutMs();
        protected abstract bool AllowManualOverride();
        protected abstract int DelayBetweenAttemptsMs();

        protected async Task LoadData(StatefulIdentifier identifier, Guid correlationId)
        {
            var storedData = await _stateRepository.RetrieveStateData<ThirdPartyStateData, State>(identifier.Identifier, MachineName, null, identifier.ParentIdentifier);
            if (storedData != null)
            {
                Data = storedData.Data;
                _created = storedData.StateMetaData.Created;
                _modified = storedData.StateMetaData.Modified;
                _sequenceNumber = storedData.StateMetaData.SequenceNumber;
            }
        }

        public async Task<TResponse> Process(StatefulIdentifier identifier, TRequest request, Guid correlationId)
        {
            _statefulIdentifier = identifier;
            _correlationId = correlationId;
            Data.Request = request;
            await LoadData(identifier, correlationId);
            while (!_machine.IsInState(State.Terminal) && !_machine.IsInState(State.Manual) && Data.Attempts < MaxAttempts())
            {
                if (Data.Attempts > 0)
                {
                    await Task.Delay(DelayBetweenAttemptsMs());
                }

                if (_machine.CanFire(Trigger.Verify))
                {
                    await _machine.FireAsync(Trigger.Verify);
                }

                if (_machine.CanFire(Trigger.ExecuteRequest))
                {
                    await _machine.FireAsync(Trigger.ExecuteRequest);
                }

                if (_machine.CanFire(Trigger.Reset))
                {
                    await _machine.FireAsync(Trigger.Reset);
                }

                Data.Attempts++;
            }

            if (_machine.IsInState(State.Completed))
            {
                return Data.Response;
            }

            if (AllowManualOverride())
            {
                if (_machine.CanFire(Trigger.ToManual))
                {
                    await _machine.FireAsync(Trigger.ToManual);
                }
                else
                {
                    if (_machine.CanFire(Trigger.ToFailed))
                    {
                        await _machine.FireAsync(Trigger.ToFailed);
                    }
                }
            }
            else
            {
                if (_machine.CanFire(Trigger.ToFailed))
                {
                    await _machine.FireAsync(Trigger.ToFailed);
                }
            }


            return default(TResponse);
        }

        protected async Task InternalVerify()
        {
            if (Data.Attempts == 0) return;

            int timeout = ExecuteCallTimeoutMs();
            var cancellationToken = CancellationToken.None;
            var task = Verify(Data.Request, Data.Attempts, cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) == task)
            {
                var result = await task;
                if (result.Response != null)
                {
                    Data.Response = result.Response;
                }
                switch (result.Verification)
                {
                    case ThirdPartyVerification.Unsuccessful:
                        await _machine.FireAsync(Trigger.ExecuteRequest);
                        return;

                    case ThirdPartyVerification.Successful:
                        await _machine.FireAsync(Trigger.SuccessfulResponse);
                        break;

                    case ThirdPartyVerification.ManualCheck:
                        await _machine.FireAsync(Trigger.ToManual);
                        break;
                }
            }
            else
            {
                // timeout/cancellation logic
                await _machine.FireAsync(Trigger.NoResponse);
            }
        }

        protected async Task InternalRequest()
        {
            int timeout = ExecuteCallTimeoutMs();
            var cancellationToken = CancellationToken.None;
            var task = Execute(Data.Request, Data.Attempts, cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) == task)
            {
                var result = await task;
                if (result.Response != null)
                {
                    Data.Response = result.Response;
                }
                switch (result.Status)
                {
                    case ThirdPartyResponse.Successful:
                        await _machine.FireAsync(Trigger.SuccessfulResponse);
                        break;
                    case ThirdPartyResponse.Unsuccessful:
                        await _machine.FireAsync(Trigger.ErrorResponse);
                        break;
                }
            }
            else
            {
                // timeout/cancellation logic
                await _machine.FireAsync(Trigger.NoResponse);
            }
        }

        public State CurrentState => _machine.State;

        public string Graph => Stateless.Graph.UmlDotGraph.Format(_machine.GetInfo());

        public enum ThirdPartyResponse
        {
            Successful,
            Unsuccessful
        }

        public enum ThirdPartyVerification
        {
            Successful,
            Unsuccessful,
            ManualCheck
        }

        public enum State
        {
            InProcess,
            BeforeRequest,
            Pending,
            Verification,
            Requested,
            AfterRequest,
            Manual,
            Error,
            Unknown,

            Terminal,
            Completed,
            Failed
        }

        public enum Trigger
        {
            Reset,
            Verify,
            ExecuteRequest,
            SuccessfulResponse,
            ErrorResponse,
            NoResponse,
            ToManual,
            ToFailed
        }

        public class ThirdPartyStateData : BaseStateDataModel<State>
        {
            public int Attempts { get; set; }
            public TRequest Request { get; set; }
            public TResponse Response { get; set; }
            public ThirdPartyHttpRequestData ExecuteHttpRequest { get; set; }
            public ThirdPartyHttpRequestData VerifyHttpRequest { get; set; }
            public ThirdPartyHttpResponseData ExecuteHttpResponse { get; set; }
            public ThirdPartyHttpResponseData VerifyHttpResponse { get; set; }

        }

        public class ExecuteAttempt
        {
            private ExecuteAttempt()
            {

            }

            public TResponse Response { get; private set; }
            public ThirdPartyResponse Status { get; private set; }

            public static ExecuteAttempt Successful(TResponse response)
            {
                if (response == null) throw new ArgumentNullException(nameof(response));
                return new ExecuteAttempt()
                {
                    Status = ThirdPartyResponse.Successful,
                    Response = response
                };
            }

            public static ExecuteAttempt Unsuccessful()
            {
                return new ExecuteAttempt()
                {
                    Status = ThirdPartyResponse.Unsuccessful
                };
            }
        }

        public class VerificationAttempt
        {
            private VerificationAttempt()
            {
            }

            public TResponse Response { get; private set; }
            public ThirdPartyVerification Verification { get; private set; }

            public static VerificationAttempt Successful(TResponse response)
            {
                if (response == null) throw new ArgumentNullException(nameof(response));
                return new VerificationAttempt
                {
                    Response = response,
                    Verification = ThirdPartyVerification.Successful
                };
            }

            public static VerificationAttempt Unsuccessful()
            {
                return new VerificationAttempt
                {
                    Verification = ThirdPartyVerification.Unsuccessful
                };
            }

            public static VerificationAttempt ManualCheck()
            {
                return new VerificationAttempt
                {
                    Verification = ThirdPartyVerification.ManualCheck
                };
            }
        }

        public class ThirdPartyHttpRequestData
        {
            public string Method { get; set; }
            public string Url { get; set; }
            public Dictionary<string, string[]> Headers { get; set; }
            public string Body { get; set; }
        }

        public class ThirdPartyHttpResponseData
        {
            public int StatusCode { get; set; }
            public Dictionary<string, string[]> Headers { get; set; }
            public string Body { get; set; }
        }
    }
}
