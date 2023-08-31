using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using SingLife.PolicySystem.Shared.Configuration;
using SingLife.ULTracker.WebAPI.Contracts.Validators.Common;
using SingLife.ULTracker.WebAPI.Infrastructure;
using SingLife.ULTracker.WebAPI.Infrastructure.BsonFormatters;

[assembly: ApiController]

namespace SingLife.ULTracker.WebAPI
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc(options =>
                {
                    options.Filters.Add<ValidateRequestContentLengthAttribute>();
                    options.Filters.Add<UnauthorizedRequestExceptionFilterAttribute>();
                })
                .AddControllersAsServices() // we have to AddControllersAsServices so that Autofac can resolve the controller with attribute filtering.
                .AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<FileValidator>())
                .AddNewtonsoftJson(options =>
                {
                    options.UseMemberCasing(); // PascalCase

                    var serializerSettings = options.SerializerSettings;

                    ((DefaultContractResolver)serializerSettings.ContractResolver).IgnoreSerializableAttribute = true;
                    serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                    serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                })
                .AddPolicySystemBsonSerializerFormatters();

            AddCustomApiVersioning(services);

            AddCustomSwagger(services);

            services.AddFluentValidationRulesToSwagger();

            services.AddHttpContextAccessor();

            services.AddOptions();

            AddAuthentication(services);
        }

        // ConfigureContainer is where you can register things directly with Autofac.
        // This runs after ConfigureServices so the things here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac here.
            // Don't call builder.Populate(), that happens in AutofacServiceProviderFactory
            ConfigureCompositionRoot(builder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            ConfigureServiceLocator();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Write streamlined request completion events, instead of the more verbose ones from the framework.
            // To use the default framework request logging instead, remove this line and set the "Microsoft"
            // level in appsettings.json to "Information".
            app.UseSerilogRequestLogging();

            var useHttpsRedirection = Configuration.GetValue<bool>("UseHttpsRedirection");
            if (useHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapControllers()
                    .RequireAuthorization();
            });

            UseCustomSwagger(app, apiVersionDescriptionProvider);

            ConfigureAudit(Configuration, httpContextAccessor);

            ConfigureHangfireClient();

            void ConfigureServiceLocator()
            {
                var autofacContainer = app.ApplicationServices.GetAutofacRoot();

                var serviceLocator = new AutofacServiceLocator(autofacContainer);
                ServiceLocator.SetLocatorProvider(() => serviceLocator);
            }
        }
    }
}