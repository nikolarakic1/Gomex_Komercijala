using GomexPraksa.ApplicationUserSecurity;
using GomexPraksa.Auth;
using GomexPraksa.ConnectionFactory;
using GomexPraksa.Repository;
using GomexPraksa.RepositoryComerc;
using GomexPraksa.Services;
using GomexPraksa.ServicesComerc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseWebRoot("wwwroot");

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7067",
                "http://localhost:5261"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// =========================
// DATABASE - IDENTITY
// =========================

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


// =========================
// IDENTITY
// =========================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();


// =========================
// JWT AUTHENTICATION
// =========================

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey!)
                    )
            };
    });

builder.Services.AddAuthorization();


// =========================
// DI
// =========================

builder.Services.AddSingleton<IConnFactory, ConnFactory>();

builder.Services.AddScoped<IArtikalRepo, ArtikalRepo>();
builder.Services.AddScoped<IArtikalService, ArtikalService>();

builder.Services.AddScoped<IAkcijaRepo, AkcijaRepo>();
builder.Services.AddScoped<IAkcijaService, AkcijaService>();

builder.Services.AddScoped<IDobavljacRepo, DobavljacRepo>();
builder.Services.AddScoped<IDobavljacServis, DobavljacServis>();

builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IRucChangeTracker, RucChangeTrackerRepo>();
builder.Services.AddScoped<IRucChangeService, RucChangeService>();

builder.Services.AddScoped<ICriticalProducts, CriticalProductsRepo>();
builder.Services.AddScoped<ICriticalProductsService, CriticalProductsService>();

builder.Services.AddScoped<IJwtService, JwtService>();

// =========================
// APP
// =========================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowLocal");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//await RoleSeeder.SeedRolesAsync(app.Services);

app.Run();