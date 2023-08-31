using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SingLife.ULTracker.UseCases.Auth;
using SingLife.ULTracker.UseCases.Auth.DTOs;
using SingLife.ULTracker.WebAPI.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI
{
    public partial class Startup
    {
        private void AddAuthentication(IServiceCollection services)
        {
            services
                .AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["webapi:Authority"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = Configuration["webapi:Audience"]
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var identity = context.Principal.Identity as ClaimsIdentity;
                            var userName = identity.FindFirst(ClaimTypes.Name);
                            var roles = await GetUserRolesAndPermissionsAsync(context.HttpContext, userName.Value);

                            identity.AddRolesAndPermissions(roles);
                        }
                    };
                });

            async Task<RoleDto[]> GetUserRolesAndPermissionsAsync(HttpContext context, string userName)
            {
                var mediator = context.RequestServices.GetService<IMediator>();

                RoleDto[] roleDtos;

                var cachedQuery = new GetCachedUserRolesAndPermissionsQuery { UserName = userName };
                roleDtos = await mediator.Send(cachedQuery);

                if (roleDtos == null || !roleDtos.Any())
                {
                    var query = new GetUserRolesAndPermissionsQuery { UserName = userName };
                    roleDtos = await mediator.Send(query);

                    var command = new UpdateUserRolesAndPermissionsCacheCommand
                    {
                        UserName = userName,
                        Roles = roleDtos
                    };
                    await mediator.Send(command);
                }

                return roleDtos;
            }
        }
    }
}