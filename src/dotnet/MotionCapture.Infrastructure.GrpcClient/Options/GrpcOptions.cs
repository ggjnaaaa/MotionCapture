namespace MotionCapture.Infrastructure.GrpcClient.Options;

public class GrpcClientConfig
{
    public string? ClientType { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public List<string> RetryableStatuses { get; set; } = new() { "Unavailable" };
    public int TimeoutSec { get; set; } = 10;
    public string? Address { get; set; }
}