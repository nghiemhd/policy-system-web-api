using Autofac;
using Autofac.Features.AttributeFilters;
using AutofacSerilogIntegration;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Reporting.Client;
using Singlife.ULTracker.WebAPI.Infrastructure;
using SingLife.PolicySystem.Shared.ApiClient;
using SingLife.PolicySystem.Shared.MediatRRegistration;
using SingLife.PolicySystem.UA.Infrastructure.CompositionRoot;
using SingLife.PolicySystem.VulEnhanced.Infrastructure.CompositionRoot;
using SingLife.ULTracker.Infrastructure.CompositionRoot;
using SingLife.ULTracker.Infrastructure.EsbIntegration;
using System;
using System.Linq;
using System.Net.Http;
using WebApiClients = Singlife.ULTracker.WebAPI.Infrastructure.WebApiClients;

namespace SingLife.ULTracker.WebAPI
{
    public partial class Startup
    {
        private void ConfigureCompositionRoot(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterModule(CreateULTrackerModule());
            containerBuilder.RegisterModule(CreateUAModule());
            containerBuilder.RegisterModule(CreateVulEnhancedModule());
            containerBuilder.RegisterModule(CreateCustomerNotificationModule());

            RegisterWebApiClients(containerBuilder);
            RegisterMediatRComponents(containerBuilder);
            RegisterAutoMapperComponents(containerBuilder);
            RegisterAspNetWebApiComponents(containerBuilder);
            RegisterHangfireComponents(containerBuilder);
            RegisterReportingServiceComponents(containerBuilder);
            RegisterEsbComponents(containerBuilder);

            containerBuilder.RegisterLogger(autowireProperties: true);
        }

        private ULTrackerModule CreateULTrackerModule()
        {
            return new ULTrackerModule
            {
                Environment = Configuration["Environment"],
                ULTrackerConnectionStringName = "ULTrackerDatabase",
                AccountValuesApiBaseAddress = Configuration["V1:AccountValuesApiClient:BaseAddress"],
                AccountValuesAsyncTimeOutMilliseconds = double.Parse(Configuration["V1:AccountValuesApiClient:AsyncTimeOutMilliseconds"]),
                ListsAndFactorsApiBaseAddress = Configuration["V1:ListsAndFactorsApiClient:BaseAddress"],
                EApplicationsModule = new EApplicationsModuleSettings
                {
                    UWMeBaseAddressWithVersion = Configuration["V1:UWMeApiClient:BaseAddress"],
                    UWMeTimeOutMilliseconds = double.Parse(Configuration["V1:UWMeApiClient:RequestTimeOutMilliseconds"]),
                    UWMeUsername = Configuration["V1:UWMeApiClient:Username"],
                    UWMePassword = Configuration["V1:UWMeApiClient:Password"]
                },
                PolicyDocumentModule = new PolicyDocumentModuleSettings
                {
                    S3BucketName = Configuration["S3BucketName"],
                    S3RegionEndpointSystemName = Configuration["S3RegionEndpointSystemName"]
                },
                AuditModule = new AuditModuleSettings
                {
                    ConnectionStringName = "AuditDatabase"
                },
                UseExternalOffsetClock = bool.Parse(Configuration["UseExternalOffsetClock"]),
                CachingModule = new CachingModuleSettings
                {
                    RedisConfigurationString = Configuration["CPASRedisConfigurationString"]
                },
                DocumentGenerationV2Module = new DocumentGenerationV2ModuleSettings
                {
                    Environment = Configuration["V2:DocumentGenerationApiClient:Environment"],
                    BaseAddress = Configuration["V2:DocumentGenerationApiClient:BaseAddress"],
                    TimeoutInMilliseconds = double.Parse(Configuration["V2:DocumentGenerationApiClient:AsyncTimeoutMilliseconds"]),
                }
            };
        }

        private UAModule CreateUAModule()
        {
            return new UAModule
            {
                Environment = Configuration["Environment"],
                S3BucketName = Configuration["S3BucketName"],
                S3RegionEndpointSystemName = Configuration["S3RegionEndpointSystemName"]
            };
        }

        private VulEnhancedModule CreateVulEnhancedModule()
        {
            return new VulEnhancedModule
            {
                QuotationEngineApiBaseAddress = Configuration["V1:QuotationEngineApiClient:BaseAddress"],
                QuotationEngineApiAsyncTimeoutMilliseconds = double.Parse(Configuration["V1:QuotationEngineApiClient:AsyncTimeoutMilliseconds"]),
                AccountValuesApiBaseAddress = Configuration["V1:AccountValuesApiClient:BaseAddress"],
                AccountValuesAsyncTimeOutMilliseconds = double.Parse(Configuration["V1:AccountValuesApiClient:AsyncTimeOutMilliseconds"]),
                Environment = Configuration["Environment"],
                S3BucketName = Configuration["S3BucketName"],
                S3RegionEndpointSystemName = Configuration["S3RegionEndpointSystemName"]
            };
        }

        private CustomerNotificationModule CreateCustomerNotificationModule()
        {
            return new CustomerNotificationModule
            {
                CustomerNotificationBaseAddressWithVersion = Configuration["V1:CustomerNotificationApiClient:BasesAddress"],
                CustomerNotificationServiceIsTurnedOn = bool.Parse(Configuration["CustomerNotificationServiceIsTurnedOn"]),
                CustomerNotificationTimeOutMilliseconds = double.Parse(Configuration["V1:CustomerNotificationApiClient:RequestTimeOutMilliseconds"]),
                SingLifeLogoUrl = Configuration["SingLifeLogoUrl"]
            };
        }

        private void RegisterWebApiClients(ContainerBuilder containerBuilder)
        {
            RegisterDocumentGenerationWebApiClient(containerBuilder);

            RegisterQuotationEngineWebApiClient(containerBuilder);
        }

        private void RegisterDocumentGenerationWebApiClient(ContainerBuilder containerBuilder)
        {
            var baseAddress = Configuration["V1:SingLifeDMSApiClient:BaseAddress"];
            var documentClientSettings = InitializeDocumentGenerationWebApiClientSettings();

            var singlifeDMSApiClient = new WebApiClient(baseAddress, documentClientSettings);
            containerBuilder.RegisterInstance(singlifeDMSApiClient)
                .Keyed<WebApiClient>(WebApiClients.SingLifeDMS);

            WebApiClientSettings InitializeDocumentGenerationWebApiClientSettings()
            {
                var userName = Configuration["V1:SingLifeDMSApiClient:Username"];
                var password = Configuration["V1:SingLifeDMSApiClient:Password"];

                return new WebApiClientSettings
                {
                    TimeoutMilliseconds = double.Parse(Configuration["V1:SingLifeDMSApiClient:AsyncTimeOutMilliseconds"]),
                    RequestAuthorizationProvider = new BasicAuthenticationHttpRequestAuthorizationProvider(userName, password)
                };
            }
        }

        private void RegisterQuotationEngineWebApiClient(ContainerBuilder containerBuilder)
        {
            var baseAddress = Configuration["V1:QuotationEngineApiClient:BaseAddress"];

            var quotationEngineApiClient = new WebApiClient(baseAddress, CreateQuotationEngineApiClientSettings());

            containerBuilder.RegisterInstance(quotationEngineApiClient)
                .Keyed<WebApiClient>(WebApiClients.QuotationEngine);

            WebApiClientSettings CreateQuotationEngineApiClientSettings()
            {
                return new WebApiClientSettings
                {
                    TimeoutMilliseconds = double.Parse(Configuration["V1:QuotationEngineApiClient:AsyncTimeoutMilliseconds"]),
                    RequestAuthorizationProvider = new NullHttpRequestAuthorizationProvider()
                };
            }
        }

        private void RegisterMediatRComponents(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterMediatR(
                new[]
                {
                    typeof(SingLife.PolicySystem.UA.UseCases.Applications.MappingProfile),
                    typeof(SingLife.PolicySystem.VulEnhanced.UseCases.Policies.MappingsProfile),
                    typeof(SingLife.ULTracker.UseCases.Customers.MappingsProfile)
                },
                new[]
                {
                    typeof(SingLife.ULTracker.Infrastructure.CompositionRoot.LoggingBehavior<,>),
                    typeof(SingLife.ULTracker.Infrastructure.CompositionRoot.AuthorizationBehavior<,>),
                    typeof(SingLife.ULTracker.Infrastructure.CompositionRoot.EnqueueMessagesForDispatchingBehavior<,>)
                });

            RegisterRequestAuthorizationServices();

            void RegisterRequestAuthorizationServices()
            {
                var ulTrackerUseCasesAssembly = typeof(SingLife.ULTracker.UseCases.Customers.MappingsProfile).Assembly;

                containerBuilder.RegisterAssemblyTypes(ulTrackerUseCasesAssembly)
                    .AsClosedTypesOf(typeof(SingLife.ULTracker.UseCases.Auth.IRequestAuthorizationService<>))
                    .AsImplementedInterfaces();
            }
        }

        private void RegisterAspNetWebApiComponents(ContainerBuilder containerBuilder)
        {
            var controllers = typeof(Startup).Assembly.GetTypes().Where(t => t.BaseType == typeof(ControllerBase)).ToArray();

            controllers.Concat(
                typeof(PolicySystem.UA.WebApi.V1.Controllers.UAPolicyController).Assembly.GetTypes()
                .Where(t => t.BaseType == typeof(ControllerBase)));

            controllers.Concat(
                typeof(PolicySystem.VulEnhanced.WebApi.Controllers.VulEnhancedPolicyController).Assembly.GetTypes()
                .Where(t => t.BaseType == typeof(ControllerBase)));

            containerBuilder.RegisterTypes(controllers).WithAttributeFiltering();
        }

        private static void RegisterHangfireComponents(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<BackgroundJobClient>().AsImplementedInterfaces().UsingConstructor(Type.EmptyTypes);
        }

        private void RegisterReportingServiceComponents(ContainerBuilder containerBuilder)
        {
            var baseAddress = Configuration["V1:ReportingClient:BaseAddress"];

            containerBuilder
                .Register(c => new ReportingService(new Uri(baseAddress)))
                .As<IReportingService>()
                .InstancePerLifetimeScope();
        }

        private void RegisterEsbComponents(ContainerBuilder containerBuilder)
        {
            RegisterEsbApiClient(containerBuilder);
            RegisterEsbAccessTokenService(containerBuilder);
        }

        private void RegisterEsbApiClient(ContainerBuilder builder)
        {
            var clientSettings = new EsbApiClientSettings
            {
                BaseAddress = Configuration["EsbApiClient:BaseAddress"],
                XApiKey = Configuration["EsbApiClient:XApiKey"],
                TimeoutMilliseconds = double.Parse(Configuration["EsbApiClient:TimeoutMilliseconds"])
            };

            builder
                .RegisterInstance(clientSettings.CreateHttpClient())
                .Keyed<HttpClient>(HttpClients.EsbIntegration);
        }

        private void RegisterEsbAccessTokenService(ContainerBuilder builder)
        {
            var authorizationClientSettings = new EsbAuthorizationClientSettings
            {
                BaseAddress = Configuration["EsbAuthorizationClient:BaseAddress"],
                Base64EncodedCredentials = Configuration["EsbAuthorizationClient:Base64EncodedCredentials"]
            };

            var accessTokenCredentials = new EsbAccessTokenCredentials
            {
                Username = Configuration["EsbAccessTokenCredentials:Username"],
                Password = Configuration["EsbAccessTokenCredentials:Password"],
                Scope = Configuration["EsbAccessTokenCredentials:Scope"]
            };

            builder
                .RegisterInstance(new EsbAccessTokenService(authorizationClientSettings.CreateHttpClient(), accessTokenCredentials))
                .As<IEsbAccessTokenService>()
                .SingleInstance();
        }
    }
}