using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentitySample.Authentication;
using IdentitySample.Authentication.Entities;
using IdentitySample.BusinessLayer.Settings;
using IdentitySample.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentitySample.BusinessLayer.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly JwtSettings jwtSettings;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public IdentityService(IOptions<JwtSettings> jwtSettingsOptions, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            jwtSettings = jwtSettingsOptions.Value;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            if (request.UserName == request.Password)
            {
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, request.UserName),
                    new Claim(ClaimTypes.Role, RoleNames.Administrator),
                    new Claim(ClaimTypes.Role, RoleNames.PowerUser)
                };

                if (request.UserName == "marco")
                {
                    claims.Add(new Claim(CustomClaimTypes.ApplicationId, "42"));
                    claims.Add(new Claim(CustomClaimTypes.Age, "40"));
                }

                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey));
                var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                var jwtSecurityToken = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims,
                    DateTime.UtcNow, DateTime.UtcNow.AddDays(10), signingCredentials);

                var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                var response = new AuthResponse { AccessToken = accessToken };
                return Task.FromResult(response);
            }

            return Task.FromResult<AuthResponse>(null);
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);
            var response = new RegisterResponse
            {
                Succeeded = result.Succeeded,
                Errors = result.Errors.Select(e => e.Description)
            };

            return response;
        }
    }
}
