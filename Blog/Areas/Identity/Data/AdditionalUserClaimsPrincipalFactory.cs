using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Blog.Areas.Identity.Data;

public class AdditionalUserClaimsPrincipalFactory 
    : UserClaimsPrincipalFactory<BlogUser, IdentityRole>
{
    public AdditionalUserClaimsPrincipalFactory( 
        UserManager<BlogUser> userManager,
        RoleManager<IdentityRole> roleManager, 
        IOptions<IdentityOptions> optionsAccessor) 
        : base(userManager, roleManager, optionsAccessor)
    {}

    public override async Task<ClaimsPrincipal> CreateAsync(BlogUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        var claims = new List<Claim>();
        claims.Add(new Claim("DisplayName", user.DisplayName ?? ""));

        identity.AddClaims(claims);
        return principal;
    }
}