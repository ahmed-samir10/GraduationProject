using AutoMapper;
using KonSanad.APIS.Helpers;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Security.Principal;
using System.Text;

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
//<<<<<<< HEAD
//تسجيل سيرفس المستفيد
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
//
//builder.Services.AddScoped<IDonationOrderService, DonationOrderService>();
//builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register DI for resolvers and AutoMapper (place before AddAutoMapper call)
builder.Services.AddTransient<CampaignUrlResolver>();
builder.Services.AddTransient<ImageUrlsResolver>(); // if you have this resolver
// register AutoMapper profiles (will now be created by AutoMapper using reflection + DI resolvers)
builder.Services.AddAutoMapper(typeof(MappingProfiles));

//=======
//builder.Services.AddScoped<IDonationOrderService, DonationOrderService>();
//builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
//>>>>>>> AhmedBranch
// Register campaign service
//builder.Services.AddScoped<ICampaignService, CampaignService>();

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

// register Doner service
//builder.Services.AddScoped<IDonorService, DonorService>();

// Register AutoMapper (ensure AutoMapper.Extensions.Microsoft.DependencyInjection is installed)
builder.Services.AddTransient<AutoMapper.IValueResolver<KonSanad.Repository.Entities.Campaign, KonSanad.Core.Dtos.CampaignDto, string>, KonSanad.APIS.Helpers.CampaignUrlResolver>();


// register ImageUrls resolver (NewProductRequestDto -> HelpOrder.ImageUrlsJson)
builder.Services.AddTransient<AutoMapper.IValueResolver<KonSanad.Core.Dtos.NewProductRequestDto, KonSanad.Repository.Entities.HelpOrder, string?>, KonSanad.APIS.Helpers.ImageUrlsResolver>();

//انا عملت للسطر دا كومينت مش عارف اي لازمته بس هو حل مشكلة الصوره ال في الحمله
//builder.Services.AddAutoMapper(m => m.AddProfile(new MappingProfiles(builder.Configuration)));

// JWT auth
var jwtKey = builder.Configuration["JWT:Key"]
             ?? "BackupSuperLongSecretKey1234567890ABCDEF1234567890";
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
         ClockSkew = TimeSpan.FromSeconds(300) // allow small clock skew
     };

     options.Events = new JwtBearerEvents
     {
         OnAuthenticationFailed = ctx =>
         {
             var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtAuth");
             logger.LogError(ctx.Exception, "JWT authentication failed: {Message}", ctx.Exception.Message);
             return Task.CompletedTask;
         },
         OnTokenValidated = ctx =>
         {
             var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtAuth");
             logger.LogInformation("JWT validated for {sub}", ctx.Principal?.Identity?.Name ?? "(no name)");
             return Task.CompletedTask;
         },
         OnChallenge = ctx =>
         {
             // keep the default 401 behavior but log
             var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtAuth");
             logger.LogWarning("JWT OnChallenge: {Error} - {ErrorDescription}", ctx.Error, ctx.ErrorDescription);
             return Task.CompletedTask;
         }
    };
});



builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] {}
        }
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


app.UseAuthorization();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
