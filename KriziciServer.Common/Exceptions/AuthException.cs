using System;

namespace KriziciServer.Common.Exceptions
{
    public class AuthException : Exception
    {
        public AuthException(string code)
        {
            Code = code;
        }

        public string Code { get; }
    }
}