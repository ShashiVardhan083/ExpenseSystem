using System.Net;
using System.Text.Json;
using ExpenseSystem.Domain;

namespace ExpenseSystem.API.Extensions;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate Next;
    private readonly ILogger<GlobalExceptionMiddleware> Logger;
    private readonly IHostEnvironment Env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        Next = next;
        Logger = logger;
        Env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        try
        {
            await Next(context);
        }
        catch (DomainException ex)
        {
            Logger.LogWarning(ex, "Domain violation at {Path}. CorrelationId: {CorrelationId}", context.Request.Path, correlationId);
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message, correlationId);
        }
        catch (KeyNotFoundException ex)
        {
            Logger.LogWarning(ex, "Resource not found at {Path}. CorrelationId: {CorrelationId}", context.Request.Path, correlationId);
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message, correlationId);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex, "Unauthorized access at {Path}. CorrelationId: {CorrelationId}", context.Request.Path, correlationId);
            await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "You are not authorized to access this resource.", correlationId);
        }
        catch (ApplicationException ex)
        {
            Logger.LogError(ex, "Application error at {Path}. CorrelationId: {CorrelationId}", context.Request.Path, correlationId);
            await WriteErrorResponse(context, HttpStatusCode.UnprocessableEntity, ex.Message, correlationId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unhandled exception at {Path}. CorrelationId: {CorrelationId}", context.Request.Path, correlationId);
            var message = Env.IsDevelopment() ? ex.Message : "An unexpected error occurred. Please try again later.";
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, message, correlationId);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message, string correlationId)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = statusCode.ToString(),
            Status = (int)statusCode,
            Detail = message,
            Instance = context.Request.Path.ToString(),
            TraceId = correlationId,
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}