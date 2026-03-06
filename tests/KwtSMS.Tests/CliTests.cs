using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using Xunit;
using KwtSMS;

namespace KwtSMS.Tests
{
    /// <summary>
    /// Tests for the CLI tool (tools/KwtSMS.Cli).
    /// Captures stdout/stderr and verifies output and exit codes.
    /// Uses mocked HTTP to avoid real API calls.
    /// </summary>
    [Collection("HttpClient")]
    public class CliTests : IDisposable
    {
        private readonly HttpClient _originalClient;
        private readonly TextWriter _originalOut;
        private readonly TextWriter _originalErr;

        public CliTests()
        {
            _originalClient = HttpRequest.Client;
            _originalOut = Console.Out;
            _originalErr = Console.Error;
        }

        public void Dispose()
        {
            HttpRequest.Client = _originalClient;
            Console.SetOut(_originalOut);
            Console.SetError(_originalErr);
        }

        private (int exitCode, string stdout, string stderr) RunCli(string[] args, Func<HttpRequestMessage, HttpResponseMessage>? handler = null)
        {
            if (handler != null)
            {
                var mockHandler = new MockHttpHandler(handler);
                HttpRequest.Client = new HttpClient(mockHandler) { Timeout = TimeSpan.FromSeconds(15) };
            }

            // Set env vars so FromEnv() works
            Environment.SetEnvironmentVariable("KWTSMS_USERNAME", "testuser");
            Environment.SetEnvironmentVariable("KWTSMS_PASSWORD", "testpass");
            Environment.SetEnvironmentVariable("KWTSMS_TEST_MODE", "1");
            Environment.SetEnvironmentVariable("KWTSMS_LOG_FILE", "");

            var outWriter = new StringWriter();
            var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            int exitCode;
            try
            {
                exitCode = KwtSMS.Cli.Program.Main(args);
            }
            finally
            {
                Console.SetOut(_originalOut);
                Console.SetError(_originalErr);
                Environment.SetEnvironmentVariable("KWTSMS_USERNAME", null);
                Environment.SetEnvironmentVariable("KWTSMS_PASSWORD", null);
                Environment.SetEnvironmentVariable("KWTSMS_TEST_MODE", null);
                Environment.SetEnvironmentVariable("KWTSMS_LOG_FILE", null);
            }

            return (exitCode, outWriter.ToString(), errWriter.ToString());
        }

        // ── Help and version ──

        [Fact]
        public void Help_ShowsUsage()
        {
            var (exit, stdout, _) = RunCli(new[] { "--help" });
            Assert.Equal(0, exit);
            Assert.Contains("kwtsms", stdout);
            Assert.Contains("COMMANDS", stdout);
            Assert.Contains("setup", stdout);
            Assert.Contains("verify", stdout);
            Assert.Contains("send", stdout);
        }

        [Fact]
        public void Help_NoArgs_ShowsUsage()
        {
            var (exit, stdout, _) = RunCli(Array.Empty<string>());
            Assert.Equal(0, exit);
            Assert.Contains("USAGE", stdout);
        }

        [Fact]
        public void Version_ShowsVersion()
        {
            var (exit, stdout, _) = RunCli(new[] { "--version" });
            Assert.Equal(0, exit);
            Assert.Contains("kwtsms", stdout);
            Assert.Contains("0.2.0", stdout);
        }

        [Fact]
        public void UnknownCommand_ReturnsError()
        {
            var (exit, _, stderr) = RunCli(new[] { "foobar" });
            Assert.Equal(1, exit);
            Assert.Contains("Unknown command", stderr);
        }

        // ── Verify ──

        [Fact]
        public void Verify_Success_ShowsBalanceAndPurchased()
        {
            var (exit, stdout, _) = RunCli(new[] { "verify" }, req =>
            {
                var json = JsonSerializer.Serialize(new { result = "OK", available = 150.0, purchased = 1000.0 });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("OK", stdout);
            Assert.Contains("Balance:", stdout);
            Assert.Contains("150", stdout);
            Assert.Contains("Purchased:", stdout);
            Assert.Contains("1000", stdout);
        }

        [Fact]
        public void Verify_Error_ShowsErrorMessage()
        {
            var (exit, _, stderr) = RunCli(new[] { "verify" }, req =>
            {
                var json = JsonSerializer.Serialize(new { result = "ERROR", code = "ERR003", description = "Authentication error" });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(1, exit);
            Assert.Contains("ERROR", stderr);
        }

        // ── Balance ──

        [Fact]
        public void Balance_ShowsAvailableAndPurchased()
        {
            var (exit, stdout, _) = RunCli(new[] { "balance" }, req =>
            {
                var json = JsonSerializer.Serialize(new { result = "OK", available = 75.0, purchased = 500.0 });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("Available:", stdout);
            Assert.Contains("75", stdout);
            Assert.Contains("Purchased:", stdout);
            Assert.Contains("500", stdout);
        }

        // ── Send ──

        [Fact]
        public void Send_Success_ShowsResult()
        {
            var (exit, stdout, _) = RunCli(new[] { "send", "96598765432", "Hello test" }, req =>
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["result"] = "OK",
                    ["msg-id"] = "cli-test-123",
                    ["numbers"] = 1,
                    ["points-charged"] = 1,
                    ["balance-after"] = 149.0,
                    ["unix-timestamp"] = 1684763355
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("OK", stdout);
            Assert.Contains("cli-test-123", stdout);
            Assert.Contains("Message ID:", stdout);
            Assert.Contains("Balance after:", stdout);
        }

        [Fact]
        public void Send_TestMode_PrintsWarning()
        {
            var (exit, stdout, _) = RunCli(new[] { "send", "96598765432", "Hello test" }, req =>
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["result"] = "OK",
                    ["msg-id"] = "warn-123",
                    ["numbers"] = 1,
                    ["points-charged"] = 1,
                    ["balance-after"] = 149.0,
                    ["unix-timestamp"] = 1684763355
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("WARNING", stdout);
            Assert.Contains("Test mode", stdout);
        }

        [Fact]
        public void Send_MissingArgs_ShowsUsage()
        {
            var (exit, _, stderr) = RunCli(new[] { "send" });
            Assert.Equal(1, exit);
            Assert.Contains("Usage:", stderr);
        }

        [Fact]
        public void Send_Error_ShowsActionGuidance()
        {
            var (exit, _, stderr) = RunCli(new[] { "send", "96598765432", "Test" }, req =>
            {
                var json = JsonSerializer.Serialize(new { result = "ERROR", code = "ERR010", description = "Zero balance" });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(1, exit);
            Assert.Contains("ERR010", stderr);
            Assert.Contains("Action:", stderr);
        }

        [Fact]
        public void Send_WithSenderFlag()
        {
            string? capturedBody = null;
            var (exit, stdout, _) = RunCli(new[] { "send", "96598765432", "Hello", "--sender", "MY APP" }, req =>
            {
                capturedBody = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["result"] = "OK",
                    ["msg-id"] = "sender-123",
                    ["numbers"] = 1,
                    ["points-charged"] = 1,
                    ["balance-after"] = 148.0,
                    ["unix-timestamp"] = 1684763355
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("OK", stdout);
            Assert.NotNull(capturedBody);
            Assert.Contains("MY APP", capturedBody!);
        }

        // ── Validate ──

        [Fact]
        public void Validate_Success_ShowsCategories()
        {
            var (exit, stdout, _) = RunCli(new[] { "validate", "96598765432", "966551234567" }, req =>
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["result"] = "OK",
                    ["mobile"] = new Dictionary<string, object>
                    {
                        ["OK"] = new[] { "96598765432" },
                        ["ER"] = Array.Empty<string>(),
                        ["NR"] = new[] { "966551234567" }
                    }
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("OK (1)", stdout);
            Assert.Contains("96598765432", stdout);
            Assert.Contains("No route (1)", stdout);
        }

        [Fact]
        public void Validate_MissingArgs_ShowsUsage()
        {
            var (exit, _, stderr) = RunCli(new[] { "validate" });
            Assert.Equal(1, exit);
            Assert.Contains("Usage:", stderr);
        }

        // ── SenderID ──

        [Fact]
        public void SenderId_Success_ListsIds()
        {
            var (exit, stdout, _) = RunCli(new[] { "senderid" }, req =>
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["result"] = "OK",
                    ["senderid"] = new[] { "KWT-SMS", "MYAPP" }
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("Sender IDs (2)", stdout);
            Assert.Contains("KWT-SMS", stdout);
            Assert.Contains("MYAPP", stdout);
        }

        // ── Coverage ──

        [Fact]
        public void Coverage_Success_ListsPrefixes()
        {
            var (exit, stdout, _) = RunCli(new[] { "coverage" }, req =>
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["result"] = "OK",
                    ["prefixes"] = new[] { "965", "966", "971" }
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("Active prefixes (3)", stdout);
            Assert.Contains("965", stdout);
        }

        // ── Status ──

        [Fact]
        public void Status_Success_ShowsStatus()
        {
            var (exit, stdout, _) = RunCli(new[] { "status", "msg-abc123" }, req =>
            {
                var json = JsonSerializer.Serialize(new { result = "OK", status = "sent", description = "Message successfully sent" });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("Status:", stdout);
            Assert.Contains("sent", stdout);
        }

        [Fact]
        public void Status_MissingArgs_ShowsUsage()
        {
            var (exit, _, stderr) = RunCli(new[] { "status" });
            Assert.Equal(1, exit);
            Assert.Contains("Usage:", stderr);
        }

        // ── DLR ──

        [Fact]
        public void Dlr_Success_ShowsReports()
        {
            var (exit, stdout, _) = RunCli(new[] { "dlr", "msg-abc123" }, req =>
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["result"] = "OK",
                    ["report"] = new[] { new Dictionary<string, object> { ["Number"] = "96598765432", ["Status"] = "Received by recipient" } }
                });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });

            Assert.Equal(0, exit);
            Assert.Contains("Delivery reports (1)", stdout);
            Assert.Contains("96598765432", stdout);
            Assert.Contains("Received by recipient", stdout);
        }

        [Fact]
        public void Dlr_MissingArgs_ShowsUsage()
        {
            var (exit, _, stderr) = RunCli(new[] { "dlr" });
            Assert.Equal(1, exit);
            Assert.Contains("Usage:", stderr);
        }
    }
}
