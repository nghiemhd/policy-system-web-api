using Microsoft.Extensions.Configuration;
using SingLife.ULTracker.Shared;

namespace SingLife.PolicySystem.Shared.Configuration
{ 
    internal static class ApplicationInitialization
    {
        public static void Initialize(IConfiguration configuration)
        {
            InitializeProducts(configuration);
        }

        private static void InitializeProducts(IConfiguration configuration)
        {
            var ul = configuration["Product:UL"];
            var ua = configuration["Product:UA"];
            var vul = configuration["Product:VUL"];
            var vulEnhanced = configuration["Product:VULEnhanced"];
            var ulpb = configuration["Product:ULPB"];
            var ppli = configuration["Product:PPLI"];

            Products.Initialize(ul, ua, vul, vulEnhanced, ulpb, ppli);
        }
    }
}
