using GymManagement.API.Models;
using Microsoft.AspNetCore.Identity;
using GymManagement.API.Data;
using GymManagement.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Configure CORS for Angular and Vercel
var allowedOrigins = new List<string>
{
    "http://localhost:4200"
};

var frontendUrl = builder.Configuration["Frontend:Url"];

if (!string.IsNullOrWhiteSpace(frontendUrl))
{
    allowedOrigins.Add(frontendUrl.TrimEnd('/'));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure forwarded headers for Render's proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Render's proxy IPs must be explicitly trusted
    // before forwarded headers are accepted.
    var knownProxy = builder.Configuration["ReverseProxy:KnownProxy"];

    if (!string.IsNullOrWhiteSpace(knownProxy) &&
        IPAddress.TryParse(knownProxy, out var proxyAddress))
    {
        options.KnownProxies.Add(proxyAddress);
    }
});

// Configure PostgreSQL
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Authentication Service
builder.Services.AddScoped<AuthService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Create demo Admin account in Development only
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<GymDbContext>();

    var configuration = scope.ServiceProvider
        .GetRequiredService<IConfiguration>();

    var adminEmail = "admin@gymmanagement.com";
    var adminPassword = configuration["DemoAdmin:Password"];

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "DemoAdmin:Password is not configured.");
    }

    var existingAdmin = await db.Users
        .FirstOrDefaultAsync(u => u.Email == adminEmail);

    if (existingAdmin == null)
    {
        var admin = new User
        {
            Email = adminEmail,
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();

        admin.PasswordHash = passwordHasher.HashPassword(
            admin, adminPassword);

        db.Users.Add(admin);
        await db.SaveChangesAsync();

        Console.WriteLine("Demo Admin account created.");
    }
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Existing Weather Forecast endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}
