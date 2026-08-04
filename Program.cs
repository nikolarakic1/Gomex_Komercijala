using GomexPraksa.ConnectionFactory;
using GomexPraksa.Repository;
using GomexPraksa.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnFactory, ConnFactory>();

builder.Services.AddScoped<IArtikalRepo, ArtikalRepo>();
builder.Services.AddScoped<IArtikalService, ArtikalService>();
builder.Services.AddScoped<IAkcijaService, AkcijaService>();
builder.Services.AddScoped<IAkcijaRepo, AkcijaRepo>();

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