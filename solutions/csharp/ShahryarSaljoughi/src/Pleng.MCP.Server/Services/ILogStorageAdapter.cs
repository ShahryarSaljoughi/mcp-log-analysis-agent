using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pleng.MCP.Server.Services;

internal interface ILogStorageAdapter
{
    public LogModel[] GetLogs(string serviceName, DateTimeOffset since, DateTimeOffset until);
    string[] GetRegisteredServices();
}

internal class LogStorageAdapter : ILogStorageAdapter
{
    private LogModel[] _logs = [
            new ()
            {
                LogLevel = LogLevel.Error,
                ErrorType = "Database Connection Timedout",
                ServiceName = "Payment",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            },
            new ()
            {
                LogLevel = LogLevel.Error,
                ErrorType = "Database Connection Timedout",
                ServiceName = "Loan",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-90),
            }
            ,
            new ()
            {
                LogLevel = LogLevel.Error,
                ErrorType = "Database Connection Timedout",
                ServiceName = "CommunicationsHub",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-10),
            }
        ];
    public LogModel[] GetLogs(string serviceName, DateTimeOffset since, DateTimeOffset until)
    {
        return _logs.Where(l => l.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) && l.CreatedAt >= since && l.CreatedAt <= until).ToArray();
    }

    public string[] GetRegisteredServices()
    {
        return [.. _logs.Select(l => l.ServiceName).Distinct()];
    }
}

