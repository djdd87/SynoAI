using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SynoAI.Core.Processors;

public static class ProcessorExtensions
{
    public static void RegisterProcessors(this IServiceCollection services)
    {
        // Get all types implementing INotifier from the current assembly
        var notifierTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IProcessorEngine).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        // Register each concrete notifier
        foreach (var notifierType in notifierTypes)
        {
            services.AddTransient(notifierType);
        }

        // Register all of the IProcessors as an array for injection into the factory
        services.AddTransient<IEnumerable<IProcessorEngine>>(sp =>
            notifierTypes.Select(t => (IProcessorEngine)sp.GetRequiredService(t)).ToList());

        // Register the factory
        services.AddSingleton<IProcessorFactory, ProcessorFactory>();
    }
}