# Examples

| Example | Description |
|---------|-------------|
| [RawApi](RawApi/) | Call every kwtSMS endpoint directly with HttpClient (no library needed) |
| [BasicUsage](BasicUsage/) | Connect, verify credentials, send a single SMS |
| [OtpFlow](OtpFlow/) | Send OTP codes with proper error handling and user-facing messages |
| [BulkSms](BulkSms/) | Send to multiple numbers with auto-batching |
| [AspNetEndpoint](AspNetEndpoint/) | ASP.NET Core minimal API endpoint for OTP delivery |
| [ErrorHandling](ErrorHandling/) | Comprehensive error handling: validation, cleaning, error codes |
| [OtpProduction](OtpProduction/) | Production-grade OTP service with rate limiting, storage abstraction, and resend timers |

## Prerequisites

- .NET 6.0 or later
- kwtSMS API credentials ([sign up at kwtsms.com](https://www.kwtsms.com))
- A `.env` file with your credentials (see [main README](../README.md))

## Running Examples

Examples are standalone code files, not runnable projects. Copy the relevant code into your application or create a console project to try them.
