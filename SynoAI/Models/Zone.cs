namespace SynoAI.Models
{
    public class Zone
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public OverlapMode Mode { get; set; }
    }
}