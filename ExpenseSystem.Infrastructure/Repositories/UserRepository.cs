using ExpenseSystem.Application.Interfaces;
using ExpenseSystem.Domain.Entities;
using ExpenseSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ExpenseDbContext Context;

    public UserRepository(ExpenseDbContext context)
    {
        Context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
        => await Context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalized = email.ToLower().Trim();
        return await Context.Users.FirstOrDefaultAsync(u => u.Email == normalized);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await Context.Users.OrderBy(u => u.FullName).ToListAsync();

    public async Task AddAsync(User user)
        => await Context.Users.AddAsync(user);

    public async Task SaveChangesAsync()
        => await Context.SaveChangesAsync();

    public async Task<bool> EmailExistsAsync(string email)
    {
        var normalized = email.ToLower().Trim();
        return await Context.Users.AnyAsync(u => u.Email == normalized);
    }
}