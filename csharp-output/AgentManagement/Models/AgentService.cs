using System;
using System.Threading.Tasks;

namespace AgentManagement.Models
{
    /// <summary>
    /// Represents an agent record containing personal and contact information.
    /// </summary>
    public sealed record AgentRecord
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
        /// Gets the city where the agent resides.
        /// </summary>
        public string AgentCity { get; init; } = string.Empty;

        /// <summary>
        /// Gets the state code of the agent's address.
        /// </summary>
        public string AgentState { get; init; } = string.Empty;

        /// <summary>
        /// Gets the ZIP code of the agent's address.
        /// </summary>
        public string AgentZipCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets the agent's date of birth in YYYY-MM-DD format.
        /// </summary>
        public string AgentDateOfBirth { get; init; } = string.Empty;

        /// <summary>
        /// Gets the type of the agent (e.g., internal, external).
        /// </summary>
        public string AgentType { get; init; } = string.Empty;

        /// <summary>
        /// Gets the status of the agent (e.g., active/inactive).
        /// </summary>
        public string AgentStatus { get; init; } = string.Empty;

        /// <summary>
        /// Gets the email address of the agent.
        /// </summary>
        public string AgentEmail { get; init; } = string.Empty;

        /// <summary>
        /// Gets the contact number of the agent.
        /// </summary>
        public string AgentContactNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets the start date of the agent's employment in YYYY-MM-DD format.
        /// </summary>
        public string AgentStartDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the end date of the agent's employment in YYYY-MM-DD format.
        /// </summary>
        public string AgentEndDate { get; init; } = string.Empty;
    }
}

namespace AgentManagement.Services
{
    using AgentManagement.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides operations for managing agent records.
    /// </summary>
    public interface IAgentService
    {
        /// <summary>
        /// Validates the specified agent record asynchronously.
        /// </summary>
        /// <param name="agent">The agent record to validate.</param>
        /// <returns>True if the agent record is valid; otherwise, false.</returns>
        Task<bool> ValidateAgentAsync(AgentRecord agent);

        /// <summary>
        /// Saves the specified agent record asynchronously.
        /// </summary>
        /// <param name="agent">The agent record to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveAgentAsync(AgentRecord agent);
    }

    /// <summary>
    /// Implements agent record management operations.
    /// </summary>
    public class AgentService : IAgentService
    {
        private readonly ILogger<AgentService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for error and event logging.</param>
        public AgentService(ILogger<AgentService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> ValidateAgentAsync(AgentRecord agent)
        {
            if (agent is null)
                throw new ArgumentNullException(nameof(agent));

            try
            {
                // Example validation: Ensure required fields are not empty
                bool isValid = !string.IsNullOrWhiteSpace(agent.AgentCode)
                    && !string.IsNullOrWhiteSpace(agent.AgentName)
                    && !string.IsNullOrWhiteSpace(agent.AgentEmail);

                // Additional validation logic can be added here (e.g., regex for email, date parsing)
                await Task.CompletedTask; // Simulate async operation

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating agent record.");
                throw; // Rethrow for higher-level handling
            }
        }

        /// <inheritdoc />
        public async Task SaveAgentAsync(AgentRecord agent)
        {
            if (agent is null)
                throw new ArgumentNullException(nameof(agent));

            try
            {
                // Simulate saving to a database or external system
                await Task.Delay(100); // Simulate async I/O

                _logger.LogInformation("Agent record saved: {AgentCode}", agent.AgentCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving agent record.");
                throw; // Rethrow for higher-level handling
            }
        }
    }
}