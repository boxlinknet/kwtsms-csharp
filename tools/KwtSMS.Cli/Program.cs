using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KwtSMS;

namespace KwtSMS.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                PrintUsage();
                return 0;
            }

            if (args[0] == "--version" || args[0] == "-v")
            {
                Console.WriteLine("kwtsms 0.2.0");
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
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading credentials: {ex.Message}");
                Console.Error.WriteLine("Set KWTSMS_USERNAME and KWTSMS_PASSWORD environment variables or create a .env file.");
                Console.Error.WriteLine("Run 'kwtsms setup' to create a .env file interactively.");
                return 1;
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
            if (File.Exists(envPath))
            {
                Console.Write(".env file already exists. Overwrite? [y/N] ");
                var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (answer != "y" && answer != "yes")
                {
                    Console.WriteLine("Aborted.");
                    return 0;
                }
            }

            Console.WriteLine("kwtSMS Setup");
            Console.WriteLine("============");
            Console.WriteLine("Enter your API credentials from kwtsms.com -> Account -> API.");
            Console.WriteLine();

            Console.Write("API Username: ");
            var username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("API Password: ");
            var password = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Sender ID [KWT-SMS]: ");
            var senderId = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(senderId)) senderId = "KWT-SMS";

            Console.Write("Test mode? (messages queued but not delivered) [Y/n]: ");
            var testInput = Console.ReadLine()?.Trim().ToLowerInvariant();
            var testMode = testInput != "n" && testInput != "no" ? "1" : "0";

            var content = $"KWTSMS_USERNAME={username}\nKWTSMS_PASSWORD={password}\nKWTSMS_SENDER_ID={senderId}\nKWTSMS_TEST_MODE={testMode}\nKWTSMS_LOG_FILE=kwtsms.log\n";
            File.WriteAllText(envPath, content);

            Console.WriteLine();
            Console.WriteLine($"Saved to {envPath}");
            Console.WriteLine("Run 'kwtsms verify' to test your credentials.");

            if (testMode == "1")
                Console.WriteLine("NOTE: Test mode is ON. Messages will be queued but NOT delivered.");

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
