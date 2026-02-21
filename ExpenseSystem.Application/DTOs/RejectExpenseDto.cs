using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ExpenseSystem.Application.DTOs;

    public class RejectExpenseDto
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Please provide a meaningful rejection reason.")]
        public string Reason { get; set; } = string.Empty;
    }

