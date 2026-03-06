# Error Handling

Demonstrates comprehensive error handling patterns: phone validation, message cleaning, API errors with action guidance, and user-facing error mapping.

## Key Points

- Every API error response includes an `Action` field with developer-friendly guidance.
- Use `PhoneUtils.ValidatePhoneInput()` to validate numbers locally before calling the API.
- Use `MessageUtils.CleanMessage()` to sanitize message content (emojis, HTML, hidden characters).
- Access all error codes via `ApiErrors.Errors` for building custom error UIs.
- The library never throws exceptions. All methods return structured results.
