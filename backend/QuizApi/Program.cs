using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Serilog;
using QuizApi.DAL;                    
using QuizApi.Domain;   

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// MVC/JSON
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core: domain DB
builder.Services.AddDbContext<QuizDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// EF Core: Identity DB
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DbContext>()
    .AddDefaultTokenProviders();

// Identity
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // True for production
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<DbContext>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(QuizApi.Application.Mapping.MappingProfile));

var app = builder.Build();

// EnsureCreated
using (var scope = app.Services.CreateScope())
{
    var quizDb = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
    quizDb.Database.EnsureCreated();

    var identityDb = scope.ServiceProvider.GetRequiredService<DbContext>();
    identityDb.Database.EnsureCreated();
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

// Redirect "/" to Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", ctx =>
    {
        ctx.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.Run();
