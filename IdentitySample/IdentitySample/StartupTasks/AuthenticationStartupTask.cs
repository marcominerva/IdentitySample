using IdentitySample.Authentication;
using IdentitySample.Authentication.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentitySample.StartupTasks;

public class AuthenticationStartupTask(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var roleNames = new string[] { RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User };

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var administratorUser = new ApplicationUser
        {
            UserName = "marco.minerva@gmail.com",
            Email = "marco.minerva@gmail.com",
            FirstName = "Marco",
            LastName = "Minerva"
        };

        await CheckCreateUserAsync(administratorUser, "Taggia42!", RoleNames.Administrator, RoleNames.User);

        async Task CheckCreateUserAsync(ApplicationUser user, string password, params string[] roles)
        {
            var dbUser = await userManager.FindByEmailAsync(user.Email);
            if (dbUser == null)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, roles);
                }
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
