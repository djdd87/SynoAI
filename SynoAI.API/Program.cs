using Microsoft.EntityFrameworkCore;
using SynoAI.API.Endpoints;
using SynoAI.API.EndPoints;
using SynoAI.Core.Interfaces;
using SynoAI.Core.Notifiers;
using SynoAI.Core.Processors;
using SynoAI.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AddDbContext ensures that the context is scoped per request
string dbPath = Path.Join(builder.Environment.ContentRootPath, "app.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Register implicit services
builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<IDetectionService, DetectionService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IZoneService, ZoneService>();

// Register the notifiers and processors
builder.Services.RegisterNotifiers();
builder.Services.RegisterProcessors();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure the routes
app.MapCameraEndpoints();
app.MapDetectionEndpoints();
app.MapZoneEndpoints();

app.Run();