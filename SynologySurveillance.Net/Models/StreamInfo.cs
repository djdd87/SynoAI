namespace SynologySurveillance.Net.Models;

public class StreamInfo
{
    public string Resolution { get; set; }
    public string Quality { get; set; }
    public int Fps { get; set; }
    public string ConstantBitrate { get; set; }
    public int BitrateCtrl { get; set; }
}