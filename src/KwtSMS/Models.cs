using System;
using System.Collections.Generic;

namespace KwtSMS
{
    /// <summary>
    /// Result of a phone number validation check.
    /// </summary>
    public class PhoneValidationResult
    {
        /// <summary>Whether the phone number is valid.</summary>
        public bool IsValid { get; set; }

        /// <summary>Error message if invalid, null if valid.</summary>
        public string? Error { get; set; }

        /// <summary>Normalized phone number (digits only, no leading zeros).</summary>
        public string Normalized { get; set; } = "";
    }

    /// <summary>
    /// A phone number that failed local pre-validation.
    /// </summary>
    public class InvalidEntry
    {
        /// <summary>Original input value.</summary>
        public string Input { get; set; } = "";

        /// <summary>Reason the number was rejected.</summary>
        public string Error { get; set; } = "";
    }

    /// <summary>
    /// Result of a single send operation (up to 200 numbers).
    /// </summary>
    public class SendResult
    {
        /// <summary>"OK" or "ERROR".</summary>
        public string Result { get; set; } = "";

        /// <summary>Message ID for status checks and delivery reports.</summary>
        public string? MsgId { get; set; }

        /// <summary>Count of numbers accepted and dispatched.</summary>
        public int? Numbers { get; set; }

        /// <summary>SMS credits deducted.</summary>
        public int? PointsCharged { get; set; }

        /// <summary>Balance immediately after deduction.</summary>
        public double? BalanceAfter { get; set; }

        /// <summary>Server timestamp (GMT+3, not UTC).</summary>
        public long? UnixTimestamp { get; set; }

        /// <summary>Error code (e.g., "ERR003").</summary>
        public string? Code { get; set; }

        /// <summary>Error description from the API.</summary>
        public string? Description { get; set; }

        /// <summary>Developer-friendly action guidance.</summary>
        public string? Action { get; set; }

        /// <summary>Numbers that failed local validation.</summary>
        public List<InvalidEntry>? Invalid { get; set; }
    }

    /// <summary>
    /// Result of a validate operation.
    /// </summary>
    public class ValidateResult
    {
        /// <summary>Valid and routable numbers.</summary>
        public List<string> Ok { get; set; } = new List<string>();

        /// <summary>Numbers with format errors.</summary>
        public List<string> Er { get; set; } = new List<string>();

        /// <summary>Numbers with no route (country not activated).</summary>
        public List<string> Nr { get; set; } = new List<string>();

        /// <summary>Numbers that failed local pre-validation.</summary>
        public List<InvalidEntry> Rejected { get; set; } = new List<InvalidEntry>();

        /// <summary>Error message if the API call failed.</summary>
        public string? Error { get; set; }

        /// <summary>Raw API response.</summary>
        public Dictionary<string, object?>? Raw { get; set; }
    }

    /// <summary>
    /// Result of a sender IDs query.
    /// </summary>
    public class SenderIdResult
    {
        /// <summary>"OK" or "ERROR".</summary>
        public string Result { get; set; } = "";

        /// <summary>List of registered sender IDs.</summary>
        public List<string> SenderIds { get; set; } = new List<string>();

        /// <summary>Error code if failed.</summary>
        public string? Code { get; set; }

        /// <summary>Error description if failed.</summary>
        public string? Description { get; set; }

        /// <summary>Developer-friendly action guidance.</summary>
        public string? Action { get; set; }
    }

    /// <summary>
    /// Result of a coverage query.
    /// </summary>
    public class CoverageResult
    {
        /// <summary>"OK" or "ERROR".</summary>
        public string Result { get; set; } = "";

        /// <summary>List of active country prefixes.</summary>
        public List<string> Prefixes { get; set; } = new List<string>();

        /// <summary>Error code if failed.</summary>
        public string? Code { get; set; }

        /// <summary>Error description if failed.</summary>
        public string? Description { get; set; }

        /// <summary>Developer-friendly action guidance.</summary>
        public string? Action { get; set; }
    }

    /// <summary>
    /// Result of a message status query.
    /// </summary>
    public class StatusResult
    {
        /// <summary>"OK" or "ERROR".</summary>
        public string Result { get; set; } = "";

        /// <summary>Message status (e.g., "sent").</summary>
        public string? Status { get; set; }

        /// <summary>Status description.</summary>
        public string? Description { get; set; }

        /// <summary>Error code if failed.</summary>
        public string? Code { get; set; }

        /// <summary>Developer-friendly action guidance.</summary>
        public string? Action { get; set; }
    }

    /// <summary>
    /// A single delivery report entry.
    /// </summary>
    public class DlrEntry
    {
        /// <summary>Phone number.</summary>
        public string Number { get; set; } = "";

        /// <summary>Delivery status.</summary>
        public string Status { get; set; } = "";
    }

    /// <summary>
    /// Result of a delivery report query.
    /// </summary>
    public class DlrResult
    {
        /// <summary>"OK" or "ERROR".</summary>
        public string Result { get; set; } = "";

        /// <summary>Delivery report entries.</summary>
        public List<DlrEntry> Report { get; set; } = new List<DlrEntry>();

        /// <summary>Error code if failed.</summary>
        public string? Code { get; set; }

        /// <summary>Error description if failed.</summary>
        public string? Description { get; set; }

        /// <summary>Developer-friendly action guidance.</summary>
        public string? Action { get; set; }
    }
}
