using KonSanad.Core.IRepositery;
using KonSanad.Repository.Data.Persistence;
using KonSanad.Repository.Repository;
using KonSanad.Services.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<KonSanadDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IDonationOrderService,DonationOrderService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<KonSanadDbContext>();

var ILoggerFactory = services.GetRequiredService<ILoggerFactory>();
try
{
    await context.Database.MigrateAsync();
 
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
        c.RoutePrefix = string.Empty;   // ← دي اللي كانت ناقصة يا معلم!!
    });
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
