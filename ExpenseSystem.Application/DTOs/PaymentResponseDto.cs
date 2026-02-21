namespace ExpenseSystem.Application.DTOs;

public class PaymentResponseDto
{
    public bool Success { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}