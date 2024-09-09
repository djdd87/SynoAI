namespace SynoAI.Core.Notifiers.Webhook;

public class WebhookNotifier : INotifier
{
    public NotifierType Type => NotifierType.Webhook;

    public Task NotifyAsync()
    {
        throw new NotImplementedException();
    }
}