using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace IdentitySample.Authentication.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}
