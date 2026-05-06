using AutoMapper;
using KonSanad.APIS.Extenaction;
using KonSanad.APIS.Helpers;
using KonSanad.Core.Entity.Identity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Identity.ctx;
using KonSanad.Repository.Data.Persistence;
using KonSanad.Repository.Repository;
using KonSanad.Services.Realtime;
using KonSanad.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Security.Principal;
using System.Text;


var cairoZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
AppContext.SetData("DOTNET_DEFAULT_TIMEZONE", "Egypt Standard Time");
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddJwtAuthentication(builder.Configuration);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();


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


// Register VolunteerServices
builder.Services.AddScoped<KonSanad.Core.IRepositery.IVolunteerService,         KonSanad.Services.Services.VolunteerService>();

// register FileService implementation from Services project
builder.Services.AddScoped<KonSanad.Core.IRepositery.IFileService, KonSanad.Services.Services.FileService>();

// register DI
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IVolunteerService,VolunteerService>();
builder.Services.AddScoped<IMangerServices, MangerServices>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
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

Console.WriteLine("Machine: " + Environment.MachineName);
Console.WriteLine("Now: " + DateTime.Now);
Console.WriteLine("TimeZone: " + TimeZoneInfo.Local.DisplayName);
if (string.IsNullOrEmpty(testConn) || testConn.Contains("IdentityDb"))
{
    throw new Exception("❌ ERROR: Still using old connection string!");
}




builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token only (without Bearer)",
        
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault;
    });

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    var config = ConfigurationOptions.Parse(connectionString);
    config.AbortOnConnectFail = false;
    config.Ssl = false;
    return ConnectionMultiplexer.Connect(config);
});
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
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
    await KonSanadDbContextSeed.SeedAsync(context , userManager);
    await Identitycontext.Database.MigrateAsync();
    await AppIdentityDbContextSeed.SeedUsersAsync(userManager);

    var rolesToEnsure = new[] { "Volunteer", "Beneficiary", "Donor" , "Manager" };
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
app.MapHub<NotificationHub>("/hubs/notifications").RequireCors("AllowAll");

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.Run();
