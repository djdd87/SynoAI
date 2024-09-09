namespace SynologySurveillance.Net.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ErrorInfo? Error { get; set; }
}
