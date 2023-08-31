using Serilog.Events;
using SingLife.PolicySystem.Shared.Audit;
using System;

namespace SingLife.ULTracker
{
    internal sealed class SerilogAuditLogger : IAuditLogger
    {
        void IAuditLogger.Log(AuditLogLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Serilog.Log.Logger?.Write(
                ConvertToSerilogLevel(level),
                exception, messageTemplate, propertyValues);
        }

        private LogEventLevel ConvertToSerilogLevel(AuditLogLevel level)
        {
            switch (level)
            {
                case AuditLogLevel.Verbose:
                    return LogEventLevel.Verbose;

                case AuditLogLevel.Debug:
                    return LogEventLevel.Debug;

                case AuditLogLevel.Warning:
                    return LogEventLevel.Warning;

                case AuditLogLevel.Error:
                    return LogEventLevel.Error;

                case AuditLogLevel.Fatal:
                    return LogEventLevel.Fatal;

                case AuditLogLevel.Information:
                default:
                    return LogEventLevel.Information;
            }
        }
    }
}