using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Gtt.CodeWorks.AzureStorage
{
    public abstract class BaseTable : TableEntity, IAuditable
    {
        protected BaseTable()
        {
            Version = 1;
            SchemaVersion = 1;
            Created = ServiceClock.CurrentTime();
            Modified = ServiceClock.CurrentTime();
        }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Modified { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int Version { get; set; }
        public int SchemaVersion { get; set; }
    }
}