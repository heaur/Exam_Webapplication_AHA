using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using QuizApi.DAL;
using QuizApi.DAL.Interfaces;
using QuizApi.DAL.Repositories;
using QuizApi.Application.Services;
using QuizApi.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using QuizApi.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// logging with Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// services
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles); // avoid JSON cycles in dev

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core (SQLite)
builder.Services.AddDbContext<QuizDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>();

// Identity 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // Can be true in Production
        options.Password.RequireDigit = true; // Require at least one digit
        options.Password.RequiredLength = 6; // Minimum length of password
    })
    .AddEntityFrameworkStores<AppDbContext>();

// DAL Repositories
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// Application Services
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IOptionService, OptionService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(QuizApi.Application.Mapping.MappingProfile));


// CORS (frontend on Vite)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// (Optional) Auto-create DB schema for MVP
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
    db.Database.EnsureCreated();
}

// pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
