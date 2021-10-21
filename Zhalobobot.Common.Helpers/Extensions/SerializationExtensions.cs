using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class SerializationExtensions
    {
        public static ByteArrayContent SerializeToJsonContent(this object obj, string contentType = "application/json")
        {
            var json = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(bytes);
            
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            
            return content;
        }
    }
}