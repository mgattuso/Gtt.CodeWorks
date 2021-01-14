namespace Gtt.CodeWorks.StateMachines
{
    public class StateDto : BaseDto
    {
        public string Identifier { get; set; }
        public string SerializedState { get; set; }
        public long SequenceNumber { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Trigger { get; set; }
        public bool IsReentry { get; set; }
        public string MachineName { get; set; }
    }
}