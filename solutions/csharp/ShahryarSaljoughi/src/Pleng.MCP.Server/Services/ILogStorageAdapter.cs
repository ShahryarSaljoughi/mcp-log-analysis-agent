using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pleng.MCP.Server.Services;

internal interface ILogStorageAdapter
{
    public LogModel[] GetLogs(string serviceName, DateTimeOffset since, DateTimeOffset until);
}


internal class LogModel
{
    public LogLevel LogLevel { get; set; }
    public required string ServiceName { get; set; }
    public required string ErrorType { get; set; }
    public long? TimeoutDuration { get; set; }
    public string? TargetServer { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();

    public override string ToString()
    {
        return $"[{LogLevel}] - {ServiceName} - {ErrorType} after {TimeoutDuration}ms on SQL-Server-01";
    }
}

