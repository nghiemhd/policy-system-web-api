using Autofac;
using AutoMapper;
using SingLife.ULTracker.Infrastructure.Common;

namespace SingLife.ULTracker.WebAPI
{
    public partial class Startup
    {
        private void RegisterAutoMapperComponents(ContainerBuilder containerBuilder)
        {
            var configuration = CreateMapperConfiguration();

            containerBuilder
                .RegisterInstance(configuration)
                .As<MapperConfiguration>()
                .As<IConfigurationProvider>();

            containerBuilder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return c.Resolve<MapperConfiguration>().CreateMapper(context.Resolve);
            });
        }

        internal static MapperConfiguration CreateMapperConfiguration()
        {
            return MapperConfigurationFactory.CreateV7CompatibleMapperConfiguration(config =>
            {
                config.ConfigureOptionPropertyMapping();
                config.AddMaps(new[] {
                    typeof(SingLife.PolicySystem.UA.Infrastructure.Checklists.MappingProfile),
                    typeof(SingLife.PolicySystem.UA.UseCases.Policies.GetPolicySummaryQuery),
                    typeof(SingLife.PolicySystem.UA.WebApi.V1.Controllers.UAPolicyController),
                    typeof(SingLife.PolicySystem.VulEnhanced.Infrastructure.CompositionRoot.VulEnhancedModule),
                    typeof(SingLife.PolicySystem.VulEnhanced.UseCases.Policies.CreatePolicyCommand),
                    typeof(SingLife.PolicySystem.VulEnhanced.WebApi.Controllers.VulEnhancedPolicyController),
                    typeof(SingLife.ULTracker.Infrastructure.CompositionRoot.ULTrackerModule),
                    typeof(SingLife.ULTracker.UseCases.DAL.ULTrackerContext),
                    typeof(SingLife.ULTracker.WebAPI.Startup)
                });
            });
        }
    }
}