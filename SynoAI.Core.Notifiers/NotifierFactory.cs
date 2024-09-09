using Microsoft.Extensions.DependencyInjection;
using SynoAI.Core.Notifiers.Discord;
using SynoAI.Core.Notifiers.Email;

namespace SynoAI.Core.Notifiers;

public interface INotifierFactory
{
    INotifier Create(NotifierType notifierType);
}

public class NotifierFactory : INotifierFactory
{
    private readonly Dictionary<NotifierType, INotifier> _notifiers;

    public NotifierFactory(IEnumerable<INotifier> notifiers)
    {
        _notifiers = notifiers.ToDictionary(x => x.Type);
    }

    public INotifier Create(NotifierType type)
    {
        return _notifiers[type];
    }
}