using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;

namespace SingLife.PolicySystem.Shared.Logging
{
    public static class UserNameLoggerConfigurationExtensions
    {
        public static LoggerConfiguration WithHttpUserName(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null) 
                throw new ArgumentNullException(nameof(enrichmentConfiguration));

            return enrichmentConfiguration.With<UserNameEnricher>();
        }
    }

    public class UserNameEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserNameEnricher() : this(new HttpContextAccessor())
        {
        }

        public UserNameEnricher(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var userName = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            var property = propertyFactory.CreateProperty("HttpContextUserName", userName);

            logEvent.AddOrUpdateProperty(property);
        }
    }
}
