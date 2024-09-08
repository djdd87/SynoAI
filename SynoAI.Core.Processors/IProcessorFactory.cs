namespace SynoAI.Core.Processors;

public interface IProcessorFactory
{
    public IProcessor Build(ProcessorType processorType);
}