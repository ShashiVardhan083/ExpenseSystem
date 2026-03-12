using ExpenseSystem.Application.Interfaces;
using ExpenseSystem.Domain.Entities;
using ExpenseSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSystem.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ExpenseDbContext DbContext;

    public ExpenseRepository(ExpenseDbContext context)
    {
        DbContext = context;
    }

    public async Task<ExpenseClaim?> GetByIdAsync(Guid id)
        => await DbContext.ExpenseClaims.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<ExpenseClaim>> GetAllAsync()
        => await DbContext.ExpenseClaims.OrderByDescending(e => e.CreatedDate).ToListAsync();

    public async Task<IEnumerable<ExpenseClaim>> GetByUserIdAsync(Guid userId)
        => await DbContext.ExpenseClaims
            .Where(e => e.SubmittedByUserId == userId)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync();

    public async Task AddAsync(ExpenseClaim expense)
        => await DbContext.ExpenseClaims.AddAsync(expense);

    public async Task SaveChangesAsync()
        => await DbContext.SaveChangesAsync();
}