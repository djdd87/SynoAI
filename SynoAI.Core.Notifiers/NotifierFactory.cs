using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SynoAI.Core.Notifiers.Discord;
using SynoAI.Core.Notifiers.Email;

namespace SynoAI.Core.Notifiers;

public interface INotifierFactory
{
    INotifier Create(NotifierType notifierType);
}

public class NotifierFactory : INotifierFactory
{
    private readonly ILogger<NotifierFactory> _logger;
    private readonly Dictionary<NotifierType, INotifier> _notifiers;

    public NotifierFactory(ILogger<NotifierFactory> logger, IEnumerable<INotifier> notifiers)
    {
        _logger = logger;
        _notifiers = notifiers.ToDictionary(x => x.Type);
        _logger.LogInformation("NotifierFactory initialized with {Count} notifiers", _notifiers.Count);
    }

    public INotifier Create(NotifierType type)
    {
        if (_notifiers.TryGetValue(type, out var notifier))
        {
            _logger.LogDebug("Created of type {Type}", type);
            return notifier;
        }

        _logger.LogWarning("Attempted to create unsupported type: {Type}", type);
        throw new ArgumentException($"Unsupported type: {type}", nameof(type));
    }
}