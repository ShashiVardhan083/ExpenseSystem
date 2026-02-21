using ExpenseSystem.Domain.Enums;

namespace ExpenseSystem.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public bool IsActive { get; private set; }

    private User() { }

    public static User Create(string email, string fullName, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower().Trim(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void RecordLogin()
    {
        LastLoginDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("User is already inactive.");
        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("User is already active.");
        IsActive = true;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");
        PasswordHash = newPasswordHash;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}