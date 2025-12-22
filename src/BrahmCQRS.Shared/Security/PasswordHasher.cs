using System.Text;
using System.Text.RegularExpressions;

namespace BrahmCQRS.Shared.Security;

/// <summary>
/// Provides password hashing, validation, and generation utilities using BCrypt.
/// </summary>
public static partial class PasswordHasher
{
    private const int MaximumIdenticalConsecutiveChars = 2;
    private const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericCharacters = "0123456789";
    private const string SpecialCharacters = @"!#$%&*@\";
    private const string SpaceCharacter = " ";
    private const int PasswordLengthMin = 8;
    private const int PasswordLengthMax = 128;

    /// <summary>
    /// Encrypts a password using BCrypt hashing algorithm.
    /// </summary>
    /// <param name="password">The plain text password to encrypt.</param>
    /// <returns>The hashed password.</returns>
    public static string EncryptPassword(this string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Validates a password against a BCrypt hash.
    /// </summary>
    /// <param name="passwordHash">The BCrypt hash to validate against.</param>
    /// <param name="password">The plain text password to validate.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public static bool ValidatePassword(this string passwordHash, string password)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty.", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    /// <summary>
    /// Generates a random password with specified character requirements.
    /// </summary>
    /// <param name="includeLowercase">Include lowercase letters.</param>
    /// <param name="includeUppercase">Include uppercase letters.</param>
    /// <param name="includeNumeric">Include numeric characters.</param>
    /// <param name="includeSpecial">Include special characters.</param>
    /// <param name="includeSpaces">Include spaces.</param>
    /// <param name="lengthOfPassword">The desired password length (8-128 characters).</param>
    /// <returns>A randomly generated password matching the specified criteria.</returns>
    /// <exception cref="ArgumentException">Thrown when the password length is invalid or no character sets are selected.</exception>
    public static string GeneratePassword(
        bool includeLowercase,
        bool includeUppercase,
        bool includeNumeric,
        bool includeSpecial,
        bool includeSpaces,
        int lengthOfPassword)
    {
        if (lengthOfPassword < PasswordLengthMin || lengthOfPassword > PasswordLengthMax)
        {
            throw new ArgumentException($"Password length must be between {PasswordLengthMin} and {PasswordLengthMax}.", nameof(lengthOfPassword));
        }

        var characterSet = new StringBuilder();

        if (includeLowercase) characterSet.Append(LowercaseCharacters);
        if (includeUppercase) characterSet.Append(UppercaseCharacters);
        if (includeNumeric) characterSet.Append(NumericCharacters);
        if (includeSpecial) characterSet.Append(SpecialCharacters);
        if (includeSpaces) characterSet.Append(SpaceCharacter);

        if (characterSet.Length == 0)
        {
            throw new ArgumentException("At least one character set must be selected.");
        }

        var password = new char[lengthOfPassword];
        var random = new Random();
        var characterSetString = characterSet.ToString();

        for (int characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++)
        {
            password[characterPosition] = characterSetString[random.Next(characterSetString.Length)];

            bool moreThanTwoIdenticalInARow =
                characterPosition > MaximumIdenticalConsecutiveChars
                && password[characterPosition] == password[characterPosition - 1]
                && password[characterPosition - 1] == password[characterPosition - 2];

            if (moreThanTwoIdenticalInARow)
            {
                characterPosition--;
            }
        }

        var generatedPassword = new string(password);

        return !PasswordIsValid(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, generatedPassword)
            ? GeneratePassword(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, lengthOfPassword)
            : generatedPassword;
    }

    /// <summary>
    /// Validates that a password meets the specified character requirements.
    /// </summary>
    /// <param name="includeLowercase">Require lowercase letters.</param>
    /// <param name="includeUppercase">Require uppercase letters.</param>
    /// <param name="includeNumeric">Require numeric characters.</param>
    /// <param name="includeSpecial">Require special characters.</param>
    /// <param name="includeSpaces">Require spaces.</param>
    /// <param name="password">The password to validate.</param>
    /// <returns>True if the password meets all specified requirements; otherwise, false.</returns>
    public static bool PasswordIsValid(
        bool includeLowercase,
        bool includeUppercase,
        bool includeNumeric,
        bool includeSpecial,
        bool includeSpaces,
        string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        const string regexLowercase = @"[a-z]";
        const string regexUppercase = @"[A-Z]";
        const string regexNumeric = @"[0-9]";
        const string regexSpecial = @"([!#$%&*@\\])+";
        const string regexSpace = @"([ ])+";

        bool lowerCaseIsValid = !includeLowercase || Regex.IsMatch(password, regexLowercase);
        bool upperCaseIsValid = !includeUppercase || Regex.IsMatch(password, regexUppercase);
        bool numericIsValid = !includeNumeric || Regex.IsMatch(password, regexNumeric);
        bool symbolsAreValid = !includeSpecial || Regex.IsMatch(password, regexSpecial);
        bool spacesAreValid = !includeSpaces || Regex.IsMatch(password, regexSpace);

        return lowerCaseIsValid && upperCaseIsValid && numericIsValid && symbolsAreValid && spacesAreValid;
    }
}
