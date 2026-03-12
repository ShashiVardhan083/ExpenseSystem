using ExpenseSystem.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting Expense System MVC Web App...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/expense-web-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7)); // write this in a separate class

    builder.Services.AddControllersWithViews();
    builder.Services.AddResponseCaching();

    //--gzip and brotli compressions--
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        //options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    {
        options.Level = System.IO.Compression.CompressionLevel.Optimal;
    });

    //builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    //{
    //    options.Level = System.IO.Compression.CompressionLevel.Optimal;
    //});

    builder.Services.AddWebOptimizer(pipeline =>
    {
        pipeline.AddCssBundle("/css/bundle.css",
            "css/site.css");

        pipeline.AddJavaScriptBundle("/js/bundle.js",
            "js/site.js");
    });

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
     .AddCookie(options =>
     {
         options.LoginPath = "/Account/Login";          // 401 → login
         options.AccessDeniedPath = "/Home/Error/403";  // 403 → custom page
         options.ExpireTimeSpan = TimeSpan.FromHours(24);
         options.SlidingExpiration = true;
     });

    builder.Services.AddAuthorization();

    // HttpClient -> API 
    var apiBaseUrl = builder.Configuration["ExpenseApiUrl"] ?? "http://localhost:5000/";

    builder.Services.AddHttpClient<IExpenseApiClient, ExpenseApiClient>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "ExpenseSystemWebMVC/1.0");
    })
    .AddTransientHttpErrorPolicy(policy =>
        policy.WaitAndRetryAsync(2, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))); //handles temporary failures

    // Session (for TempData)
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddHttpContextAccessor();


    var app = builder.Build();

    app.UseExceptionHandler("/Home/Error/500");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    //  Middleware
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseWebOptimizer();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseResponseCaching();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("MVC Web App started. API URL: {ApiUrl}", apiBaseUrl);

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