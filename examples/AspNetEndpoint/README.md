# ASP.NET Core Endpoint

Demonstrates integrating KwtSMS into an ASP.NET Core minimal API for OTP delivery.

## Key Points

- Register `KwtSmsClient` as a singleton in DI (thread-safe, reuses HttpClient).
- Validate phone numbers locally before calling the API.
- Map API error codes to user-friendly messages. Never expose raw error codes.
- Add rate limiting per phone number (3-5/hour) and per IP (10-20/hour) before going live.
- Add CAPTCHA (Cloudflare Turnstile, hCaptcha, or reCAPTCHA) before the SMS endpoint.
- Never return the OTP in the API response.
- Set a 3-4 minute resend timer on the client side.

## Before Going Live

```
[ ] Bot protection enabled (CAPTCHA)
[ ] Rate limit per phone number (max 3-5/hour)
[ ] Rate limit per IP address (max 10-20/hour)
[ ] Rate limit per user/session if authenticated
[ ] Monitoring/alerting on abuse patterns
[ ] Admin notification on low balance
[ ] Test mode OFF (KWTSMS_TEST_MODE=0)
[ ] Private Sender ID registered (not KWT-SMS)
[ ] Transactional Sender ID for OTP (not promotional)
```
