using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SynoAI.Core.Notifiers.Discord;
using SynoAI.Core.Notifiers.Email;

namespace SynoAI.Core.Notifiers;

public static class NotifierRegistrations
{
    public static void RegisterNotifiers(this IServiceCollection services)
    {
        // Get all types implementing INotifier from the current assembly
        var notifierTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(INotifier).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        // Register each concrete notifier
        foreach (var notifierType in notifierTypes)
        {
            services.AddTransient(notifierType);
        }

        // Register all of the INotifiers as an array for injection into the factory
        services.AddTransient<IEnumerable<INotifier>>(sp =>
            notifierTypes.Select(t => (INotifier)sp.GetRequiredService(t)).ToList());
        
        // Register the factory
        services.AddSingleton<INotifierFactory, NotifierFactory>();
    }
}