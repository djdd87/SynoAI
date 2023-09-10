using Microsoft.Extensions.Logging;

namespace SynoAI
{
    /// <summary>
    /// The main program class responsible for starting the SynoAI application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point of the application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new TimestampLoggerProvider());
            });

            CreateHostBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders(); // Remove any default log providers
                })
                .Build()
                .Run();

        }
        /// <summary>
        /// Main method to create and configure the host for the SynoAI application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>An <see cref="IHostBuilder"/> that configures the host for the application.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
