using System.Text.RegularExpressions;

namespace HvacConnect.Core.Validation;

/// <summary>Shared validation rules used by the console workflows.</summary>
public static class InputValidator
{
    public static (bool isValid, string errorMessage) ValidateRequiredText(string? value, string fieldName, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (false, $"{fieldName} is required.");

        var text = value.Trim();
        if (text.Length < minLength || text.Length > maxLength)
            return (false, $"{fieldName} must be between {minLength} and {maxLength} characters.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidateOrganizationName(
        string? value, string fieldName, int minLength, int maxLength, bool required = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            return required ? (false, $"{fieldName} is required.") : (true, string.Empty);

        var name = value.Trim();
        if (name.Length < minLength || name.Length > maxLength)
            return (false, $"{fieldName} must be between {minLength} and {maxLength} characters.");

        if (!name.Any(char.IsLetter))
            return (false, $"{fieldName} must contain at least one letter.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidatePersonName(string? value, string fieldName, int minLength = 2, int maxLength = 75)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (false, $"{fieldName} is required.");

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            return (false, $"{fieldName} cannot start or end with spaces.");

        if (value.Length < minLength || value.Length > maxLength)
            return (false, $"{fieldName} must be between {minLength} and {maxLength} characters.");

        if (!value.All(char.IsLetter))
            return (false, $"{fieldName} can contain letters only.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidateContactName(string? value, string fieldName, bool required, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(value))
            return required ? (false, $"{fieldName} is required.") : (true, string.Empty);

        if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            return (false, $"{fieldName} cannot start or end with spaces.");

        if (value.Length < 2 || value.Length > maxLength)
            return (false, $"{fieldName} must be between 2 and {maxLength} characters.");

        if (!value.All(ch => char.IsLetter(ch) || ch is ' ' or '\'' or '-' or '.'))
            return (false, $"{fieldName} can contain letters, spaces, apostrophes, hyphens, and periods only.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidateOptionalText(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length <= maxLength)
            return (true, string.Empty);

        return (false, $"{fieldName} cannot exceed {maxLength} characters.");
    }

    public static (bool isValid, string errorMessage) ValidateEmployeeCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (true, string.Empty);

        var code = value.Trim();
        if (code.Length is < 2 or > 30 || !Regex.IsMatch(code, @"^[A-Za-z0-9][A-Za-z0-9_-]*$", RegexOptions.CultureInvariant))
            return (false, "Employee code must be 2-30 characters and contain only letters, digits, hyphens, or underscores.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidateStateCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, @"^[A-Za-z]{2}$", RegexOptions.CultureInvariant))
            return (false, "Enter a 2-letter state code (for example, CA or NY).");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidatePostalCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 4 or > 15 ||
            !Regex.IsMatch(value, @"^[A-Za-z0-9](?:[A-Za-z0-9 -]*[A-Za-z0-9])?$", RegexOptions.CultureInvariant))
            return (false, "Postal code must be 4-15 characters and contain only letters, digits, spaces, or hyphens.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidateEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 254 ||
            !Regex.IsMatch(value.Trim(), @"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.CultureInvariant))
            return (false, "Enter a valid email address.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidatePassword(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 8 ||
            !value.Any(char.IsUpper) || !value.Any(char.IsLower) ||
            !value.Any(char.IsDigit) || !value.Any(ch => !char.IsLetterOrDigit(ch)))
            return (false, "Password must be at least 8 characters and include uppercase, lowercase, digit, and special character.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidatePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !Regex.IsMatch(value, @"^\+?(?:\([0-9]{3}\)|[0-9]{3})[ .-]?[0-9]{3}[ .-]?[0-9]{4}$", RegexOptions.CultureInvariant))
            return (false, "Enter a 10-digit phone number, for example 555-123-4567 or (555) 123-4567.");

        return (true, string.Empty);
    }

    public static (bool isValid, string errorMessage) ValidateAssetTag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length is < 3 or > 50 ||
            !Regex.IsMatch(value.Trim(), @"^[A-Za-z0-9][A-Za-z0-9._/-]*$", RegexOptions.CultureInvariant))
            return (false, "Asset tag must be 3-50 characters and contain only letters, digits, '.', '_', '/', or '-'.");

        return (true, string.Empty);
    }
}
