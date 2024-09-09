namespace SynologySurveillance.Net.Models;

public class DigitalOutputInfo
{
    public int DOIndex { get; set; }
    public string Name { get; set; }
    public int NormalState { get; set; }
    public bool TriggerState { get; set; }
}