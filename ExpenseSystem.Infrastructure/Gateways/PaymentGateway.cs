using System.Net.Http.Json;
using ExpenseSystem.Application.DTOs;
using ExpenseSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseSystem.Infrastructure.Gateways;

public class PaymentGateway : IPaymentGateway
{
    private readonly HttpClient HttpClient;
    private readonly ILogger<PaymentGateway> Logger;

    public PaymentGateway(HttpClient httpClient, ILogger<PaymentGateway> logger)
    {
        HttpClient = httpClient;
        Logger = logger;
    }

    public async Task<PaymentResponseDto> ProcessAsync(PaymentRequestDto request)
    {
        Logger.LogInformation(
            "Calling payment gateway for ExpenseId: {ExpenseId}, Amount: {Amount:C}",
            request.ExpenseId, request.Amount);

        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/payment/process", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();

                if (result != null && result.Success)
                    Logger.LogInformation("Payment successful, Reference: {Ref}", result.Reference);
                else
                    Logger.LogWarning("Payment rejected by gateway: {Msg}", result?.Message);

                return result ?? new PaymentResponseDto { Success = false, Message = "Empty response from gateway." };
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.LogWarning("Payment gateway returned {Status}: {Error}", (int)response.StatusCode, errorContent);

            return new PaymentResponseDto
            {
                Success = false,
                Message = $"Payment gateway returned {(int)response.StatusCode}: {errorContent}"
            };
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Failed to connect to payment gateway");
            return new PaymentResponseDto { Success = false, Message = $"Cannot reach payment gateway: {ex.Message}" };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error in payment gateway");
            return new PaymentResponseDto { Success = false, Message = $"Unexpected error: {ex.Message}" };
        }
    }
}