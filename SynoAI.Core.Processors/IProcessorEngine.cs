namespace SynoAI.Core.Processors;

public interface IProcessorEngine
{
    ProcessorType Type {get;}
    Task RunAsync();
}