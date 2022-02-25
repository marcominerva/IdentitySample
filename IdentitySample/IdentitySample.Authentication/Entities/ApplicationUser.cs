using Microsoft.AspNetCore.Identity;

namespace IdentitySample.Authentication.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpirationDate { get; set; }

    public Guid? TenantId { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}
