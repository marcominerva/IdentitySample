using Microsoft.AspNetCore.Authorization;

namespace IdentitySample.Authentication.Requirements;

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == CustomClaimTypes.Age) &&
            int.TryParse(context.User.FindFirst(CustomClaimTypes.Age).Value, out var age) && age >= requirement.MinimumAge)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
