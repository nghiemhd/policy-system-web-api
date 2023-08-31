using Hangfire;
using Hangfire.Storage.MySql;
using Microsoft.Extensions.Configuration;

namespace SingLife.ULTracker.WebAPI
{
    public partial class Startup
    {
        private void ConfigureHangfireClient()
        {
            GlobalConfiguration.Configuration.UseStorage(
                new MySqlStorage(
                    Configuration.GetConnectionString("JobsServerDatabase"),
                    new MySqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true,
                        TablesPrefix = "jobs-"
                    }));
        }
    }
}