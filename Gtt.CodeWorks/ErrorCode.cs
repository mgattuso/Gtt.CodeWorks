using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks
{
    public class ErrorCodeData
    {
        public ErrorCodeData(int errorCode, string description, string service)
        {
            ErrorCode = errorCode;
            Description = description;
            Service = service;
        }

        public int ErrorCode { get; }
        public string Description { get; }
        public string Service { get; }

        public override bool Equals(object obj)
        {
            if (obj is ErrorCodeData ecd)
            {
                return ecd.ErrorCode == ErrorCode && ecd.Service == Service;
            }

            return false;
        }

        protected bool Equals(ErrorCodeData other)
        {
            return ErrorCode == other.ErrorCode && Description == other.Description && Service == other.Service;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ErrorCode;
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Service != null ? Service.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
