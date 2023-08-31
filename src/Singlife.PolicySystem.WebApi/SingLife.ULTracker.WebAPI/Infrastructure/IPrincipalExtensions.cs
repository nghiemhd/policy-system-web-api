using SingLife.ULTracker.UseCases.Auth.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SingLife.ULTracker.WebAPI.Infrastructure
{
    public static class IPrincipalExtensions
    {
        private const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        private const string PermissionClaimType = "http://schemas.ultracker.singaporelife.com.sg/identity/claims/permission";

        public static void AddRolesAndPermissions(this ClaimsIdentity claimsIdentity, IEnumerable<RoleDto> roles)
        {
            RemoveAllClaims(claimsIdentity, RoleClaimType);
            RemoveAllClaims(claimsIdentity, PermissionClaimType);

            var roleClaims = roles.Select(x => new Claim(RoleClaimType, x.Name));
            var permissionClaims = roles
                .SelectMany(x => x.Permissions)
                .GroupBy(x => x.Name)
                .Select(group => new Claim(PermissionClaimType, group.Key));

            claimsIdentity.AddClaims(roleClaims);
            claimsIdentity.AddClaims(permissionClaims);
        }

        private static void RemoveAllClaims(ClaimsIdentity claimsIdentity, string claimType)
        {
            var claims = claimsIdentity.Claims.Where(x => x.Type == claimType);
            foreach (var claim in claims)
            {
                claimsIdentity.RemoveClaim(claim);
            }
        }
    }
}