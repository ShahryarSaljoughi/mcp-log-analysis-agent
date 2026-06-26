using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pleng.Agent;

internal class LogUtils
{
    public static ILoggerFactory Default;
    static LogUtils()
    {
        Default = CreateLoggerFactory();
    }

    public static ILoggerFactory CreateLoggerFactory()
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddConsole(opt => { });
            builder.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });
        });
    }
}
