# KwtSMS C# Client

[![CI](https://github.com/boxlinknet/kwtsms-csharp/actions/workflows/ci.yml/badge.svg)](https://github.com/boxlinknet/kwtsms-csharp/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/KwtSMS.svg)](https://www.nuget.org/packages/KwtSMS)
[![NuGet Downloads](https://img.shields.io/nuget/dt/KwtSMS.svg)](https://www.nuget.org/packages/KwtSMS)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![CodeQL](https://github.com/boxlinknet/kwtsms-csharp/actions/workflows/codeql.yml/badge.svg)](https://github.com/boxlinknet/kwtsms-csharp/actions/workflows/codeql.yml)

C# client for the [kwtSMS](https://www.kwtsms.com) SMS gateway. Send SMS, check balance, validate numbers, and more. Zero dependencies on .NET 6+.

## Install

```bash
dotnet add package KwtSMS
```

## Quick Start

```csharp
using KwtSMS;

// Load credentials from environment variables or .env file
var sms = KwtSmsClient.FromEnv();

// Verify credentials and check balance
var (ok, balance, error) = sms.Verify();
Console.WriteLine($"Balance: {balance} credits");

// Send SMS
var result = sms.Send("96598765432", "Your OTP for MyApp is: 123456");
if (result.Result == "OK")
{
    Console.WriteLine($"Sent! Message ID: {result.MsgId}");
    Console.WriteLine($"Balance after: {result.BalanceAfter}");
}
```

## Setup

Create a `.env` file in your project root:

```ini
KWTSMS_USERNAME=your_api_user
KWTSMS_PASSWORD=your_api_pass
KWTSMS_SENDER_ID=YOUR-SENDERID   # Use KWT-SMS for testing only
KWTSMS_TEST_MODE=1                # 1=test (safe), 0=live
KWTSMS_LOG_FILE=kwtsms.log        # or "" to disable
```

Add `.env` to your `.gitignore`:
```
.env
```

Or pass credentials directly:

```csharp
var sms = new KwtSmsClient("your_api_user", "your_api_pass", "YOUR-SENDERID", testMode: true);
```

## API Methods

### Verify Credentials

```csharp
var (ok, balance, error) = sms.Verify();
// ok: true if credentials are valid
// balance: available credits
// error: error message if failed
```

### Send SMS

```csharp
// Single number
var result = sms.Send("96598765432", "Hello from C#");

// Multiple numbers (comma-separated)
var result = sms.Send("96598765432,96512345678", "Bulk message");

// Multiple numbers (array)
var result = sms.Send(new[] { "96598765432", "96512345678" }, "Bulk message");

// Override sender ID
var result = sms.Send("96598765432", "Hello", sender: "MY-APP");
```

The `Send` method automatically:
- Normalizes phone numbers (strips `+`, `00`, spaces, dashes, converts Arabic digits)
- Deduplicates numbers before sending
- Cleans message text (strips emojis, HTML, hidden characters, converts Arabic digits)
- Validates numbers locally before calling the API
- For >200 numbers: auto-batches with 0.5s delay, retries ERR013 with exponential backoff

### Send with Retry (ERR028)

```csharp
// Automatically retries on ERR028 (rate limit), sleeping 16 seconds between attempts
var result = sms.SendWithRetry("96598765432", "OTP: 123456", maxRetries: 3);
```

### Check Balance

```csharp
double? balance = sms.Balance();
// Returns cached value on API failure
```

### Validate Numbers

```csharp
var result = sms.Validate("96598765432", "+96512345678", "invalid");
// result.Ok   = valid and routable numbers
// result.Er   = numbers with format errors
// result.Nr   = numbers with no route (country not activated)
// result.Rejected = numbers that failed local validation
```

### List Sender IDs

```csharp
var result = sms.SenderIds();
// result.SenderIds = ["KWT-SMS", "MY-APP", ...]
```

### Coverage (Active Countries)

```csharp
var result = sms.Coverage();
// result.Prefixes = ["965", "966", "971", ...]
```

### Message Status

```csharp
var result = sms.Status("msg-id-from-send-response");
// result.Status = "sent"
// result.Description = "Message successfully sent"
```

### Delivery Report (International Only)

```csharp
// DLR is only available for international (non-Kuwait) numbers.
// Wait at least 5 minutes after sending before checking.
var result = sms.Dlr("msg-id-from-send-response");
// result.Report = [{ Number: "971504496677", Status: "Received by recipient" }]
```

## Utility Functions

```csharp
using KwtSMS;

// Normalize phone number (digits only, no leading zeros)
string normalized = PhoneUtils.NormalizePhone("+965 9876-5432");
// "96598765432"

// Validate phone number
var validation = PhoneUtils.ValidatePhoneInput("user@email.com");
// validation.IsValid = false
// validation.Error = "'user@email.com' is an email address, not a phone number"

// Clean message text
string cleaned = MessageUtils.CleanMessage("Hello \U0001F600 World <b>bold</b>");
// "Hello  World bold"

// Access all error codes
foreach (var kvp in ApiErrors.Errors)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
```

## Error Handling

Every error response includes a developer-friendly `Action` field:

```csharp
var result = sms.Send("96598765432", "Test");
if (result.Result == "ERROR")
{
    Console.WriteLine($"Code: {result.Code}");           // "ERR003"
    Console.WriteLine($"Description: {result.Description}"); // "Authentication error..."
    Console.WriteLine($"Action: {result.Action}");       // "Wrong API username or password..."
}
```

The library never throws exceptions. All methods return structured results.

## Cached Balance

The client caches balance from `Verify()` and `Send()` responses:

```csharp
sms.Verify();
Console.WriteLine($"Balance: {sms.CachedBalance}");
Console.WriteLine($"Purchased: {sms.CachedPurchased}");
```

Never call `Balance()` after `Send()`. The send response already includes your updated balance in `BalanceAfter`. Save it to your database.

## ASP.NET Core Integration

Register `KwtSmsClient` as a singleton (thread-safe, reuses HttpClient):

```csharp
builder.Services.AddSingleton<KwtSmsClient>(sp => KwtSmsClient.FromEnv());
```

See [examples/AspNetEndpoint](examples/AspNetEndpoint/) for a complete endpoint example.

## Credential Management

API credentials must NEVER be hardcoded. Recommended approaches:

1. **Environment variables / .env file** (default): Use `KwtSmsClient.FromEnv()`. The `.env` file is gitignored and editable without redeployment.

2. **Admin settings UI** (web apps): Provide a settings page where an admin can update credentials and test the connection with `Verify()`.

3. **Secrets manager** (production): Load from Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault. Pass to the constructor.

4. **Constructor injection** (custom config): Pass credentials directly from your DI container.

```csharp
// Azure Key Vault example
var username = await secretClient.GetSecretAsync("kwtsms-username");
var password = await secretClient.GetSecretAsync("kwtsms-password");
var sms = new KwtSmsClient(username.Value.Value, password.Value.Value, "MY-APP");
```

## Server Timezone

The `unix-timestamp` values in API responses are GMT+3 (Asia/Kuwait server time), NOT UTC. Convert when storing or displaying:

```csharp
var serverTime = DateTimeOffset.FromUnixTimeSeconds(result.UnixTimestamp!.Value);
// This timestamp is in GMT+3. Adjust if you need UTC:
var utcTime = serverTime.AddHours(-3);
```

## Best Practices

### Validate Before Calling the API

```csharp
// Validate phone locally first
var validation = PhoneUtils.ValidatePhoneInput(userInput);
if (!validation.IsValid)
{
    return new { error = validation.Error };
}

// Clean message
var message = MessageUtils.CleanMessage(userMessage);
if (string.IsNullOrWhiteSpace(message))
{
    return new { error = "Message is empty after cleaning." };
}

// Only valid, clean input reaches the API
var result = sms.Send(validation.Normalized, message);
```

### User-Facing Error Messages

Never expose raw API errors to end users:

| Situation | API Error | User Message |
|-----------|-----------|-------------|
| Invalid phone | ERR006, ERR025 | "Please enter a valid phone number in international format." |
| Wrong credentials | ERR003 | "SMS service is temporarily unavailable." (log + alert admin) |
| No balance | ERR010, ERR011 | "SMS service is temporarily unavailable." (alert admin to top up) |
| Rate limited | ERR028 | "Please wait a moment before requesting another code." |
| Message rejected | ERR031, ERR032 | "Your message could not be sent. Please try with different content." |

### OTP Requirements

- Always include app name: `"Your OTP for APPNAME is: 123456"`
- Use Transactional Sender ID (not Promotional, not `KWT-SMS`)
- Minimum 3-4 minute resend timer (KNET standard: 4 minutes)
- OTP expiry: 3-5 minutes
- Generate a new code on every resend, invalidate previous codes
- Send to a single number per request (never batch OTP sends)

### Sender ID

| | Promotional | Transactional |
|--|-------------|---------------|
| Use for | Bulk SMS, marketing, offers | OTP, alerts, notifications |
| DND delivery | Blocked (credits lost) | Bypasses DND |
| Speed | May have delays | Priority delivery |
| Cost | 10 KD one-time | 15 KD one-time |

`KWT-SMS` is a shared test sender: delays, blocked on Virgin Kuwait. Never use in production. Register your own at [kwtsms.com](https://www.kwtsms.com). Sender ID is case-sensitive.

## Security Checklist

```
BEFORE GOING LIVE:
[ ] Bot protection enabled (CAPTCHA for web, Device Attestation for mobile)
[ ] Rate limit per phone number (max 3-5/hour)
[ ] Rate limit per IP address (max 10-20/hour)
[ ] Rate limit per user/session if authenticated
[ ] Monitoring/alerting on abuse patterns
[ ] Admin notification on low balance
[ ] Test mode OFF (KWTSMS_TEST_MODE=0)
[ ] Private Sender ID registered (not KWT-SMS)
[ ] Transactional Sender ID for OTP (not promotional)
```

## Compatibility

| Target | Dependencies |
|--------|-------------|
| .NET 8.0+ | Zero |
| .NET 6.0+ | Zero |
| .NET Standard 2.0 (.NET Framework 4.6.1+) | System.Text.Json |

## Examples

| Example | Description |
|---------|-------------|
| [BasicUsage](examples/BasicUsage/) | Connect, verify, send SMS |
| [OtpFlow](examples/OtpFlow/) | OTP with proper error handling |
| [BulkSms](examples/BulkSms/) | Auto-batching for >200 numbers |
| [AspNetEndpoint](examples/AspNetEndpoint/) | ASP.NET Core minimal API |
| [ErrorHandling](examples/ErrorHandling/) | Error codes, validation, cleaning |
| [OtpProduction](examples/OtpProduction/) | Production OTP with rate limiting |

## Links

- [kwtSMS website](https://www.kwtsms.com)
- [API documentation (PDF)](https://www.kwtsms.com/doc/KwtSMS.com_API_Documentation_v41.pdf)
- [Implementation best practices](https://www.kwtsms.com/articles/sms-api-implementation-best-practices.html)
- [Integration test checklist](https://www.kwtsms.com/articles/sms-api-integration-test-checklist.html)
- [Sender ID help](https://www.kwtsms.com/sender-id-help.html)
- [Support (WhatsApp)](https://wa.me/96599220322)

## License

MIT
