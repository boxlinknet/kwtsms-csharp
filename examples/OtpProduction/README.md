# Production OTP Service

A complete, production-grade OTP service built on KwtSMS with rate limiting, proper error mapping, and storage abstraction.

## Features

- Rate limiting per phone number (configurable, default 5/hour)
- Rate limiting per IP address (configurable, default 20/hour)
- Resend cooldown enforcement (default 4 minutes, KNET standard)
- OTP expiry (default 5 minutes)
- Max verification attempts (5 tries before invalidation)
- New code on every resend (previous codes invalidated)
- User-friendly error messages (API errors never shown to end users)
- Admin alerting via `InternalError` field on system-level failures

## Architecture

```
IOtpStore (interface)
    InMemoryOtpStore  (dev/testing)
    RedisOtpStore     (production, implement yourself)
    SqlOtpStore        (production, implement yourself)
```

## Usage with ASP.NET Core

```csharp
// Program.cs
builder.Services.AddSingleton<KwtSmsClient>(sp => KwtSmsClient.FromEnv());
builder.Services.AddSingleton<IOtpStore, InMemoryOtpStore>(); // Use Redis in production
builder.Services.AddSingleton<OtpService>();

// Endpoint
app.MapPost("/api/request-otp", (OtpService otp, HttpContext ctx) =>
{
    var body = ctx.Request.ReadFromJsonAsync<OtpRequest>().Result;
    var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    return otp.RequestOtp(body.Phone, ip);
});

app.MapPost("/api/verify-otp", (OtpService otp, HttpContext ctx) =>
{
    var body = ctx.Request.ReadFromJsonAsync<VerifyRequest>().Result;
    return otp.VerifyOtp(body.Phone, body.Code);
});
```

## Before Going Live

```
[ ] Replace InMemoryOtpStore with Redis or SQL
[ ] Add CAPTCHA before the OTP request endpoint
[ ] Use Transactional Sender ID (not Promotional, not KWT-SMS)
[ ] Set KWTSMS_TEST_MODE=0
[ ] Configure admin alerts for InternalError results
[ ] Use cryptographic random for OTP generation (RandomNumberGenerator)
```
