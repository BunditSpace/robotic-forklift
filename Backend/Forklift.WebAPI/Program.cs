using Forklift.Application.Interfaces;
using Forklift.Application.Services;
using Forklift.Core.Interfaces;
using Forklift.Infrastructure.Data;
using Forklift.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Forklift API",
        Version = "v1",
        Description = "API for Forklift management"
    });

    // Include XML comments if the documentation file was generated
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure EF Core SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register DI Services
builder.Services.AddScoped<IForkliftRepository, ForkliftRepository>();
builder.Services.AddScoped<IForkliftService, ForkliftService>();

// Configure CORS for Angular frontend
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger(); // serves /swagger/v1/swagger.json
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Forklift API v1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

app.UseHttpsRedirection();

app.UseDefaultFiles();  // Serve wwwroot/index.html at /
app.UseStaticFiles();   // Serve Angular static assets

app.UseCors("AllowAngularApp");

app.UseAuthorization();
app.MapControllers();

// SPA fallback: return index.html for any unmatched route (Angular client-side routing)
app.MapFallbackToFile("index.html");

app.SeedDatabase();

app.Run();
