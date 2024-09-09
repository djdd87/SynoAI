namespace SynoAI.Core.Notifiers.Discord;

public class DiscordNotifier : INotifier
{
    public NotifierType Type => NotifierType.Discord;

    public Task NotifyAsync()
    {
        throw new NotImplementedException();
    }
}
