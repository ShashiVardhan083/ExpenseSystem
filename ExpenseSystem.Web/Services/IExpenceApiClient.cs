using ExpenseSystem.Web.Models;
using ExpenseSystem.Application.DTOs;

namespace ExpenseSystem.Web.Services;

public interface IExpenseApiClient
{
    // Auth
    Task<LoginResponseDto> LoginAsync(string email, string password);
    Task<UserResponseDto> RegisterAsync(string email, string fullName, string password, string confirmPassword);

    // Expenses
    Task<IEnumerable<ExpenseResponseDto>> GetAllExpensesAsync();
    Task<ExpenseResponseDto?> GetExpenseByIdAsync(Guid id);
    Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto dto);
    Task<ExpenseResponseDto> ApproveExpenseAsync(Guid id);
    Task<ExpenseResponseDto> RejectExpenseAsync(Guid id, RejectExpenseDto dto);
    Task<ExpenseResponseDto> ProcessPaymentAsync(Guid id);
}