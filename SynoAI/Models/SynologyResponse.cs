namespace SynoAI.Models
{
    public class SynologyResponse<T> : SynologyResponse
    {
        public T Data { get; set; }
    }

    public class SynologyResponse
    {
        public bool Success { get; set; }
        public SynologyError Error { get; set; }
    }
}
