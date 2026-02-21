using ExpenseSystem.Application.DTOs;

namespace ExpenseSystem.Application.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentResponseDto> ProcessAsync(PaymentRequestDto request);
}