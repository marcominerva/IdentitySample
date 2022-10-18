using IdentitySample.Authentication;
using IdentitySample.Authentication.Entities;
using IdentitySample.Authentication.Settings;
using IdentitySample.Authorization.Handlers;
using IdentitySample.Authorization.Requirements;
using IdentitySample.BusinessLayer.Services;
using IdentitySample.Contracts;
using IdentitySample.DataAccessLayer;
using IdentitySample.Services;
using IdentitySample.StartupTasks;
using IdentitySample.StorageProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
Configure(app, app.Environment);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = Configure<JwtSettings>(nameof(JwtSettings));

    services.AddControllers();
    services.AddMemoryCache();
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

    services.AddSqlServer<AuthenticationDbContext>(configuration.GetConnectionString("AuthConnection"));

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

    services.AddDbContext<DataContext>((services, options) =>
    {
        var tenantService = services.GetRequiredService<ITenantService>();
        var tenant = tenantService.GetTenant();

        options.UseSqlServer(tenant.ConnectionString);
    });

    services.AddAzureStorage((services, options) =>
    {
        var tenantService = services.GetRequiredService<ITenantService>();
        var tenant = tenantService.GetTenant();

        options.ConnectionString = tenant.StorageConnectionString;
        options.ContainerName = tenant.ContainerName ?? tenant.Id.ToString();
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
    services.AddScoped<ITenantService, TenantService>();

    services.AddScoped<IProductService, ProductService>();

    services.AddHostedService<AuthenticationStartupTask>();

    T Configure<T>(string sectionName) where T : class
    {
        var section = configuration.GetSection(sectionName);
        var settings = section.Get<T>();
        services.Configure<T>(section);

        return settings;
    }
}

void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseHttpsRedirection();

    if (env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentitySample v1"));
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}