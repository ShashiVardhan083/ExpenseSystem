using System.ComponentModel.DataAnnotations;

namespace ExpenseSystem.Application.DTOs
{

    public class RejectResponseDto
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Please provide a meaningful rejection reason (minimum 10 characters).")]
        public string Reason { get; set; } = string.Empty;
    }
}