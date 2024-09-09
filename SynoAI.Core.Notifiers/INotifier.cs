namespace SynoAI.Core.Notifiers;

public interface INotifier
{
    NotifierType Type {get;}
    Task NotifyAsync();
}