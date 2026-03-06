using System;
using System.Text;
using System.Text.RegularExpressions;

namespace KwtSMS
{
    /// <summary>
    /// Phone number normalization and validation utilities.
    /// </summary>
    public static class PhoneUtils
    {
        /// <summary>
        /// Normalize a phone number to kwtSMS-accepted format (digits only, no leading zeros).
        /// Converts Arabic-Indic and Extended Arabic-Indic digits to Latin,
        /// strips all non-digit characters, and removes leading zeros.
        /// </summary>
        /// <param name="phone">Raw phone number input.</param>
        /// <returns>Normalized phone number string.</returns>
        public static string NormalizePhone(string? phone)
        {
            if (string.IsNullOrEmpty(phone)) return "";

            var sb = new StringBuilder(phone!.Length);

            foreach (var ch in phone)
            {
                // Arabic-Indic digits U+0660-U+0669
                if (ch >= '\u0660' && ch <= '\u0669')
                {
                    sb.Append((char)('0' + (ch - '\u0660')));
                }
                // Extended Arabic-Indic / Persian digits U+06F0-U+06F9
                else if (ch >= '\u06F0' && ch <= '\u06F9')
                {
                    sb.Append((char)('0' + (ch - '\u06F0')));
                }
                // Latin digits
                else if (ch >= '0' && ch <= '9')
                {
                    sb.Append(ch);
                }
                // Everything else is stripped
            }

            // Strip leading zeros
            var result = sb.ToString().TrimStart('0');
            return result;
        }

        /// <summary>
        /// Validate a raw phone number input before sending to the API.
        /// Returns validation result with error message and normalized number.
        /// Never throws exceptions.
        /// </summary>
        /// <param name="phone">Raw phone number input (any type, coerced to string).</param>
        /// <returns>Validation result containing IsValid, Error, and Normalized fields.</returns>
        public static PhoneValidationResult ValidatePhoneInput(object? phone)
        {
            var input = phone?.ToString() ?? "";
            input = input.Trim();

            // Empty or blank
            if (string.IsNullOrWhiteSpace(input))
            {
                return new PhoneValidationResult
                {
                    IsValid = false,
                    Error = "Phone number is required",
                    Normalized = ""
                };
            }

            // Email address detection
            if (input.Contains("@"))
            {
                return new PhoneValidationResult
                {
                    IsValid = false,
                    Error = $"'{input}' is an email address, not a phone number",
                    Normalized = ""
                };
            }

            // Normalize
            var normalized = NormalizePhone(input);

            // No digits found
            if (string.IsNullOrEmpty(normalized))
            {
                return new PhoneValidationResult
                {
                    IsValid = false,
                    Error = $"'{input}' is not a valid phone number, no digits found",
                    Normalized = ""
                };
            }

            // Too short (minimum 7 digits per E.164)
            if (normalized.Length < 7)
            {
                return new PhoneValidationResult
                {
                    IsValid = false,
                    Error = $"'{input}' is too short ({normalized.Length} digits, minimum is 7)",
                    Normalized = normalized
                };
            }

            // Too long (maximum 15 digits per E.164)
            if (normalized.Length > 15)
            {
                return new PhoneValidationResult
                {
                    IsValid = false,
                    Error = $"'{input}' is too long ({normalized.Length} digits, maximum is 15)",
                    Normalized = normalized
                };
            }

            return new PhoneValidationResult
            {
                IsValid = true,
                Error = null,
                Normalized = normalized
            };
        }
    }
}
