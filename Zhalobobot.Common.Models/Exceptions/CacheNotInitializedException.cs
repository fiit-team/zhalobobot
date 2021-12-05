using System;

namespace Zhalobobot.Common.Models.Exceptions
{
    public class CacheNotInitializedException : Exception
    {
        public CacheNotInitializedException(string message)
            : base(message)
        {
        }
    }
}