using GomexPraksa.ConnectionFactory;
using GomexPraksa.Repository;
using GomexPraksa.RepositoryComerc;
using GomexPraksa.Services;
using GomexPraksa.ServicesComerc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 

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
builder.Services.AddScoped<IRucChangeService, RucChangeService>();
builder.Services.AddScoped<IRucChangeTracker, RucChangeTrackerRepo>();
builder.Services.AddScoped<ICriticalProducts, CriticalProductsRepo>();
builder.Services.AddScoped<ICriticalProductsService, CriticalProductsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();