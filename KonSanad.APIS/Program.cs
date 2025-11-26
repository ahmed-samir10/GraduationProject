using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Identity.ctx;
using KonSanad.Repository.Data.Persistence;
using KonSanad.Repository.Repository;
using KonSanad.Services.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Principal;
using System.Text;
using AutoMapper;
using KonSanad.APIS.Helpers;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<KonSanadDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDbContext<AppIdentityDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
});
builder.Services.AddScoped<IDonationOrderService,DonationOrderService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// identity services
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
})
       .AddRoles<IdentityRole>()
       .AddEntityFrameworkStores<AppIdentityDbContext>()
       .AddSignInManager<SignInManager<AppUser>>()
       .AddUserManager<UserManager<AppUser>>();

// register token service
builder.Services.AddScoped<ITokenService, TokenService>();

// Register AutoMapper (ensure AutoMapper.Extensions.Microsoft.DependencyInjection is installed)
builder.Services.AddAutoMapper(typeof(MappingProfiles));

// JWT auth
var jwtKey = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:ValidIssuer"];
var jwtAudience = builder.Configuration["JWT:ValidAudience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var context = services.GetRequiredService<KonSanadDbContext>();

var Identitycontext = services.GetRequiredService<AppIdentityDbContext>();
var userManager = services.GetRequiredService<UserManager<AppUser>>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

var ILoggerFactory = services.GetRequiredService<ILoggerFactory>();
try
{
    await context.Database.MigrateAsync();
    await Identitycontext.Database.MigrateAsync();
    
    await AppIdentityDbContextSeed.SeedUsersAsync(userManager);

    var rolesToEnsure = new[] { "Volunteer", "Beneficiary", "Donor" };
    foreach (var r in rolesToEnsure)
    {
        if (!await roleManager.RoleExistsAsync(r))
        {
            var rr = await roleManager.CreateAsync(new IdentityRole(r));
            if (!rr.Succeeded)
            {
                var errors = string.Join(", ", rr.Errors.Select(e => e.Description));
                var logger = ILoggerFactory.CreateLogger<Program>();
                logger.LogError("Failed to create role {Role}: {Errors}", r, errors);
            }
        }
    }
}
catch (Exception ex)
{
    var logger = ILoggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred during migration");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KonSanad API V1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
