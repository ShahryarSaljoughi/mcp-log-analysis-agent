using Microsoft.Extensions.Logging;

namespace Pleng.MCP.Server.Services;

internal class LogModel
{
    public LogLevel LogLevel { get; set; }
    public required string ServiceName { get; set; }
    public required string ErrorType { get; set; }
    public long? TimeoutDuration { get; set; }
    public string? TargetServer { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public required DateTimeOffset CreatedAt { get; set; }
    public override string ToString()
    {
        return $"[{LogLevel}] - {ServiceName} - {ErrorType} after {TimeoutDuration}ms on SQL-Server-01";
    }
}

