namespace SynologySurveillance.Net.Models;

public class CameraInfo
{
    public int StatusId { get; set; }
    public string Status { get; set; }
    public string DsName { get; set; }
    public string Model { get; set; }
    public string Vendor { get; set; }
    public int DsId { get; set; }
    public string Name { get; set; }
    public string Mac { get; set; }
    public string Ip { get; set; }
    public int CamId { get; set; }
    public StreamInfo StreamHigh { get; set; }
    public StreamInfo StreamMedium { get; set; }
    public StreamInfo StreamLow { get; set; }
    public int RecStatus { get; set; }
    public string UserName { get; set; }
    public string AudioCodec { get; set; }
    public bool AudioOut { get; set; }
    public string VideoCodec { get; set; }
    public string RecordPrefix { get; set; }
    public string RecordSchedule { get; set; }
    public string RotationBySpace { get; set; }
    public int RotationByDay { get; set; }
    public int Port { get; set; }
    public string PtzCapability { get; set; }
    public int DINumber { get; set; }
    public DIDOItem[] DIList { get; set; }
    public int DONumber { get; set; }
    public DIDOItem[] DOList { get; set; }
}