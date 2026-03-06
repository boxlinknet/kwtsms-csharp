# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-03-06

### Added

- Initial release of KwtSMS C# client library.
- `KwtSmsClient` class with all API methods: `Send`, `Balance`, `Verify`, `Validate`, `SenderIds`, `Coverage`, `Status`, `Dlr`.
- `FromEnv()` factory method for loading credentials from environment variables or `.env` files.
- `PhoneUtils.NormalizePhone()` for phone number normalization (Arabic digits, stripping non-digits, removing leading zeros).
- `PhoneUtils.ValidatePhoneInput()` for local phone validation (empty, email, too short, too long, no digits).
- `MessageUtils.CleanMessage()` for SMS content sanitization (emojis, HTML, hidden characters, Arabic digit conversion).
- `ApiErrors.Errors` dictionary with all 29+ kwtSMS error codes mapped to developer-friendly action messages.
- `ApiErrors.EnrichError()` for adding action guidance to error responses.
- Auto-batching for bulk sends (>200 numbers) with 0.5s delay between batches.
- ERR013 (queue full) automatic retry with exponential backoff (30s, 60s, 120s).
- `SendWithRetry()` for automatic ERR028 (rate limit) retry with 16s delay.
- Phone number deduplication before sending.
- JSONL logging with password masking.
- Thread-safe cached balance tracking.
- Multi-target: netstandard2.0, net6.0, net8.0.
- Comprehensive test suite: unit tests, mocked API tests, real integration tests.
- CI/CD with GitHub Actions (test on .NET 6/8/9, publish to NuGet on tag).
- CodeQL security analysis.
- Dependabot for automated dependency updates.
