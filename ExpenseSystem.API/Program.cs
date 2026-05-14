using System.Text;
using ExpenseSystem.Application.Interfaces;
using ExpenseSystem.Application.Mappings;
using ExpenseSystem.Application.Services;
using ExpenseSystem.Domain.Entities;
using ExpenseSystem.Domain.Enums;
using ExpenseSystem.Infrastructure.Gateways;
using ExpenseSystem.Infrastructure.Persistence;
using ExpenseSystem.Infrastructure.Repositories;
using ExpenseSystem.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting Expense System API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/expense-api-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7));

    // Controllers 
    builder.Services.AddControllers();

    // JWT Authentication 
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = builder.Configuration["JwtSettings:SecretKey"];

    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("JWT SecretKey not configured");
    }
    var issuer = jwtSettings["Issuer"] ?? "ExpenseSystemAPI";
    var audience = jwtSettings["Audience"] ?? "ExpenseSystemClient";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT token validated for: {Name}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    //  Swagger with JWT support
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Expense Reimbursement API",
            Version = "v1",
            Description = "JWT-secured expense management API. Employee: submit & view own. Admin: approve, reject, pay all."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Enter: Bearer {your JWT token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // Entity Framework 
    builder.Services.AddDbContext<ExpenseDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("ExpenseSystem.Infrastructure")));

    builder.Services.AddHealthChecks()
    .AddDbContextCheck<ExpenseDbContext>("Database");

    // AutoMapper 
    builder.Services.AddAutoMapper(typeof(ExpenseMappingProfile).Assembly);

    //  Application Services 
    builder.Services.AddScoped<ExpenseService>();
    builder.Services.AddScoped<AuthService>();

    //  Repositories 
    builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    //  Security 
    builder.Services.AddScoped<ITokenService, TokenService>();

    //  HTTP Client / Payment Gateway 
    var paymentBaseUrl = builder.Configuration["PaymentGateway:BaseUrl"] ?? "http://localhost:7291/";
    builder.Services.AddHttpClient<IPaymentGateway, PaymentGateway>(client =>
    {
        client.BaseAddress = new Uri(paymentBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("X-API-Source", "ExpenseSystem");
    });

    // CORS 
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowMvcApp", policy =>
            policy.WithOrigins("https://localhost:7292", "http://localhost:5002")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials());
    });

    var app = builder.Build();

    // Middleware Pipeline
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseCors("AllowMvcApp");

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Expense System API";
        c.DisplayRequestDuration();
    });

    app.UseMiddleware<ExpenseSystem.API.Extensions.GlobalExceptionMiddleware>();

    //app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    component = e.Key,
                    status = e.Value.Status.ToString(),
                    error = e.Value.Exception?.Message
                })
            };

            await context.Response.WriteAsJsonAsync(result);
        }
    });


    // Redirect root to Swagger
    app.MapGet("/", () => Results.Redirect("/swagger"));

    Log.Information("Expense System API started. Swagger at /swagger");
    Log.Information("Admin: admin@expense.com | Employee: employee@expense.com");

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();

        context.Database.Migrate();

        if (!context.Users.Any())
        {
            var admin = User.Create(
                "admin@expense.com",
                "System Administrator",
                BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                UserRole.Admin);

            var employee = User.Create(
                "employee@expense.com",
                "Employee User",
                BCrypt.Net.BCrypt.HashPassword("Employee123!"),
                UserRole.Employee);

            context.Users.AddRange(admin, employee);
            context.SaveChanges();
        }
    }
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}