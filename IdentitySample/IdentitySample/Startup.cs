using System;
using System.IO;
using System.Reflection;
using System.Text;
using IdentitySample.Authentication;
using IdentitySample.Authentication.Entities;
using IdentitySample.Authentication.Requirements;
using IdentitySample.BusinessLayer.Services;
using IdentitySample.BusinessLayer.Settings;
using IdentitySample.Services;
using IdentitySample.StartupTasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddHttpContextAccessor();

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

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            services.AddDbContext<AuthenticationDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("SqlConnection");
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<AuthenticationDbContext>()
            .AddDefaultTokenProviders();

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
            services.AddScoped<IAuthorizationHandler, UserActiveHandler>();

            services.AddAuthorization(options =>
            {
                var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
                policyBuilder.Requirements.Add(new UserActiveRequirement());
                options.FallbackPolicy = options.DefaultPolicy = policyBuilder.Build();

                //options.AddPolicy("UserActive", policy =>
                //{
                //    policy.Requirements.Add(new UserActiveRequirement());
                //});

                options.AddPolicy("SuperApplication", policy =>
                {
                    policy.RequireClaim(CustomClaimTypes.ApplicationId, "42");
                });

                options.AddPolicy("AdministratorOrPowerUser", policy =>
                {
                    policy.RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
                });

                options.AddPolicy("AtLeast18", policy =>
                {
                    policy.Requirements.Add(new MinimumAgeRequirement(18));
                });

                options.AddPolicy("AtLeast21", policy =>
                {
                    policy.Requirements.Add(new MinimumAgeRequirement(21));
                });
            });

            // Uncomment if you want to use the old password hashing format for both login and registration.
            //services.Configure<PasswordHasherOptions>(options =>
            //{
            //    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2;
            //});

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserService, HttpUserService>();
            services.AddScoped<IAuthenticatedService, AuthenticatedService>();

            services.AddHostedService<AuthenticationStartupTask>();

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
