using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ExpenseSystem.Application.DTOs;
using ExpenseSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseSystem.Web.Services;

public class ExpenseApiClient : IExpenseApiClient
{
    private readonly HttpClient HttpClient;
    private readonly ILogger<ExpenseApiClient> Logger;
    private readonly IHttpContextAccessor HttpContextAccessor;

    public ExpenseApiClient(
        HttpClient httpClient,
        ILogger<ExpenseApiClient> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        HttpClient = httpClient;
        Logger = logger;
        HttpContextAccessor = httpContextAccessor;
    }

    private void AddAuthorizationHeader()
    {
        var httpContext = HttpContextAccessor.HttpContext;

        if (httpContext?.User.Identity?.IsAuthenticated == true)
        {
            var token = httpContext.User.FindFirst("JwtToken")?.Value;

            if (!string.IsNullOrWhiteSpace(token))
            {
                HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }

    // AUTH

    public async Task<LoginResponseDto> LoginAsync(string email, string password)
    {
        var request = new { Email = email, Password = password };
        var response = await HttpClient.PostAsJsonAsync("api/auth/login", request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await ReadProblemDetailAsync(response);
            throw new ApplicationException(error ?? "Invalid email or password.");
        }

        response.EnsureSuccessStatusCode(); // triggers 500 page if API fails

        return await response.Content.ReadFromJsonAsync<LoginResponseDto>()
               ?? throw new ApplicationException("API returned empty response");
    }

    public async Task<UserResponseDto> RegisterAsync(string email, string fullName, string password, string confirmPassword)
    {
        var request = new
        {
            Email = email,
            FullName = fullName,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        var response = await HttpClient.PostAsJsonAsync("api/auth/register", request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await ReadProblemDetailAsync(response);
            throw new ApplicationException(error ?? "Registration failed.");
        }

        response.EnsureSuccessStatusCode(); 

        return await response.Content.ReadFromJsonAsync<UserResponseDto>()
               ?? throw new ApplicationException("API returned empty response");
    }

    // EXPENSES

    public async Task<IEnumerable<ExpenseResponseDto>> GetAllExpensesAsync()
    {
        AddAuthorizationHeader();

        var response = await HttpClient.GetAsync("api/expenses");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new ApplicationException("You need to login.");

        response.EnsureSuccessStatusCode(); // IIS 500 / 503 / crash -> custom 500 page

        return await response.Content.ReadFromJsonAsync<IEnumerable<ExpenseResponseDto>>()
               ?? Enumerable.Empty<ExpenseResponseDto>();
    }

    public async Task<ExpenseResponseDto?> GetExpenseByIdAsync(Guid id)
    {
        AddAuthorizationHeader();

        var response = await HttpClient.GetAsync($"api/expenses/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new ApplicationException("You need to login.");

        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new ApplicationException("You do not have permission.");

        response.EnsureSuccessStatusCode(); 

        return await response.Content.ReadFromJsonAsync<ExpenseResponseDto>();
    }

    public async Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto dto)
    {
        AddAuthorizationHeader();

        var response = await HttpClient.PostAsJsonAsync("api/expenses", dto);

        if (!response.IsSuccessStatusCode)
        {
            var message = await ReadProblemDetailAsync(response)
              ?? $"API Error: {response.StatusCode}";


            throw new ApplicationException(message);
        }

        return await response.Content.ReadFromJsonAsync<ExpenseResponseDto>()
               ?? throw new ApplicationException("API returned empty response.");
    }


    public async Task<ExpenseResponseDto> ApproveExpenseAsync(Guid id)
    {
        AddAuthorizationHeader();

        var response = await HttpClient.PutAsync($"api/expenses/{id}/approve", null);

        response.EnsureSuccessStatusCode(); 

        return await response.Content.ReadFromJsonAsync<ExpenseResponseDto>()
               ?? throw new ApplicationException("API returned empty response");
    }

    public async Task<ExpenseResponseDto> RejectExpenseAsync(Guid id, RejectExpenseDto dto)
    {
        AddAuthorizationHeader();

        var response = await HttpClient.PutAsJsonAsync($"api/expenses/{id}/reject", dto);

        response.EnsureSuccessStatusCode(); 

        return await response.Content.ReadFromJsonAsync<ExpenseResponseDto>()
               ?? throw new ApplicationException("API returned empty response");
    }

    public async Task<ExpenseResponseDto> ProcessPaymentAsync(Guid id)
    {
        AddAuthorizationHeader();

        var response = await HttpClient.PostAsync($"api/expenses/{id}/process-payment", null);

        response.EnsureSuccessStatusCode(); 

        return await response.Content.ReadFromJsonAsync<ExpenseResponseDto>()
               ?? throw new ApplicationException("API returned empty response");
    }

    // Helper

    private static async Task<string?> ReadProblemDetailAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return problem?.Detail;
        }
        catch
        {
            return await response.Content.ReadAsStringAsync();
        }
    }
    private record ProblemDetail(string? Detail, string? Title);
}
