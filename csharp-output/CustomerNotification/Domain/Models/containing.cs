using System;

namespace CustomerNotification.Domain.Models
{
    /// <summary>
    /// Represents a customer notification record containing customer, policy, agent, and notification details.
    /// </summary>
    /// <remarks>
    /// This model is converted from a COBOL data structure (WS-CUSTOMER-NOTIFY-RECORD).
    /// All fields are strings to match COBOL PIC X(n) definitions.
    /// </remarks>
    public sealed record CustomerNotificationRecord
    {
        /// <summary>
        /// Gets the customer policy number.
        /// </summary>
        public string PolicyNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets the customer's first name.
        /// </summary>
        public string FirstName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the customer's middle initial.
        /// </summary>
        public string MiddleName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the customer's last name.
        /// </summary>
        public string LastName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the policy start date (format: yyyy-MM-dd or as provided).
        /// </summary>
        public string StartDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the policy expiry date (format: yyyy-MM-dd or as provided).
        /// </summary>
        public string ExpiryDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the notification date (format: yyyy-MM-dd or as provided).
        /// </summary>
        public string NotifyDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the notification messages.
        /// </summary>
        public string NotifyMessages { get; init; } = string.Empty;

        /// <summary>
        /// Gets the agent code.
        /// </summary>
        public string AgentCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets the agent name.
        /// </summary>
        public string AgentName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the statutory message.
        /// </summary>
        public string StatutoryMessage { get; init; } = string.Empty;
    }
}