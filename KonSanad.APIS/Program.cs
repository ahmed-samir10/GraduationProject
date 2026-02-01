using System;
using System.Security.Principal;
using System.Text;
using AutoMapper;
using KonSanad.APIS.Extenaction;
using KonSanad.APIS.Helpers;
using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using KonSanad.Core.IServices;
using KonSanad.Repository.Data.Identity.ctx;
using KonSanad.Repository.Data.Persistence;
using KonSanad.Repository.Repository;
using KonSanad.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddJwtAuthentication(builder.Configuration);
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
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IDonorServices, DonorServices>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
//volunteer service DI registration
builder.Services.AddScoped<IVolunteerService, VolunteerService>();


// register DI
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IVolunteerService,VolunteerServices>();
builder.Services.AddAutoMapper(typeof(MappingProfiles));

// Register DI for resolvers and AutoMapper (place before AddAutoMapper call)
builder.Services.AddTransient<CampaignUrlResolver>();
builder.Services.AddTransient<ImageUrlsResolver>(); 

// register AutoMapper profiles (will now be created by AutoMapper using reflection + DI resolvers)
builder.Services.AddAutoMapper(typeof(MappingProfiles));

// register token service
builder.Services.AddScoped<ITokenService, TokenService>();

// register ImageUrls resolver (NewProductRequestDto -> HelpOrder.ImageUrlsJson)
//builder.Services.AddTransient<AutoMapper.IValueResolver<KonSanad.Core.Dtos.NewProductRequestDto, KonSanad.Repository.Entities.HelpOrder, string?>, KonSanad.APIS.Helpers.ImageUrlsResolver>();

// identity services
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
})
       .AddRoles<IdentityRole>()
       .AddEntityFrameworkStores<AppIdentityDbContext>()
       //.AddEntityFrameworkStores<AppIdentityDbContext>()
       .AddSignInManager<SignInManager<AppUser>>()
       .AddUserManager<UserManager<AppUser>>();

var testConn = builder.Configuration.GetConnectionString("IdentityConnection");
Console.WriteLine("========================================");
Console.WriteLine($"🔗 Identity Connection String: {testConn}");
Console.WriteLine("========================================");

if (string.IsNullOrEmpty(testConn) || testConn.Contains("IdentityDb"))
{
    throw new Exception("❌ ERROR: Still using old connection string!");
}




builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
        
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] {}
        }
    });
   // c.OperationFilter<SwaggerJsonBodyOperationFilter>();
});








builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
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
    await KonSanadDbContextSeed.SeedAsync(context);
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
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KonSanad API V1");
    c.RoutePrefix = string.Empty;
});

// في الأول


// باقي الكود...

// في الـ middleware
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();