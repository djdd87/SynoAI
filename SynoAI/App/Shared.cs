using System.Net.Http;

namespace SynoAI.App
{
    public static class Shared
    {
        public static IHttpClient HttpClient = new HttpClientWrapper();
    }
}
