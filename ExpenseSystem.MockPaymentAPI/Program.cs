// ═══════════════════════════════════════════════════════════════
// MOCK PAYMENT API
//
// This is a separate minimal Web API that simulates an external
// payment service (think: Stripe, PayPal, bank API).
//
// PURPOSE: Demonstrate real HttpClient usage from ExpenseSystem.API
// It runs on port 5001, while the main API runs on port 5000.
//
// In a real system this would be:
//  - An actual third-party payment processor API
//  - A microservice owned by the Finance team
//  - A banking API with OAuth and strict rate limits
// ═══════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Mock Payment Gateway API",
        Version = "v1",
        Description = "Simulates an external payment processor for POC purposes"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Gateway v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();