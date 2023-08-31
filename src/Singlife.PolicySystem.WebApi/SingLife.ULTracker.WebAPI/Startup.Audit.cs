using Audit.Core;
using Audit.WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SingLife.PolicySystem.Shared.Audit;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using AuditConfiguration = Audit.Core.Configuration;

namespace SingLife.ULTracker.WebAPI
{
    public partial class Startup
    {
        private void ConfigureAudit(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            AuditConfiguration.Setup()
                .AddApplicationName(ApplicationSettings.ApplicationName)
                .AddAuthenticatedUser(httpContextAccessor)
                .UseWebApiCorrelatedAudit(logger: new SerilogAuditLogger());

            AuditConfiguration.Setup()
                .UseEFCorrelatedAudit(httpContextAccessor);

            AuditConfiguration.Setup()
                .UsePolicySystemJsonContractResolver(config =>
                {
                    config.IgnoreLargeBinaryObjects();
                    config.IgnoreStreamProperties();
                });

            AuditConfiguration.AddCustomAction(ActionType.OnEventSaving, scope =>
            {
                var auditAction = scope.GetWebApiAuditAction();
                if (auditAction?.ResponseStatusCode == 500
                    || auditAction?.ResponseStatusCode == 400
                    || !string.IsNullOrWhiteSpace(auditAction?.Exception))
                {
                    scope.Discard();
                }
            });

            AuditConfiguration.Setup()
                .UseMySql(config => config
                    .ConnectionString(configuration.GetConnectionString("AuditDatabase"))
                    .TableName("audit-events")
                    .IdColumnName("id")
                    .JsonColumnName("data"));
        }
    }
}
