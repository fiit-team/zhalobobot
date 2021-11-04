using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Zhalobobot.Common.Helpers.Extensions;

namespace Zhalobobot.Common.Clients.Core
{
    public class ClientBase
    {
        private readonly HttpClient client;
        private readonly string serverUri;
        private readonly string pathPrefix;

        protected ClientBase(string pathPrefix, HttpClient client, string serverUri)
        {
            this.client = client;
            this.serverUri = serverUri;
            this.pathPrefix = pathPrefix.Trim('/');
        }
        
        protected RemoteProcedureCall Method(string method)
            => new($"{pathPrefix}/{method}", this);
        
        protected RemoteProcedureCall<TResult> Method<TResult>(string method)
            => new($"{pathPrefix}/{method}", this);

        private async Task<ZhalobobotResult> SendRequest(string path, object? request, CancellationToken cancellationToken)
        {
            var response = await client.PostAsJsonAsync($"{serverUri}/{path}", request, cancellationToken);

            return new ZhalobobotResult(response.IsSuccessStatusCode, response.StatusCode);
        }
        
        private async Task<ZhalobobotResult<TResult>> SendRequest<TResult>(string path, object? request, CancellationToken cancellationToken)
        {
            var response = await client.PostAsJsonAsync($"{serverUri}/{path}", request, cancellationToken);
            
            var result = await response.As<TResult>() ?? default(TResult);

            return response.IsSuccessStatusCode
                ? new ZhalobobotResult<TResult>(result ?? throw new Exception(), true, response.StatusCode)
                : new ZhalobobotResult<TResult>(default, false, response.StatusCode, response.ReasonPhrase);
        }
        
        protected class RemoteProcedureCall
        {
            private readonly string path;
            private readonly ClientBase client;

            public RemoteProcedureCall(string path, ClientBase client)
            {
                this.path = path;
                this.client = client;
            }

            public Task<ZhalobobotResult> CallAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
                => client.SendRequest(path, request, cancellationToken);

            public Task<ZhalobobotResult> CallAsync(CancellationToken cancellationToken = default)
                => client.SendRequest(path, null, cancellationToken);
        }
        
        protected class RemoteProcedureCall<TResult>
        {
            private readonly string path;
            private readonly ClientBase client;

            public RemoteProcedureCall(string path, ClientBase client)
            {
                this.path = path;
                this.client = client;
            }

            public Task<ZhalobobotResult<TResult>> CallAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
                => client.SendRequest<TResult>(path, request, cancellationToken);

            public Task<ZhalobobotResult<TResult>> CallAsync(CancellationToken cancellationToken = default)
                => client.SendRequest<TResult>(path, null, cancellationToken);
        }
    }
}