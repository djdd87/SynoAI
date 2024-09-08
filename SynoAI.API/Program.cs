using Microsoft.EntityFrameworkCore;
using SynoAI.API.EndPoints;
using SynoAI.Core.Interfaces;
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
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetService<AppDbContext>()!);

var app = builder.Build();

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

app.Run();