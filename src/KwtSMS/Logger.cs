using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace KwtSMS
{
    /// <summary>
    /// JSONL logger for API calls. One JSON line per call. Never crashes the main flow.
    /// </summary>
    internal static class Logger
    {
        private static readonly object _writeLock = new object();

        /// <summary>
        /// Write a JSONL log entry. Masks password in the request payload.
        /// Thread-safe. Never throws exceptions.
        /// </summary>
        internal static void WriteLog(
            string logFile,
            string endpoint,
            Dictionary<string, object?>? request,
            Dictionary<string, object?>? response,
            bool ok,
            string? error)
        {
            if (string.IsNullOrEmpty(logFile)) return;

            try
            {
                var maskedRequest = MaskPassword(request);

                var entry = new Dictionary<string, object?>
                {
                    ["ts"] = DateTime.UtcNow.ToString("o"),
                    ["endpoint"] = endpoint,
                    ["request"] = maskedRequest,
                    ["response"] = response,
                    ["ok"] = ok,
                    ["error"] = error
                };

                var json = JsonSerializer.Serialize(entry);
                lock (_writeLock)
                {
                    File.AppendAllText(logFile, json + "\n");
                }
            }
            catch
            {
                // Logging must never crash the main flow
            }
        }

        private static Dictionary<string, object?>? MaskPassword(Dictionary<string, object?>? data)
        {
            if (data == null) return null;

            var masked = new Dictionary<string, object?>(data);
            if (masked.ContainsKey("password"))
            {
                masked["password"] = "***";
            }
            return masked;
        }
    }
}
