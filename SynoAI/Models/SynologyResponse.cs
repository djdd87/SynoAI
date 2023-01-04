namespace SynoAI.Models
{
    internal class SynologyResponse<T> : SynologyResponse
    {
        public T Data { get; set; }
    }

    internal class SynologyResponse
    {
        public bool Success { get; set; }
        public SynologyError Error { get; set; }
    }
}
