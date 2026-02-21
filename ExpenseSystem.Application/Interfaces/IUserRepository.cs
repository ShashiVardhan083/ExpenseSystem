using ExpenseSystem.Domain.Entities;

namespace ExpenseSystem.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task<bool> EmailExistsAsync(string email);
}