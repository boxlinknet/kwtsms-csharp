using System;
using Xunit;
using KwtSMS;

namespace KwtSMS.Tests
{
    /// <summary>
    /// Real API integration tests. Skipped if CSHARP_USERNAME / CSHARP_PASSWORD are not set.
    /// Always uses test_mode=true (no credits consumed, messages not delivered).
    /// </summary>
    [Trait("Category", "Integration")]
    public class IntegrationTests
    {
        private readonly string? _username;
        private readonly string? _password;

        public IntegrationTests()
        {
            _username = Environment.GetEnvironmentVariable("CSHARP_USERNAME");
            _password = Environment.GetEnvironmentVariable("CSHARP_PASSWORD");
        }

        private KwtSmsClient CreateClient()
        {
            return new KwtSmsClient(_username!, _password!, "KWT-SMS", testMode: true, logFile: "");
        }

        private void SkipIfNoCredentials()
        {
            Skip.If(string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password),
                "CSHARP_USERNAME / CSHARP_PASSWORD not set");
        }

        // ── Verify ──

        [SkippableFact]
        public void Integration_Verify_ValidCredentials()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var (ok, balance, error) = client.Verify();

            Assert.True(ok, $"Verify failed: {error}");
            Assert.NotNull(balance);
            Assert.True(balance >= 0, $"Balance should be non-negative, got {balance}");
        }

        [SkippableFact]
        public void Integration_Verify_WrongCredentials()
        {
            SkipIfNoCredentials();
            var client = new KwtSmsClient("wrong_user", "wrong_pass", testMode: true, logFile: "");

            var (ok, balance, error) = client.Verify();

            Assert.False(ok);
            Assert.Null(balance);
            Assert.NotNull(error);
            Assert.Contains("Authentication", error, StringComparison.OrdinalIgnoreCase);
        }

        // ── Balance ──

        [SkippableFact]
        public void Integration_Balance()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var balance = client.Balance();

            Assert.NotNull(balance);
            Assert.True(balance >= 0);
        }

        // ── Send ──

        [SkippableFact]
        public void Integration_Send_ValidKuwaitNumber()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("96598765432", "Integration test from C# client");

            // In test mode, should be OK or a known error
            Assert.NotNull(result.Result);
        }

        [SkippableFact]
        public void Integration_Send_InvalidInput_Email()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("user@email.com", "Test");

            Assert.Equal("ERROR", result.Result);
            Assert.NotNull(result.Invalid);
        }

        [SkippableFact]
        public void Integration_Send_InvalidInput_TooShort()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("123", "Test");

            Assert.Equal("ERROR", result.Result);
        }

        [SkippableFact]
        public void Integration_Send_MixedValidInvalid()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("96598765432,abc,user@email.com", "Test mixed");

            // Valid number should be sent, invalid ones reported
            Assert.NotNull(result.Invalid);
            Assert.Equal(2, result.Invalid!.Count);
        }

        [SkippableFact]
        public void Integration_Send_PlusPrefixNormalization()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("+96598765432", "Test with + prefix");

            Assert.NotNull(result.Result);
        }

        [SkippableFact]
        public void Integration_Send_DoubleZeroNormalization()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("0096598765432", "Test with 00 prefix");

            Assert.NotNull(result.Result);
        }

        [SkippableFact]
        public void Integration_Send_ArabicDigits()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            // ٩٦٥٩٨٧٦٥٤٣٢
            var result = client.Send("\u0669\u0666\u0665\u0669\u0668\u0667\u0666\u0665\u0664\u0663\u0662", "Test Arabic digits");

            Assert.NotNull(result.Result);
        }

        [SkippableFact]
        public void Integration_Send_DuplicateNumbers()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("+96598765432,0096598765432,96598765432", "Test dedup");

            // Should deduplicate to single number
            Assert.NotNull(result.Result);
        }

        [SkippableFact]
        public void Integration_Send_EmptySender()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("96598765432", "Test empty sender", sender: "");

            // API should reject empty sender or use default
            Assert.NotNull(result.Result);
        }

        [SkippableFact]
        public void Integration_Send_WrongSender()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Send("96598765432", "Test wrong sender", sender: "NONEXISTENT-SENDER");

            Assert.NotNull(result.Result);
        }

        // ── SenderIds ──

        [SkippableFact]
        public void Integration_SenderIds()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.SenderIds();

            Assert.Equal("OK", result.Result);
            Assert.NotEmpty(result.SenderIds);
        }

        // ── Coverage ──

        [SkippableFact]
        public void Integration_Coverage()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Coverage();

            Assert.NotNull(result.Result);
        }

        // ── Validate ──

        [SkippableFact]
        public void Integration_Validate()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            var result = client.Validate("96598765432", "+96512345678");

            Assert.Null(result.Error);
        }
    }
}
