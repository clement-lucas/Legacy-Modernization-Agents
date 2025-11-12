using System;

namespace Insurance.Notifications.Models
{
    /// <summary>
    /// Represents a notification record for an insurance agent, including agent details,
    /// policyholder information, policy dates, and notification messages.
    /// </summary>
    public record AgentNotifyRecord
    {
        /// <summary>
        /// Gets the unique code identifying the agent.
        /// </summary>
        public string AgentCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets the full name of the agent.
        /// </summary>
        public string AgentName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the first line of the agent's address.
        /// </summary>
        public string AgentAddress1 { get; init; } = string.Empty;

        /// <summary>
        /// Gets the second line of the agent's address.
        /// </summary>
        public string AgentAddress2 { get; init; } = string.Empty;

        /// <summary>
        /// Gets the city where the agent is located.
        /// </summary>
        public string AgentCity { get; init; } = string.Empty;

        /// <summary>
        /// Gets the state abbreviation for the agent's location.
        /// </summary>
        public string AgentState { get; init; } = string.Empty;

        /// <summary>
        /// Gets the policy number associated with the notification.
        /// </summary>
        public string PolicyNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets the first name of the policyholder.
        /// </summary>
        public string PolicyHolderFirstName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the middle initial of the policyholder.
        /// </summary>
        public string PolicyHolderMiddleInitial { get; init; } = string.Empty;

        /// <summary>
        /// Gets the last name of the policyholder.
        /// </summary>
        public string PolicyHolderLastName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the start date of the policy (format: yyyy-MM-dd).
        /// </summary>
        public string PolicyStartDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the expiry date of the policy (format: yyyy-MM-dd).
        /// </summary>
        public string PolicyExpiryDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the date when the notification was sent (format: yyyy-MM-dd).
        /// </summary>
        public string NotifyDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the notification messages associated with the agent and policy.
        /// </summary>
        public string NotifyMessages { get; init; } = string.Empty;
    }
}