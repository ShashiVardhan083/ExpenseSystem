using ExpenseSystem.Domain.Entities;

namespace ExpenseSystem.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
}