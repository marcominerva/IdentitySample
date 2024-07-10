using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityClient;

public static class IdentityClientExtensions
{
    public static IServiceCollection AddIdentityClient(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.Get<JwtSettings>();

        services.AddHttpContextAccessor();

        services.AddHttpClient<IAuthorizationHandler, ValidateTokenHandler>(client =>
        {
            client.BaseAddress = new Uri(jwtSettings.AuthorizationServerUrl);
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
                RequireExpirationTime = false,
                SignatureValidator = delegate (string token, TokenValidationParameters _)
                {
                    var jwt = new JsonWebToken(token);
                    return jwt;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
            policyBuilder.Requirements.Add(new ValidTokenRequirement());

            options.FallbackPolicy = options.DefaultPolicy = policyBuilder.Build();
        });

        return services;
    }
}
