using IdentitySample.Authentication.Entities;
using IdentitySample.Authentication.Extensions;
using IdentitySample.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentitySample.Authorization.Handlers;

public class UserActiveHandler : AuthorizationHandler<UserActiveRequirement>
{
    private readonly UserManager<ApplicationUser> userManager;

    public UserActiveHandler(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.GetId();
            var user = await userManager.FindByIdAsync(userId.ToString());
            var securityStamp = context.User.GetClaimValue(ClaimTypes.SerialNumber);

            if (user != null && user.LockoutEnd.GetValueOrDefault() <= DateTimeOffset.UtcNow
                && securityStamp == user.SecurityStamp)
            {
                context.Succeed(requirement);
            }
        }
    }
}
