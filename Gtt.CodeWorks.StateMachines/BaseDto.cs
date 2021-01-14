using System;

namespace Gtt.CodeWorks.StateMachines
{
    public class BaseDto : IAuditable
    {
        public BaseDto()
        {
            Created = ServiceClock.CurrentTime();
            Modified = Created;
        }

        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Modified { get; set; }
        public string ModifiedBy { get; set; }

        public void SetModified(string modifiedBy)
        {
            ModifiedBy = modifiedBy;
            Modified = DateTime.UtcNow;
        }
    }
}