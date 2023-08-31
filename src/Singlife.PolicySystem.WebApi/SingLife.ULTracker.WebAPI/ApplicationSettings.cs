using Microsoft.Extensions.Configuration;
using ULTrackerSettingsV1 = SingLife.ULTracker.WebAPI.V1.ULTrackerSettings;

namespace SingLife.ULTracker.WebAPI
{
    public static class ApplicationSettings
    {
        public static string ApplicationName { get; private set; }

        public static ULTrackerSettingsV1 V1 { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            ApplicationName = configuration["Serilog:Properties:ApplicationName"];
            V1 = ULTrackerSettingsV1.Initialize(configuration);
        }
    }
}