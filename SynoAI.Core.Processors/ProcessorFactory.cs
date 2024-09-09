using Microsoft.Extensions.Logging;

namespace SynoAI.Core.Processors;

public interface IProcessorFactory
{
    public IProcessor Build(ProcessorType processorType);
}

public class ProcessorFactory : IProcessorFactory
{
    private readonly ILogger<ProcessorFactory> _logger;
    private readonly Dictionary<ProcessorType, IProcessor> _processors;

    public ProcessorFactory(ILogger<ProcessorFactory> logger, IEnumerable<IProcessor> processors)
    {
        _logger = logger;
        _processors = processors.ToDictionary(x => x.Type);
        _logger.LogInformation("ProcessorFactory initialized with {Count} processors", _processors.Count);
    }

    public IProcessor Build(ProcessorType type)
    {
        if (_processors.TryGetValue(type, out var processor))
        {
            _logger.LogDebug("Created notifier of type {Type}", type);
            return processor;
        }

        _logger.LogWarning("Attempted to create unsupported type: {Type}", type);
        throw new ArgumentException($"Unsupported type: {type}", nameof(type));
    }
}