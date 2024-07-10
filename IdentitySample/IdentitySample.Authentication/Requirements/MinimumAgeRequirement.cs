using Microsoft.AspNetCore.Authorization;

namespace IdentitySample.Authentication.Requirements;

public class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge { get; } = minimumAge;
}
