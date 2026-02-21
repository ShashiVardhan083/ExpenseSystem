namespace ExpenseSystem.Application.DTOs;

public class ExpenseResponseDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? PaymentReference { get; set; }
    public Guid SubmittedByUserId { get; set; }
}