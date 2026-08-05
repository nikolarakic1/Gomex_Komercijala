using GomexPraksa.ConnectionFactory;
using GomexPraksa.Repository;
using GomexPraksa.RepositoryComerc;
using GomexPraksa.Services;
using GomexPraksa.ServicesComerc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseWebRoot("wwwroot"); // creates expectation; to point elsewhere, replace with existing folder path (e.g., "StaticFiles")




builder.Services.AddControllers(); 

// CORS for frontend (development): allow GomexPraksaMVC dev origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
    {
        policy.WithOrigins("https://localhost:7067", "http://localhost:5261")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnFactory, ConnFactory>();

builder.Services.AddScoped<IArtikalRepo, ArtikalRepo>();
builder.Services.AddScoped<IArtikalService, ArtikalService>();
builder.Services.AddScoped<IAkcijaService, AkcijaService>();
builder.Services.AddScoped<IAkcijaRepo, AkcijaRepo>();
builder.Services.AddScoped<IDobavljacRepo, DobavljacRepo>();
builder.Services.AddScoped<IDobavljacServis, DobavljacServis>();
builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowLocal");

app.UseAuthorization();

app.MapControllers();

app.Run();