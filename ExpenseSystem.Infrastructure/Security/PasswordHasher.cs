namespace ExpenseSystem.Infrastructure.Security;

/// <summary>
/// PASSWORD HASHER - Utility for secure password hashing
/// 
/// USES BCRYPT:
///  - Industry-standard password hashing
///  - Automatically handles salt generation
///  - Configurable work factor for security/performance balance
///  - Resistant to rainbow table attacks
/// 
/// NOTE: This is a thin wrapper around BCrypt.Net
/// The actual AuthService uses BCrypt.Net.BCrypt directly
/// </summary>
public static class PasswordHasher
{
    private const int WorkFactor = 12; // Higher = more secure but slower

    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
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