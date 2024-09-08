namespace SynoAI.Core.Notifiers;

public interface INotifierFactory
{
    INotifier Build();
}