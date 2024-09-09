namespace SynoAI.Core.Processors;

public interface IProcessorFactory
{
    public IProcessor Build(ProcessorType processorType);
}

public class ProcessorFactory : IProcessorFactory
{
    private readonly Dictionary<ProcessorType, IProcessor> _processors;

    public ProcessorFactory(IEnumerable<IProcessor> notifiers)
    {
        _processors = notifiers.ToDictionary(x=> x.Type);
    }

    public IProcessor Build(ProcessorType type)
    {
        return _processors[type];
    }
}