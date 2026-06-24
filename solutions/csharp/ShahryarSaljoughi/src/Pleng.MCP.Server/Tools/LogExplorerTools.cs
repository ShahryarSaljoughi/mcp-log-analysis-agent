using ModelContextProtocol.Server;
using Pleng.MCP.Server.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;

namespace Pleng.MCP.Server.Tools;

internal class LogExplorerTools
{
    private readonly ILogStorageAdapter _logStorageAdapter;

    public LogExplorerTools(ILogStorageAdapter logAdapter)
    {
        this._logStorageAdapter = logAdapter;
    }

    [McpServerTool(Destructive = false, Idempotent = true, OutputSchemaType = typeof(string[]))]
    [Description("Returns application logs for a specified service since some minutes ago.")]
    public async Task<string[]> GetLogsAsync(
        [Description("name of the service whose logs are requested")] string serviceName, 
        [Description("number of minutes ago, to start fetch logs")] int minutesAgo)
    {
        var start = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(minutesAgo));
        var logs = _logStorageAdapter.GetLogs(serviceName, since: start, DateTimeOffset.UtcNow);
        return logs is {  } ? [.. logs.Select(l => l.ToString())] : [];
    }
}
