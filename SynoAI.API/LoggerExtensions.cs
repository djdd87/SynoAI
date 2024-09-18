namespace SynoAI.API
{
    public static class LoggerExtensions
    {
        public static ILogger CreateLogger(this ILoggerFactory factory)
        {
            return factory.CreateLogger("SynoAI.API");
        }
    }
}
