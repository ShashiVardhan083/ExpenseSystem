namespace ExpenseSystem.Infrastructure.Security;

public static class PasswordHasher
{
    private const int WorkFactor = 12; 

    // Hashes a plain text password
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    
    // Verifies a password against a hash
    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty", nameof(hash));

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}