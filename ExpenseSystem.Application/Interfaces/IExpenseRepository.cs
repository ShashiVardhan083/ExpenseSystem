using ExpenseSystem.Domain.Entities;

namespace ExpenseSystem.Application.Interfaces;

public interface IExpenseRepository
{
    Task<ExpenseClaim?> GetByIdAsync(Guid id);
    Task<IEnumerable<ExpenseClaim>> GetAllAsync();
    Task<IEnumerable<ExpenseClaim>> GetByUserIdAsync(Guid userId);
    Task AddAsync(ExpenseClaim expense);
    Task SaveChangesAsync();
}