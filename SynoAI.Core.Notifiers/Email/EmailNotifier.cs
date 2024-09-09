namespace SynoAI.Core.Notifiers.Email;

public class EmailNotifier : INotifier
{
    public NotifierType Type => NotifierType.Email;

    public Task NotifyAsync()
    {
        throw new NotImplementedException();
    }
}