using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SynoAI.App
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
        Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content);
    }
}
