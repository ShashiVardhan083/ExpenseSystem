using ExpenseSystem.Domain.Enums;
namespace ExpenseSystem.Domain.Entities;
public class ExpenseClaim
{
    public Guid Id { get; private set; }
    public string EmployeeName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ProcessedDate { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? PaymentReference { get; private set; }

    // Navigation: which user submitted this expense
    public Guid SubmittedByUserId { get; private set; }

    private ExpenseClaim() { }

    public static ExpenseClaim Create(string employeeName, string description, decimal amount, Guid submittedByUserId)
    {
        if (amount <= 0)
            throw new DomainException("Expense amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(employeeName))
            throw new DomainException("Employee name is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Expense description is required.");

        return new ExpenseClaim
        {
            Id = Guid.NewGuid(),
            EmployeeName = employeeName.Trim(),
            Description = description.Trim(),
            Amount = amount,
            Status = ExpenseStatus.Submitted,
            CreatedDate = DateTime.UtcNow,
            SubmittedByUserId = submittedByUserId
        };
    }

    public void Approve()
    {
        if (Status != ExpenseStatus.Submitted)
            throw new DomainException(
                $"Cannot approve an expense that is currently '{Status}'. Only 'Submitted' claims can be approved.");

        Status = ExpenseStatus.Approved;
        ProcessedDate = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new DomainException(
                $"Cannot reject an expense that is currently '{Status}'. Only 'Submitted' claims can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A rejection reason must be provided.");

        Status = ExpenseStatus.Rejected;
        RejectionReason = reason.Trim();
        ProcessedDate = DateTime.UtcNow;
    }

    public void MarkAsPaid(string paymentReference)
    {
        if (Status != ExpenseStatus.Approved)
            throw new DomainException(
                $"Cannot process payment for an expense with status '{Status}'. Only 'Approved' claims can be paid.");

        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new DomainException("Payment reference is required.");

        Status = ExpenseStatus.Paid;
        PaymentReference = paymentReference;
        ProcessedDate = DateTime.UtcNow;
    }
}