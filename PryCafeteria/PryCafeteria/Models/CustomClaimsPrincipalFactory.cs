using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PryCafeteria.Models;
using System.Security.Claims;

namespace PryCafeteria.Models
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public CustomClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Agregar nombre real como claim
            if (!string.IsNullOrEmpty(user.Nombre))
                identity.AddClaim(new Claim("Nombre", user.Nombre));

            return identity;
        }
    }
}
