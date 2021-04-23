using System;
using System.Text;
using IdentitySample.Authentication;
using IdentitySample.Authentication.Requirements;
using IdentitySample.BusinessLayer.Services;
using IdentitySample.BusinessLayer.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace IdentitySample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtSettings = Configure<JwtSettings>(nameof(JwtSettings));

            services.AddControllers();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentitySample", Version = "v1" });

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Insert the Bearer Token",
                    Name = HeaderNames.Authorization,
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference= new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                });
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
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;

                options.AddPolicy("SuperApplication", policy =>
                {
                    //policy.RequireClaim(ClaimTypes.Role, RoleNames.Administrator, RoleNames.PowerUser);
                    //policy.RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
                    policy.RequireClaim(CustomClaimTypes.ApplicationId, "42");
                });

                options.AddPolicy("TaggiaUser", policy =>
                {
                    policy.RequireClaim(JwtRegisteredClaimNames.Iss, "Taggia");
                });

                options.AddPolicy("18", policy =>
                {
                    policy.Requirements.Add(new MinimumAgeRequirement(18));
                });

                options.AddPolicy("21", policy =>
                {
                    policy.Requirements.Add(new MinimumAgeRequirement(21));
                });
            });

            services.AddScoped<IIdentityService, IdentityService>();

            T Configure<T>(string sectionName) where T : class
            {
                var section = Configuration.GetSection(sectionName);
                var settings = section.Get<T>();
                services.Configure<T>(section);

                return settings;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentitySample v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
