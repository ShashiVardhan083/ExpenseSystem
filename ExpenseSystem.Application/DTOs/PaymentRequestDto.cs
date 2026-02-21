namespace ExpenseSystem.Application.DTOs;

public class PaymentRequestDto
{
    public Guid ExpenseId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}