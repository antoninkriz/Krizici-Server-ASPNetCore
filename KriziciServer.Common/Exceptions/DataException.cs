using System;

namespace KriziciServer.Common.Exceptions
{
    public class DataException : Exception
    {
        public DataException(string code)
        {
            Code = code;
        }

        public DataException(string code, Exception causedBy)
        {
            Code = code;
            CausedBy = causedBy;
        }

        public string Code { get; }

        public Exception CausedBy { get; }
    }
}