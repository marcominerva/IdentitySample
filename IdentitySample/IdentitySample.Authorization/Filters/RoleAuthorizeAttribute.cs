using Microsoft.AspNetCore.Authorization;

namespace IdentitySample.Authorization.Filters;

public class RoleAuthorizeAttribute : AuthorizeAttribute
{
	public RoleAuthorizeAttribute(params string[] roles)
	{
		Roles = string.Join(",", roles);
	}
}