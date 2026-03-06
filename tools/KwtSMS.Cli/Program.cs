using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KwtSMS;

namespace KwtSMS.Cli
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                PrintUsage();
                return 0;
            }

            if (args[0] == "--version" || args[0] == "-v")
            {
                Console.WriteLine("kwtsms 0.3.0");
                return 0;
            }

            var command = args[0].ToLowerInvariant();

            if (command == "setup")
                return RunSetup();

            KwtSmsClient sms;
            try
            {
                sms = KwtSmsClient.FromEnv();
            }
            catch
            {
                var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                if (!File.Exists(envPath))
                {
                    Console.WriteLine("No .env file found. Starting first-time setup...");
                    Console.WriteLine();
                    var setupResult = RunSetup();
                    if (setupResult != 0) return setupResult;
                    try
                    {
                        sms = KwtSmsClient.FromEnv();
                    }
                    catch (Exception ex2)
                    {
                        Console.Error.WriteLine($"Error: {ex2.Message}");
                        return 1;
                    }
                }
                else
                {
                    Console.Error.WriteLine("Error: credentials missing or incomplete in .env");
                    Console.Error.WriteLine("Run 'kwtsms setup' to fix.");
                    return 1;
                }
            }

            try
            {
                switch (command)
                {
                    case "verify":
                        return RunVerify(sms);
                    case "balance":
                        return RunBalance(sms);
                    case "send":
                        return RunSend(sms, args);
                    case "validate":
                        return RunValidate(sms, args);
                    case "senderid":
                        return RunSenderId(sms);
                    case "coverage":
                        return RunCoverage(sms);
                    case "status":
                        return RunStatus(sms, args);
                    case "dlr":
                        return RunDlr(sms, args);
                    default:
                        Console.Error.WriteLine($"Unknown command: {command}");
                        Console.Error.WriteLine("Run 'kwtsms --help' for usage.");
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine(@"kwtsms - CLI for the kwtSMS SMS gateway (kwtsms.com)

USAGE:
    kwtsms <command> [arguments] [options]

COMMANDS:
    setup                               Create .env file interactively
    verify                              Test credentials and show balance
    balance                             Show available and purchased credits
    send <mobile> <message> [--sender ID]  Send SMS to one or more numbers
    validate <number> [number ...]      Validate phone numbers
    senderid                            List registered sender IDs
    coverage                            List active country prefixes
    status <msg-id>                     Check message queue status
    dlr <msg-id>                        Get delivery report (international only)

OPTIONS:
    -h, --help       Show this help
    -v, --version    Show version

SETUP:
    Set credentials via environment variables or a .env file:

        KWTSMS_USERNAME=csharp_username
        KWTSMS_PASSWORD=csharp_password
        KWTSMS_SENDER_ID=KWT-SMS        # optional, default: KWT-SMS
        KWTSMS_TEST_MODE=1              # optional, 1=test 0=live
        KWTSMS_LOG_FILE=kwtsms.log      # optional, empty to disable

EXAMPLES:
    kwtsms setup
    kwtsms verify
    kwtsms balance
    kwtsms send 96598765432 ""Your OTP is: 123456""
    kwtsms send ""96598765432,96512345678"" ""Hello!"" --sender MY-APP
    kwtsms validate 96598765432 +96512345678
    kwtsms senderid
    kwtsms coverage
    kwtsms status f4c841adee210f31307633ceaebff2ec
    kwtsms dlr f4c841adee210f31307633ceaebff2ec");
        }

        static int RunSetup()
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

            // Load existing .env values as defaults
            var existing = new Dictionary<string, string>();
            if (File.Exists(envPath))
            {
                try
                {
                    foreach (var rawLine in File.ReadAllLines(envPath))
                    {
                        var line = rawLine.Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                        var eq = line.IndexOf('=');
                        if (eq < 0) continue;
                        var key = line.Substring(0, eq).Trim();
                        var val = line.Substring(eq + 1).Trim();
                        if (!string.IsNullOrEmpty(key)) existing[key] = val;
                    }
                }
                catch { /* ignore parse errors */ }
            }

            Console.WriteLine();
            Console.WriteLine("── kwtSMS Setup ──────────────────────────────────────────────────");
            Console.WriteLine("Verifies your API credentials and creates a .env file.");
            Console.WriteLine("Press Enter to keep the value shown in brackets.");
            Console.WriteLine();

            // Username
            var defaultUser = existing.GetValueOrDefault("KWTSMS_USERNAME", "");
            var userPrompt = !string.IsNullOrEmpty(defaultUser)
                ? $"API Username [{defaultUser}]: "
                : "API Username: ";
            Console.Write(userPrompt);
            var username = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(username)) username = defaultUser;

            // Password
            var defaultPass = existing.GetValueOrDefault("KWTSMS_PASSWORD", "");
            string password;
            if (!string.IsNullOrEmpty(defaultPass))
            {
                Console.Write("API Password [keep existing]: ");
                var rawPass = Console.ReadLine()?.Trim() ?? "";
                password = !string.IsNullOrEmpty(rawPass) ? rawPass : defaultPass;
            }
            else
            {
                Console.Write("API Password: ");
                password = Console.ReadLine()?.Trim() ?? "";
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine();
                Console.WriteLine("Error: username and password are required.");
                return 1;
            }

            return RunSetupContinue(envPath, existing, username, password);
        }

        static int RunSetupContinue(string envPath, Dictionary<string, string> existing, string username, string password)
        {
            // Verify credentials
            Console.Write("\nVerifying credentials... ");
            try
            {
                var tempClient = new KwtSmsClient(username, password, logFile: "");
                var (ok, balance, error) = tempClient.Verify();
                if (ok)
                {
                    Console.WriteLine($"OK  (Balance: {balance})");
                }
                else
                {
                    Console.WriteLine($"FAILED");
                    Console.Error.WriteLine($"Error: {error}");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED");
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }

            // Fetch Sender IDs
            Console.Write("Fetching Sender IDs... ");
            var senderIds = new List<string>();
            try
            {
                var tempClient = new KwtSmsClient(username, password, logFile: "");
                var sidResult = tempClient.SenderIds();
                if (sidResult.Result == "OK")
                    senderIds = sidResult.SenderIds;
            }
            catch { /* ignore */ }

            string senderId;
            if (senderIds.Count > 0)
            {
                Console.WriteLine("OK");
                Console.WriteLine();
                Console.WriteLine("Available Sender IDs:");
                for (int i = 0; i < senderIds.Count; i++)
                    Console.WriteLine($"  {i + 1}. {senderIds[i]}");

                var defaultSid = existing.GetValueOrDefault("KWTSMS_SENDER_ID", senderIds[0]);
                Console.Write($"\nSelect Sender ID (number or name) [{defaultSid}]: ");
                var choice = Console.ReadLine()?.Trim() ?? "";
                if (int.TryParse(choice, out var idx) && idx >= 1 && idx <= senderIds.Count)
                    senderId = senderIds[idx - 1];
                else if (!string.IsNullOrEmpty(choice))
                    senderId = choice;
                else
                    senderId = defaultSid;
            }
            else
            {
                Console.WriteLine("(none returned)");
                var defaultSid = existing.GetValueOrDefault("KWTSMS_SENDER_ID", "KWT-SMS");
                Console.Write($"Sender ID [{defaultSid}]: ");
                var sidInput = Console.ReadLine()?.Trim() ?? "";
                senderId = !string.IsNullOrEmpty(sidInput) ? sidInput : defaultSid;
            }

            // Send mode
            var currentMode = existing.GetValueOrDefault("KWTSMS_TEST_MODE", "1");
            Console.WriteLine();
            Console.WriteLine("Send mode:");
            Console.WriteLine("  1. Test mode: messages queued but NOT delivered, no credits consumed  [default]");
            Console.WriteLine("  2. Live mode: messages delivered to handsets, credits consumed");
            var modeDefault = currentMode != "0" ? "1" : "2";
            Console.Write($"\nChoose [{modeDefault}]: ");
            var modeChoice = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(modeChoice)) modeChoice = modeDefault;
            var testMode = modeChoice == "2" ? "0" : "1";

            if (testMode == "1")
                Console.WriteLine("  → Test mode selected.");
            else
                Console.WriteLine("  → Live mode selected. Real messages will be sent and credits consumed.");

            // Log file
            var defaultLog = existing.GetValueOrDefault("KWTSMS_LOG_FILE", "kwtsms.log");
            Console.WriteLine();
            Console.WriteLine("API logging (every API call is logged to a file, passwords are always masked):");
            if (!string.IsNullOrEmpty(defaultLog))
                Console.WriteLine($"  Current: {defaultLog}");
            Console.WriteLine("  Type \"off\" to disable logging.");
            Console.Write($"  Log file path [{(string.IsNullOrEmpty(defaultLog) ? "off" : defaultLog)}]: ");
            var logInput = Console.ReadLine()?.Trim() ?? "";
            string logFile;
            if (logInput.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                logFile = "";
                Console.WriteLine("  → Logging disabled.");
            }
            else if (!string.IsNullOrEmpty(logInput))
                logFile = logInput;
            else
                logFile = defaultLog;

            // Sanitize: strip newlines to prevent values from breaking .env format
            username = username.Replace("\r", "").Replace("\n", "");
            password = password.Replace("\r", "").Replace("\n", "");
            senderId = senderId.Replace("\r", "").Replace("\n", "");
            logFile = logFile.Replace("\r", "").Replace("\n", "");

            // Write .env
            var content = "# kwtSMS credentials, generated by kwtsms setup\n"
                + $"KWTSMS_USERNAME={username}\n"
                + $"KWTSMS_PASSWORD={password}\n"
                + $"KWTSMS_SENDER_ID={senderId}\n"
                + $"KWTSMS_TEST_MODE={testMode}\n"
                + $"KWTSMS_LOG_FILE={logFile}\n";

            try
            {
                File.WriteAllText(envPath, content);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"\nError writing {envPath}: {ex.Message}");
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine($"  Saved to {envPath}");
            if (testMode == "1")
                Console.WriteLine("  Mode: TEST: messages queued but not delivered (no credits consumed)");
            else
                Console.WriteLine("  Mode: LIVE: messages will be delivered and credits consumed");
            Console.WriteLine("  Run 'kwtsms setup' at any time to change settings.");
            Console.WriteLine("─────────────────────────────────────────────────────────────────");
            Console.WriteLine();

            return 0;
        }

        static int RunVerify(KwtSmsClient sms)
        {
            var (ok, balance, error) = sms.Verify();
            if (ok)
            {
                Console.WriteLine("OK");
                Console.WriteLine($"Balance:   {balance}");
                Console.WriteLine($"Purchased: {sms.CachedPurchased}");
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: {error}");
                return 1;
            }
        }

        static int RunBalance(KwtSmsClient sms)
        {
            // Verify first to get both available and purchased
            var (ok, balance, error) = sms.Verify();
            if (ok)
            {
                Console.WriteLine($"Available: {balance}");
                Console.WriteLine($"Purchased: {sms.CachedPurchased}");
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: {error}");
                return 1;
            }
        }

        static int RunSend(KwtSmsClient sms, string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: kwtsms send <mobile> <message> [--sender ID]");
                return 1;
            }

            // Test mode warning
            if (sms.TestMode)
            {
                Console.WriteLine("WARNING: Test mode is ON. Message will be queued but NOT delivered to handset.");
                Console.WriteLine();
            }

            var mobile = args[1];
            var message = args[2];
            string? sender = null;

            for (int i = 3; i < args.Length; i++)
            {
                if ((args[i] == "--sender" || args[i] == "-s") && i + 1 < args.Length)
                {
                    sender = args[++i];
                }
            }

            var result = sms.Send(mobile, message, sender: sender);

            if (result.Result == "OK")
            {
                Console.WriteLine("OK");
                Console.WriteLine($"Message ID:     {result.MsgId}");
                Console.WriteLine($"Numbers:        {result.Numbers}");
                Console.WriteLine($"Points charged: {result.PointsCharged}");
                Console.WriteLine($"Balance after:  {result.BalanceAfter}");
                if (result.Invalid != null && result.Invalid.Count > 0)
                {
                    Console.WriteLine($"Rejected:       {result.Invalid.Count} number(s)");
                    foreach (var inv in result.Invalid)
                        Console.WriteLine($"  {inv.Input}: {inv.Error}");
                }
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: {result.Code}");
                Console.Error.WriteLine($"Description: {result.Description}");
                if (result.Action != null)
                    Console.Error.WriteLine($"Action: {result.Action}");
                if (result.Invalid != null && result.Invalid.Count > 0)
                {
                    Console.Error.WriteLine($"Rejected: {result.Invalid.Count} number(s)");
                    foreach (var inv in result.Invalid)
                        Console.Error.WriteLine($"  {inv.Input}: {inv.Error}");
                }
                return 1;
            }
        }

        static int RunValidate(KwtSmsClient sms, string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: kwtsms validate <number> [number ...]");
                return 1;
            }

            var numbers = args.Skip(1).ToArray();
            var result = sms.Validate(numbers);

            if (result.Error != null)
            {
                Console.Error.WriteLine($"ERROR: {result.Error}");
                return 1;
            }

            if (result.Ok.Count > 0)
            {
                Console.WriteLine($"OK ({result.Ok.Count}):");
                foreach (var n in result.Ok)
                    Console.WriteLine($"  {n}");
            }
            if (result.Er.Count > 0)
            {
                Console.WriteLine($"Format errors ({result.Er.Count}):");
                foreach (var n in result.Er)
                    Console.WriteLine($"  {n}");
            }
            if (result.Nr.Count > 0)
            {
                Console.WriteLine($"No route ({result.Nr.Count}):");
                foreach (var n in result.Nr)
                    Console.WriteLine($"  {n}");
            }
            if (result.Rejected.Count > 0)
            {
                Console.WriteLine($"Rejected locally ({result.Rejected.Count}):");
                foreach (var r in result.Rejected)
                    Console.WriteLine($"  {r.Input}: {r.Error}");
            }

            return 0;
        }

        static int RunSenderId(KwtSmsClient sms)
        {
            var result = sms.SenderIds();
            if (result.Result == "OK")
            {
                Console.WriteLine($"Sender IDs ({result.SenderIds.Count}):");
                foreach (var id in result.SenderIds)
                    Console.WriteLine($"  {id}");
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: {result.Code}");
                Console.Error.WriteLine($"Description: {result.Description}");
                if (result.Action != null)
                    Console.Error.WriteLine($"Action: {result.Action}");
                return 1;
            }
        }

        static int RunCoverage(KwtSmsClient sms)
        {
            var result = sms.Coverage();
            if (result.Result == "OK")
            {
                Console.WriteLine($"Active prefixes ({result.Prefixes.Count}):");
                foreach (var prefix in result.Prefixes)
                    Console.WriteLine($"  {prefix}");
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: {result.Code}");
                Console.Error.WriteLine($"Description: {result.Description}");
                if (result.Action != null)
                    Console.Error.WriteLine($"Action: {result.Action}");
                return 1;
            }
        }

        static int RunStatus(KwtSmsClient sms, string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: kwtsms status <msg-id>");
                return 1;
            }

            var result = sms.Status(args[1]);
            if (result.Result == "OK")
            {
                Console.WriteLine($"Status:      {result.Status}");
                Console.WriteLine($"Description: {result.Description}");
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: {result.Code}");
                Console.Error.WriteLine($"Description: {result.Description}");
                if (result.Action != null)
                    Console.Error.WriteLine($"Action: {result.Action}");
                return 1;
            }
        }

        static int RunDlr(KwtSmsClient sms, string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: kwtsms dlr <msg-id>");
                return 1;
            }

            var result = sms.Dlr(args[1]);
            if (result.Result == "OK")
            {
                Console.WriteLine($"Delivery reports ({result.Report.Count}):");
                foreach (var entry in result.Report)
                    Console.WriteLine($"  {entry.Number}: {entry.Status}");
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"ERROR: {result.Code}");
                Console.Error.WriteLine($"Description: {result.Description}");
                if (result.Action != null)
                    Console.Error.WriteLine($"Action: {result.Action}");
                return 1;
            }
        }
    }
}
