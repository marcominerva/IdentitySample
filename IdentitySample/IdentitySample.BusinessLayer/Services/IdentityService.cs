﻿using IdentitySample.Authentication;
using IdentitySample.Authentication.Entities;
using IdentitySample.Authentication.Extensions;
using IdentitySample.Authentication.Settings;
using IdentitySample.Contracts;
using IdentitySample.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentitySample.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly JwtSettings jwtSettings;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IUserService userService;

    public IdentityService(IOptions<JwtSettings> jwtSettingsOptions,
        UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IUserService userService)
    {
        jwtSettings = jwtSettingsOptions.Value;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.userService = userService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var signInResult = await signInManager.PasswordSignInAsync(request.UserName, request.Password, false, false);
        if (!signInResult.Succeeded)
        {
            return null;
        }

        var user = await userManager.FindByNameAsync(request.UserName);
        _ = await userManager.UpdateSecurityStampAsync(user);

        var userRoles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GroupSid, user.TenantId?.ToString() ?? string.Empty),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp.ToString())
        }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role))).ToList();

        var loginResponse = CreateToken(claims);

        user.RefreshToken = loginResponse.RefreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

        _ = await userManager.UpdateAsync(user);

        return loginResponse;
    }

    public async Task<AuthResponse> ImpersonateAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.LockoutEnd.GetValueOrDefault() > DateTimeOffset.UtcNow)
        {
            return null;
        }

        _ = await userManager.UpdateSecurityStampAsync(user);
        var identity = userService.GetIdentity();

        UpdateClaim(ClaimTypes.NameIdentifier, user.Id.ToString());
        UpdateClaim(ClaimTypes.Name, user.UserName);
        UpdateClaim(ClaimTypes.GivenName, user.FirstName);
        UpdateClaim(ClaimTypes.Surname, user.LastName ?? string.Empty);
        UpdateClaim(ClaimTypes.Email, user.Email);
        UpdateClaim(ClaimTypes.GroupSid, user.TenantId?.ToString() ?? string.Empty);
        UpdateClaim(ClaimTypes.SerialNumber, user.SecurityStamp.ToString());

        var loginResponse = CreateToken(identity.Claims.ToList());

        user.RefreshToken = loginResponse.RefreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

        _ = await userManager.UpdateAsync(user);

        return loginResponse;

        void UpdateClaim(string type, string value)
        {
            var existingClaim = identity.FindFirst(type);
            if (existingClaim is not null)
            {
                identity.RemoveClaim(existingClaim);
            }

            identity.AddClaim(new Claim(type, value));
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = ValidateAccessToken(request.AccessToken);
        if (user != null)
        {
            var userId = user.GetId();
            var dbUser = await userManager.FindByIdAsync(userId.ToString());

            if (dbUser?.RefreshToken == null || dbUser?.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser?.RefreshToken != request.RefreshToken)
            {
                return null;
            }

            var loginResponse = CreateToken(user.Claims.ToList());

            dbUser.RefreshToken = loginResponse.RefreshToken;
            dbUser.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

            _ = await userManager.UpdateAsync(dbUser);

            return loginResponse;
        }

        return null;
    }

    private AuthResponse CreateToken(IList<Claim> claims)
    {
        var audienceClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud);
        _ = claims.Remove(audienceClaim);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(jwtSettings.Issuer, jwtSettings.Audience, claims,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes), signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = GenerateRefreshToken()
        };

        return response;

        static string GenerateRefreshToken()
        {
            var randomNumber = new byte[256];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }

    private ClaimsPrincipal ValidateAccessToken(string accessToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var user = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return user;
            }
        }
        catch
        {
        }

        return null;
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
        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        var response = new RegisterResponse
        {
            Succeeded = result.Succeeded,
            Errors = result.Errors.Select(e => e.Description)
        };

        return response;
    }
}
