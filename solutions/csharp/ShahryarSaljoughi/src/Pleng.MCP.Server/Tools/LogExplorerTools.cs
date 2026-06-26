using ModelContextProtocol.Server;
using Pleng.MCP.Server.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using ModelContextProtocol;

namespace Pleng.MCP.Server.Tools;

[McpServerToolType]
internal class LogExplorerTools
{
    private readonly ILogStorageAdapter _logStorageAdapter;

    public LogExplorerTools(ILogStorageAdapter logAdapter)
    {
        this._logStorageAdapter = logAdapter;
    }

    [McpServerTool(Destructive = false, Idempotent = true, OutputSchemaType = typeof(string[]))]
    [Description("returns list of services for whome logs are available.")]
    public string[] GetServiceNames()
    {
        return _logStorageAdapter.GetRegisteredServices();
    }

    [McpServerTool(Destructive = false, Idempotent = true, OutputSchemaType = typeof(string[]))]
    [Description("Returns application logs for a specified service since some minutes ago.")]
    public async Task<string[]> GetLogsAsync(
        [Description("name of the service whose logs are requested")] string serviceName, 
        [Description("number of minutes ago, to start fetch logs")] int minutesAgo)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new McpException("Service name cannot be null or empty.");
        }
        else if (_logStorageAdapter.GetRegisteredServices().All(s => !s.Equals(serviceName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new McpException("Service name is not valid.");
        }
        if (minutesAgo <= 0)
        {
            throw new McpException("Minutes ago must be a positive integer.");
        }

        var logs = _logStorageAdapter.GetLogs(
            serviceName, 
            since: DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(minutesAgo)), 
            until: DateTimeOffset.UtcNow);

        return logs is {  } ? [.. logs.Select(l => l.ToString())] : [];
    }
}
