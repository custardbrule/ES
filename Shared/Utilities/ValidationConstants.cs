namespace Utilities;

/// <summary>
/// Constants for validation patterns used across the application
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Account validation regex pattern
    /// - Minimum 6 characters
    /// - Only allows: letters (a-z, A-Z), numbers (0-9), and special characters (@, _, -)
    /// </summary>
    public const string AccountPattern = @"^[a-zA-Z0-9@_-]{6,}$";

    /// <summary>
    /// Password validation regex pattern
    /// - Minimum 8 characters
    /// - At least 1 uppercase letter (A-Z)
    /// - At least 1 number (0-9)
    /// - At least 1 special character from: @$!%*?&
    /// </summary>
    public const string PasswordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

    /// <summary>
    /// Account validation error message
    /// </summary>
    public const string AccountValidationMessage = "Account must be at least 6 characters and contain only letters, numbers, @, _, or -";

    /// <summary>
    /// Password validation error message
    /// </summary>
    public const string PasswordValidationMessage = "Password must be at least 8 characters with at least 1 uppercase letter, 1 number, and 1 special character (@$!%*?&)";
}
