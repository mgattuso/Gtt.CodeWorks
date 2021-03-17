using Gtt.CodeWorks.AzureStorage;

namespace Gtt.CodeWorks.StateMachines.AzureStorage
{
    public class StateDataTable : BaseTable
    {
        // PARTITION = MACHINE+IDENTIFIER
        // KEY SEQUENCE NUMBER OR 'current' FOR Current record marker
        public string MachineName { get; set; }
        public long SequenceNumber { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Trigger { get; set; }
        public bool IsReentry { get; set; }
        public long ContentLength { get; set; }
        public string SerializedState { get; set; }
        public string CorrelationId { get; set; }
        public string UserIdentifier { get; set; }
        public string Username { get; set; }
        public string UpstreamPersist { get; set; }
    }
}