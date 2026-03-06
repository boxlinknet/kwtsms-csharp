# OTP Flow

Demonstrates sending one-time passwords (OTP) with proper validation, error handling, and user-facing error messages.

## Key Points

- Always include app/company name in OTP messages: `"Your OTP for APPNAME is: 123456"`.
- Send OTP to a single number per request (never batch OTP sends).
- Use a Transactional Sender ID for OTP (not Promotional). Promotional IDs are blocked by DND on Zain and Ooredoo.
- Set a 3-4 minute resend timer (KNET standard is 4 minutes).
- OTP expiry: 3-5 minutes.
- Generate a new code on each resend and invalidate previous codes.
- Map API errors to user-friendly messages. Never show raw error codes to end users.
