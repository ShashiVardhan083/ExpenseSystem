using System.ComponentModel.DataAnnotations;

namespace ExpenseSystem.Application.DTOs;

public class CreateExpenseDto
{
    [Required(ErrorMessage = "Employee name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 100_000, ErrorMessage = "Amount must be between 0.01 and 100,000.")]
    public decimal Amount { get; set; }
}