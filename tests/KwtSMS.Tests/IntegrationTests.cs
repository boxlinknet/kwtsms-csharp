using System;
using System.IO;
using System.Linq;
using Xunit;
using KwtSMS;

namespace KwtSMS.Tests
{
    /// <summary>
    /// Real API integration tests. Skipped if CSHARP_USERNAME / CSHARP_PASSWORD are not set.
    /// Always uses test_mode=true (no credits consumed, messages not delivered).
    /// </summary>
    [Trait("Category", "Integration")]
    [Collection("HttpClient")]
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
            var client = new KwtSmsClient("csharp_wrong_user", "csharp_wrong_pass", testMode: true, logFile: "");

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

        // ── Bulk Send: Client Library ──

        [SkippableFact]
        public void Integration_BulkSend_250Numbers_ClientLibrary()
        {
            SkipIfNoCredentials();
            var client = CreateClient();

            // 1. Record initial balance
            var (verifyOk, initialBalance, verifyError) = client.Verify();
            Assert.True(verifyOk, $"Verify failed: {verifyError}");
            Assert.NotNull(initialBalance);
            var balanceBefore = initialBalance!.Value;

            // 2. Generate 250 numbers: 96599220000 to 96599220249
            var numbers = new string[250];
            for (int i = 0; i < 250; i++)
                numbers[i] = (96599220000L + i).ToString();
            var mobile = string.Join(",", numbers);

            // 3. Single Send() call → internally batches into 200 + 50
            var result = client.Send(mobile, "C# client bulk test (250 numbers, test mode)");

            // 4. Verify result
            Assert.Equal("OK", result.Result);
            Assert.NotNull(result.MsgId);

            // Should have 2 comma-separated msg-ids (batch 1: 200, batch 2: 50)
            var msgIds = result.MsgId!.Split(',');
            Assert.Equal(2, msgIds.Length);
            foreach (var id in msgIds)
                Assert.False(string.IsNullOrWhiteSpace(id), "Each msg-id should be non-empty");

            // Numbers accepted should be 250
            Assert.Equal(250, result.Numbers);

            // Points charged should reflect 250 messages
            Assert.NotNull(result.PointsCharged);
            Assert.True(result.PointsCharged > 0, $"PointsCharged should be > 0, got {result.PointsCharged}");

            // 5. Balance tracking
            Assert.NotNull(result.BalanceAfter);
            var balanceAfterSend = result.BalanceAfter!.Value;
            // Balance should decrease by exactly PointsCharged
            var expectedBalance = balanceBefore - result.PointsCharged!.Value;
            Assert.True(expectedBalance == balanceAfterSend,
                $"Balance before ({balanceBefore}) - PointsCharged ({result.PointsCharged}) = {expectedBalance}, but BalanceAfter = {balanceAfterSend}");
            // CachedBalance should match the final balance
            Assert.Equal(balanceAfterSend, client.CachedBalance);

            // 6. Check status of each msg-id: ERR030 expected (test mode = stuck in queue)
            foreach (var msgId in msgIds)
            {
                var status = client.Status(msgId.Trim());
                Assert.Equal("ERROR", status.Result);
                Assert.Equal("ERR030", status.Code);
            }
        }

        // ── Bulk Send: CLI Tool ──

        [SkippableFact]
        public void Integration_BulkSend_250Numbers_CliTool()
        {
            SkipIfNoCredentials();

            // 1. Generate 250 numbers: 96599220000 to 96599220249
            var numbers = new string[250];
            for (int i = 0; i < 250; i++)
                numbers[i] = (96599220000L + i).ToString();
            var mobile = string.Join(",", numbers);

            // 2. Set KWTSMS env vars for CLI's FromEnv()
            var originalOut = Console.Out;
            var originalErr = Console.Error;

            Environment.SetEnvironmentVariable("KWTSMS_USERNAME", _username);
            Environment.SetEnvironmentVariable("KWTSMS_PASSWORD", _password);
            Environment.SetEnvironmentVariable("KWTSMS_TEST_MODE", "1");
            Environment.SetEnvironmentVariable("KWTSMS_LOG_FILE", "");

            string sendStdout, sendStderr;
            int sendExitCode;

            var outWriter = new StringWriter();
            var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);
            try
            {
                sendExitCode = KwtSMS.Cli.Program.Main(new[] { "send", mobile, "C# CLI bulk test (250 numbers)" });
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
            }
            sendStdout = outWriter.ToString();
            sendStderr = errWriter.ToString();

            // 3. Verify CLI send output
            Assert.Equal(0, sendExitCode);
            Assert.Contains("WARNING", sendStdout);
            Assert.Contains("Test mode", sendStdout);
            Assert.Contains("OK", sendStdout);
            Assert.Contains("Message ID:", sendStdout);
            Assert.Contains("Numbers:", sendStdout);
            Assert.Contains("Balance after:", sendStdout);

            // Numbers should show 250
            var numbersLine = sendStdout.Split('\n')
                .FirstOrDefault(l => l.Contains("Numbers:"));
            Assert.NotNull(numbersLine);
            Assert.Contains("250", numbersLine);

            // 4. Extract msg-ids from "Message ID:     id1,id2"
            var msgIdLine = sendStdout.Split('\n')
                .FirstOrDefault(l => l.Contains("Message ID:"));
            Assert.NotNull(msgIdLine);
            var msgIdValue = msgIdLine!.Substring(msgIdLine.IndexOf("Message ID:") + "Message ID:".Length).Trim();
            var msgIds = msgIdValue.Split(',');
            Assert.Equal(2, msgIds.Length);

            // 5. Check status of each msg-id via CLI → expect ERR030
            foreach (var msgId in msgIds)
            {
                var trimmedId = msgId.Trim();
                Assert.False(string.IsNullOrEmpty(trimmedId));

                Environment.SetEnvironmentVariable("KWTSMS_USERNAME", _username);
                Environment.SetEnvironmentVariable("KWTSMS_PASSWORD", _password);
                Environment.SetEnvironmentVariable("KWTSMS_TEST_MODE", "1");
                Environment.SetEnvironmentVariable("KWTSMS_LOG_FILE", "");

                var statusOut = new StringWriter();
                var statusErr = new StringWriter();
                Console.SetOut(statusOut);
                Console.SetError(statusErr);

                int statusExitCode;
                try
                {
                    statusExitCode = KwtSMS.Cli.Program.Main(new[] { "status", trimmedId });
                }
                finally
                {
                    Console.SetOut(originalOut);
                    Console.SetError(originalErr);
                }

                var statusStderr = statusErr.ToString();

                // ERR030 = message stuck in queue (expected for test mode)
                Assert.Equal(1, statusExitCode);
                Assert.Contains("ERR030", statusStderr);
            }

            // 6. Clean up env vars
            Environment.SetEnvironmentVariable("KWTSMS_USERNAME", null);
            Environment.SetEnvironmentVariable("KWTSMS_PASSWORD", null);
            Environment.SetEnvironmentVariable("KWTSMS_TEST_MODE", null);
            Environment.SetEnvironmentVariable("KWTSMS_LOG_FILE", null);
        }
    }
}
