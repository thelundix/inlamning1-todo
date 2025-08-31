using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Kestrel/HTTPS & request limits
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 10 * 1024; // 10 KB
});

// Db context
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"
    )
);

// Password hashing utan full Identity
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// CORS: bara kända klienter
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(
        "AllowKnownClients",
        p => p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()
    );
});

// JWT authentication
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = !string.IsNullOrEmpty(jwt["Issuer"]),
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = !string.IsNullOrEmpty(jwt["Audience"]),
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role, // istället för "role"
        };
    });

builder.Services.AddAuthorization();

// Rate limiting: global + extra hårt på /auth/login
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // per IP
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }
        )
    );
    options.AddFixedWindowLimiter(
        "login",
        opt =>
        {
            opt.PermitLimit = 5; // 5 försök/min/IP
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        }
    );
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// DB ensure och seed admin
// Använder EnsureCreated för att slippa migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var admin = new User { Username = "admin", Role = "Admin" };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin!12345");
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHsts();

app.UseCors("AllowKnownClients");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
