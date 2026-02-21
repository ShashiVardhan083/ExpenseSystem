using ExpenseSystem.Application.Interfaces;
using ExpenseSystem.Domain.Entities;
using ExpenseSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSystem.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ExpenseDbContext Context;

    public ExpenseRepository(ExpenseDbContext context)
    {
        Context = context;
    }

    public async Task<ExpenseClaim?> GetByIdAsync(Guid id)
        => await Context.ExpenseClaims.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<ExpenseClaim>> GetAllAsync()
        => await Context.ExpenseClaims.OrderByDescending(e => e.CreatedDate).ToListAsync();

    public async Task<IEnumerable<ExpenseClaim>> GetByUserIdAsync(Guid userId)
        => await Context.ExpenseClaims
            .Where(e => e.SubmittedByUserId == userId)
            .OrderByDescending(e => e.CreatedDate)
            .ToListAsync();

    public async Task AddAsync(ExpenseClaim expense)
        => await Context.ExpenseClaims.AddAsync(expense);

    public async Task SaveChangesAsync()
        => await Context.SaveChangesAsync();
}