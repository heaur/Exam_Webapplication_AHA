using Serilog;
using QuizApi.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // Enable controller support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (frontend-backend communication)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Vite dev server
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure Serilog for logging
var logger = new LoggerConfiguration()
    .WriteTo.File($"Logs/log_{DateTime.Now:yyyyMMdd_HHmmss}.txt")
    .CreateLogger();

// Replace default logging with Serilog
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// JSON options to handle reference loops
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Configure Entity Framework and SQLite
builder.Services.AddDbContext<QuizDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); // Enable CORS policy

app.UseAuthorization();

app.MapControllers(); // Connect controllers to the app

app.Run();
