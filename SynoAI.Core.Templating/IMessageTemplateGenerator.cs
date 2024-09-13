namespace SynoAI.Core.Templating
{
    public interface IMessageTemplateGenerator
    {
        Task<string> ProcessTemplateAsync(object data, string template);
    }
}
