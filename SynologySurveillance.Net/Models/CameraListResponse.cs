namespace SynologySurveillance.Net.Models;

public class CameraListResponse
{
    public int Total { get; set; }
    public CameraInfo[] Cameras { get; set; }
}