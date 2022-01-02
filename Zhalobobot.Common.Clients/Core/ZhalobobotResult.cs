using System;
using System.Net;

namespace Zhalobobot.Common.Clients.Core
{
    public class ZhalobobotResult<T> : ZhalobobotResult
    {
        private readonly T result;

        public ZhalobobotResult(T result, bool isSuccessful, HttpStatusCode statusCode, string? error = null)
            : base(isSuccessful, statusCode, error)
        {
            this.result = result;
        }

        public T Result
        {
            get
            {
                EnsureSuccess();
                return result;
            }
        }
    }

    public class ZhalobobotResult
    {
        public ZhalobobotResult(bool isSuccessful, HttpStatusCode statusCode, string? error = null)
        {
            IsSuccessful = isSuccessful;
            StatusCode = statusCode;
            Error = error;
        }

        public bool IsSuccessful { get; }

        public string? Error { get; }

        public HttpStatusCode StatusCode { get; }

        protected void EnsureSuccess()
        {
            if (IsSuccessful)
                return;
        
            var errorMessage = $"Request has failed with code = {StatusCode}.";

            if (!string.IsNullOrWhiteSpace(Error))
                errorMessage += $" Server error = '{Error}'.";

            throw new Exception(errorMessage);
        }
    }
}