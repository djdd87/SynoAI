namespace SynoAI.Core.Processors;

public interface IProcessor
{
    ProcessorType Type {get;}
    Task RunAsync();
}