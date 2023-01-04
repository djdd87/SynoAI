namespace SynoAI.App
{
    internal static class Shared
    {
        public static IHttpClient HttpClient = new HttpClientWrapper();
    }
}
