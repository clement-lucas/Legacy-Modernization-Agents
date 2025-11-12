using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Insurance.PolicyExpiryBatch
{
    /// <summary>
    /// Main batch processor for insurance policy expiry notifications.
    /// Processes policy records, generates customer and agent notifications, and produces summary reports.
    /// </summary>
    public class PolicyExpiryBatchProcessor
    {
        private readonly IDbDriver1 _dbDriver1;
        private readonly IDbDriver2 _dbDriver2;
        private readonly IFileDriver1 _fileDriver1;
        private readonly IFileDriver2 _fileDriver2;
        private readonly ILogger<PolicyExpiryBatchProcessor> _logger;

        // Working storage variables
        private PolicyRecord? _currentPolicy;
        private AgentRecord? _currentAgent;
        private CustomerNotifyRecord _customerNotifyRecord = new();
        private AgentNotifyRecord _agentNotifyRecord = new();

        private string _currentState = string.Empty;
        private string _currentAgentCode = string.Empty;

        private int _agentTotalPolicyCount = 0;
        private decimal _agentTotalPremium = 0m;
        private int _stateTotalPolicyCount = 0;
        private decimal _stateTotalPremium = 0m;
        private int _grandTotalPolicyCount = 0;
        private decimal _grandTotalPremium = 0m;

        private DateTime _processDate;

        /// <summary>
        /// Constructs the batch processor with injected dependencies.
        /// </summary>
        public PolicyExpiryBatchProcessor(
            IDbDriver1 dbDriver1,
            IDbDriver2 dbDriver2,
            IFileDriver1 fileDriver1,
            IFileDriver2 fileDriver2,
            ILogger<PolicyExpiryBatchProcessor> logger)
        {
            _dbDriver1 = dbDriver1;
            _dbDriver2 = dbDriver2;
            _fileDriver1 = fileDriver1;
            _fileDriver2 = fileDriver2;
            _logger = logger;
        }

        /// <summary>
        /// Entry point for batch processing.
        /// </summary>
        public async Task RunAsync()
        {
            try
            {
                await InitializeAsync();
                await ProcessAsync();
                await FinalizeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch processing failed.");
                throw;
            }
        }

        /// <summary>
        /// Initializes drivers, opens files, and prepares report headers.
        /// </summary>
        private async Task InitializeAsync()
        {
            _processDate = DateTime.Now;

            // Open DB cursor for policies
            var openPolicyResult = await _dbDriver1.OpenCursorAsync(_processDate);
            if (!openPolicyResult.IsSuccess)
            {
                _logger.LogError("Error opening policy cursor: {SqlCode}", openPolicyResult.SqlCode);
                throw new ApplicationException($"Error opening policy cursor: {openPolicyResult.SqlCode}");
            }

            // Open agent file
            var openAgentResult = await _fileDriver1.OpenAsync();
            if (!openAgentResult.IsSuccess)
            {
                _logger.LogError("Error opening agent file: {StatusCode}", openAgentResult.StatusCode);
                throw new ApplicationException($"Error opening agent file: {openAgentResult.StatusCode}");
            }

            // Open notification files
            foreach (var fileName in new[] { "CUSTOMER-NOTIFY-FILE", "NOTIFY-REPORT-FILE", "AGENT-NOTIFY-FILE" })
            {
                var openNotifyResult = await _fileDriver2.OpenAsync(fileName);
                if (!openNotifyResult.IsSuccess)
                {
                    _logger.LogError("Error opening notify file {FileName}: {StatusCode}", fileName, openNotifyResult.StatusCode);
                    throw new ApplicationException($"Error opening notify file {fileName}: {openNotifyResult.StatusCode}");
                }
            }

            await WriteReportHeaderAsync();
        }

        /// <summary>
        /// Main processing loop: fetches policies, processes notifications, updates tracking, and accumulates summaries.
        /// </summary>
        private async Task ProcessAsync()
        {
            bool noMorePolicy = false;

            while (!noMorePolicy)
            {
                var fetchResult = await _dbDriver1.FetchNextPolicyAsync();
                switch (fetchResult.Status)
                {
                    case DbFetchStatus.EndOfData:
                        noMorePolicy = true;
                        break;
                    case DbFetchStatus.Success:
                        _currentPolicy = fetchResult.Policy;
                        await GetAgentDetailAsync();
                        await WriteCustomerNotificationAsync();
                        await UpdateTrackingAsync();
                        await ProcessSummaryAsync();
                        break;
                    case DbFetchStatus.Error:
                        _logger.LogError("Error fetching policy record: {SqlCode}", fetchResult.SqlCode);
                        throw new ApplicationException($"Error fetching policy record: {fetchResult.SqlCode}");
                }
            }
        }

        /// <summary>
        /// Finalizes processing by closing all files and drivers.
        /// </summary>
        private async Task FinalizeAsync()
        {
            var closePolicyResult = await _dbDriver1.CloseCursorAsync();
            if (!closePolicyResult.IsSuccess)
            {
                _logger.LogError("Error closing policy cursor: {SqlCode}", closePolicyResult.SqlCode);
                throw new ApplicationException($"Error closing policy cursor: {closePolicyResult.SqlCode}");
            }

            var closeAgentResult = await _fileDriver1.CloseAsync();
            if (!closeAgentResult.IsSuccess)
            {
                _logger.LogError("Error closing agent file: {StatusCode}", closeAgentResult.StatusCode);
                throw new ApplicationException($"Error closing agent file: {closeAgentResult.StatusCode}");
            }

            foreach (var fileName in new[] { "CUSTOMER-NOTIFY-FILE", "NOTIFY-REPORT-FILE", "AGENT-NOTIFY-FILE" })
            {
                var closeNotifyResult = await _fileDriver2.CloseAsync(fileName);
                if (!closeNotifyResult.IsSuccess)
                {
                    _logger.LogError("Error closing notify file {FileName}: {StatusCode}", fileName, closeNotifyResult.StatusCode);
                    throw new ApplicationException($"Error closing notify file {fileName}: {closeNotifyResult.StatusCode}");
                }
            }
        }

        /// <summary>
        /// Fetches agent details for the current policy.
        /// </summary>
        private async Task GetAgentDetailAsync()
        {
            if (_currentPolicy == null)
                throw new InvalidOperationException("Current policy is null.");

            var agentResult = await _fileDriver1.SearchAgentAsync(_currentPolicy.AgentCode);
            if (!agentResult.IsSuccess)
            {
                _logger.LogError("Error fetching agent record: {StatusCode}", agentResult.StatusCode);
                throw new ApplicationException($"Error fetching agent record: {agentResult.StatusCode}");
            }

            _currentAgent = agentResult.Agent;
            await WriteAgentNotificationAsync();
        }

        /// <summary>
        /// Writes customer notification record to file.
        /// </summary>
        private async Task WriteCustomerNotificationAsync()
        {
            PopulateCustomerDetail();

            var writeResult = await _fileDriver2.WriteCustomerNotifyAsync("CUSTOMER-NOTIFY-FILE", _customerNotifyRecord);
            if (!writeResult.IsSuccess)
            {
                _logger.LogWarning("Error writing to customer notify file: {StatusCode}", writeResult.StatusCode);
            }
        }

        /// <summary>
        /// Writes agent notification record to file if agent is corporate.
        /// </summary>
        private async Task WriteAgentNotificationAsync()
        {
            if (_currentAgent == null || _currentPolicy == null)
                throw new InvalidOperationException("Current agent or policy is null.");

            PopulateAgentDetail();

            if (_currentAgent.AgentType == "CORPORATE")
            {
                var writeResult = await _fileDriver2.WriteAgentNotifyAsync("AGENT-NOTIFY-FILE", _agentNotifyRecord);
                if (!writeResult.IsSuccess)
                {
                    _logger.LogWarning("Error writing to agent notify file: {StatusCode}", writeResult.StatusCode);
                }
            }
        }

        /// <summary>
        /// Inserts tracking record for processed policy.
        /// </summary>
        private async Task UpdateTrackingAsync()
        {
            if (_currentPolicy == null)
                throw new InvalidOperationException("Current policy is null.");

            var insertResult = await _dbDriver2.InsertTrackingAsync(_processDate, _currentPolicy.PolicyNumber);
            if (!insertResult.IsSuccess)
            {
                _logger.LogError("Error inserting into tracking table: {SqlCode}", insertResult.SqlCode);
                throw new ApplicationException($"Error inserting into tracking table: {insertResult.SqlCode}");
            }
        }

        /// <summary>
        /// Handles summary calculations and report writing for state/agent breaks.
        /// </summary>
        private async Task ProcessSummaryAsync()
        {
            if (_currentPolicy == null || _currentAgent == null)
                throw new InvalidOperationException("Current policy or agent is null.");

            // State break
            if (_currentPolicy.HolderState != _currentState)
            {
                if (_grandTotalPolicyCount != 0)
                {
                    await WriteAgentSummaryAsync();
                    await WriteBreakLineAsync();
                    await WriteStateSummaryAsync();
                }
                ResetAgentTotals();
                ResetStateTotals();
                _currentState = _currentPolicy.HolderState;
                await WriteBreakLineAsync();
                await WriteStateHeaderAsync();
                await WriteBreakLineAsync();
                _currentAgentCode = _currentAgent.AgentCode;
                await WriteAgentHeaderAsync();
                await WriteBreakLineAsync();
                await WritePolicyHeaderAsync();
            }
            // Agent break
            else if (_currentAgent.AgentCode != _currentAgentCode)
            {
                await WriteAgentSummaryAsync();
                ResetAgentTotals();
                _currentAgentCode = _currentAgent.AgentCode;
                await WriteBreakLineAsync();
                await WriteAgentHeaderAsync();
                await WriteBreakLineAsync();
                await WritePolicyHeaderAsync();
            }

            await WritePolicyDetailLineAsync();

            _agentTotalPolicyCount++;
            _stateTotalPolicyCount++;
            _grandTotalPolicyCount++;

            _agentTotalPremium += _currentPolicy.PremiumAmount;
            _stateTotalPremium += _currentPolicy.PremiumAmount;
            _grandTotalPremium += _currentPolicy.PremiumAmount;
        }

        /// <summary>
        /// Resets agent-level counters.
        /// </summary>
        private void ResetAgentTotals()
        {
            _agentTotalPolicyCount = 0;
            _agentTotalPremium = 0m;
        }

        /// <summary>
        /// Resets state-level counters.
        /// </summary>
        private void ResetStateTotals()
        {
            _stateTotalPolicyCount = 0;
            _stateTotalPremium = 0m;
        }

        /// <summary>
        /// Populates customer notification record fields from policy and agent data.
        /// </summary>
        private void PopulateCustomerDetail()
        {
            if (_currentPolicy == null || _currentAgent == null)
                throw new InvalidOperationException("Current policy or agent is null.");

            _customerNotifyRecord = new CustomerNotifyRecord
            {
                PolicyNumber = _currentPolicy.PolicyNumber,
                FirstName = _currentPolicy.HolderFirstName,
                MiddleName = _currentPolicy.HolderMiddleName,
                LastName = _currentPolicy.HolderLastName,
                Address1 = _currentPolicy.HolderAddress1,
                Address2 = _currentPolicy.HolderAddress2,
                City = _currentPolicy.HolderCity,
                State = _currentPolicy.HolderState,
                ZipCode = _currentPolicy.HolderZipCode,
                StartDate = _currentPolicy.StartDate,
                ExpiryDate = _currentPolicy.ExpiryDate,
                NotifyDate = _processDate.ToString("MM/dd/yyyy"),
                BeneficiaryName = _currentPolicy.BeneficiaryName,
                NotifyMessage = "PLEASE NOTE YOUR POLICY IS EXPIRING SOON. GET IT RENEWED TO CONTINUE COVERAGE",
                AgentCode = _currentPolicy.AgentCode,
                AgentName = _currentAgent.AgentName,
                Email = _currentPolicy.HolderEmail,
                StatutoryMessage = "IF YOU FAIL TO RENEW BY EXPIRY DATE YOUR INSURANCE COVERAGE WILL END"
            };
        }

        /// <summary>
        /// Populates agent notification record fields from agent and policy data.
        /// </summary>
        private void PopulateAgentDetail()
        {
            if (_currentPolicy == null || _currentAgent == null)
                throw new InvalidOperationException("Current policy or agent is null.");

            _agentNotifyRecord = new AgentNotifyRecord
            {
                AgentCode = _currentAgent.AgentCode,
                AgentName = _currentAgent.AgentName,
                Address1 = _currentAgent.Address1,
                Address2 = _currentAgent.Address2,
                City = _currentAgent.City,
                State = _currentAgent.State,
                ZipCode = _currentAgent.ZipCode,
                Email = _currentAgent.Email,
                PolicyNumber = _currentPolicy.PolicyNumber,
                PolicyHolderFirstName = _currentPolicy.HolderFirstName,
                PolicyHolderMiddleName = _currentPolicy.HolderMiddleName,
                PolicyHolderLastName = _currentPolicy.HolderLastName,
                PolicyStartDate = _currentPolicy.StartDate,
                PolicyExpiryDate = _currentPolicy.ExpiryDate,
                NotifyDate = _processDate.ToString("MM/dd/yyyy"),
                NotifyMessage = "PLEASE NOTE CUSTOMER POLICY IS EXPIRING SOON"
            };
        }

        /// <summary>
        /// Writes the main report header.
        /// </summary>
        private async Task WriteReportHeaderAsync()
        {
            await WriteBreakLineAsync();

            var header = ReportLineBuilder.BuildMainHeader(_processDate);
            await WriteNotificationReportAsync(header);

            await WriteBreakLineAsync();
        }

        /// <summary>
        /// Writes a blank line to report for separation.
        /// </summary>
        private async Task WriteBreakLineAsync()
        {
            var blankLine = new string(' ', 133);
            await WriteNotificationReportAsync(blankLine);
        }

        /// <summary>
        /// Writes state header line to report.
        /// </summary>
        private async Task WriteStateHeaderAsync()
        {
            var header = ReportLineBuilder.BuildStateHeader(_currentState);
            await WriteNotificationReportAsync(header);
        }

        /// <summary>
        /// Writes agent header lines to report.
        /// </summary>
        private async Task WriteAgentHeaderAsync()
        {
            if (_currentAgent == null)
                throw new InvalidOperationException("Current agent is null.");

            var lines = ReportLineBuilder.BuildAgentHeader(_currentAgent);
            foreach (var line in lines)
            {
                await WriteNotificationReportAsync(line);
            }
        }

        /// <summary>
        /// Writes policy header lines to report.
        /// </summary>
        private async Task WritePolicyHeaderAsync()
        {
            var lines = ReportLineBuilder.BuildPolicyHeader();
            foreach (var line in lines)
            {
                await WriteNotificationReportAsync(line);
            }
        }

        /// <summary>
        /// Writes policy detail line to report.
        /// </summary>
        private async Task WritePolicyDetailLineAsync()
        {
            if (_currentPolicy == null)
                throw new InvalidOperationException("Current policy is null.");

            var line = ReportLineBuilder.BuildPolicyDetailLine(_currentPolicy);
            await WriteNotificationReportAsync(line);
        }

        /// <summary>
        /// Writes agent summary line to report.
        /// </summary>
        private async Task WriteAgentSummaryAsync()
        {
            if (_currentAgent == null)
                throw new InvalidOperationException("Current agent is null.");

            var line = ReportLineBuilder.BuildAgentSummaryLine(
                _currentAgent.AgentCode,
                _agentTotalPolicyCount,
                _agentTotalPremium);

            await WriteNotificationReportAsync(line);
        }

        /// <summary>
        /// Writes state summary line to report.
        /// </summary>
        private async Task WriteStateSummaryAsync()
        {
            var line = ReportLineBuilder.BuildStateSummaryLine(
                _currentState,
                _stateTotalPolicyCount,
                _stateTotalPremium);

            await WriteNotificationReportAsync(line);
        }

        /// <summary>
        /// Writes grand summary line to report.
        /// </summary>
        private async Task WriteGrandSummaryAsync()
        {
            var line = ReportLineBuilder.BuildGrandSummaryLine(
                _grandTotalPolicyCount,
                _grandTotalPremium);

            await WriteNotificationReportAsync(line);
        }

        /// <summary>
        /// Writes a notification report record to file.
        /// </summary>
        private async Task WriteNotificationReportAsync(string reportLine)
        {
            var writeResult = await _fileDriver2.WriteReportAsync("NOTIFY-REPORT-FILE", reportLine);
            if (!writeResult.IsSuccess)
            {
                _logger.LogWarning("Error writing to notify report file: {StatusCode}", writeResult.StatusCode);
            }
        }
    }

    #region Data Models

    /// <summary>
    /// Represents a policy record.
    /// </summary>
    public record PolicyRecord
    {
        public string PolicyNumber { get; init; } = string.Empty;
        public string HolderFirstName { get; init; } = string.Empty;
        public string HolderMiddleName { get; init; } = string.Empty;
        public string HolderLastName { get; init; } = string.Empty;
        public string HolderAddress1 { get; init; } = string.Empty;
        public string HolderAddress2 { get; init; } = string.Empty;
        public string HolderCity { get; init; } = string.Empty;
        public string HolderState { get; init; } = string.Empty;
        public string HolderZipCode { get; init; } = string.Empty;
        public string HolderEmail { get; init; } = string.Empty;
        public string BeneficiaryName { get; init; } = string.Empty;
        public string AgentCode { get; init; } = string.Empty;
        public string StartDate { get; init; } = string.Empty;
        public string ExpiryDate { get; init; } = string.Empty;
        public decimal PremiumAmount { get; init; }
    }

    /// <summary>
    /// Represents an agent record.
    /// </summary>
    public record AgentRecord
    {
        public string AgentCode { get; init; } = string.Empty;
        public string AgentName { get; init; } = string.Empty;
        public string Address1 { get; init; } = string.Empty;
        public string Address2 { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string ContactNumber { get; init; } = string.Empty;
        public string AgentType { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents a customer notification record.
    /// </summary>
    public record CustomerNotifyRecord
    {
        public string PolicyNumber { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string MiddleName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Address1 { get; init; } = string.Empty;
        public string Address2 { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;
        public string StartDate { get; init; } = string.Empty;
        public string ExpiryDate { get; init; } = string.Empty;
        public string NotifyDate { get; init; } = string.Empty;
        public string NotifyMessage { get; init; } = string.Empty;
        public string AgentCode { get; init; } = string.Empty;
        public string AgentName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string BeneficiaryName { get; init; } = string.Empty;
        public string StatutoryMessage { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents an agent notification record.
    /// </summary>
    public record AgentNotifyRecord
    {
        public string AgentCode { get; init; } = string.Empty;
        public string AgentName { get; init; } = string.Empty;
        public string Address1 { get; init; } = string.Empty;
        public string Address2 { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PolicyNumber { get; init; } = string.Empty;
        public string PolicyHolderFirstName { get; init; } = string.Empty;
        public string PolicyHolderMiddleName { get; init; } = string.Empty;
        public string PolicyHolderLastName { get; init; } = string.Empty;
        public string PolicyStartDate { get; init; } = string.Empty;
        public string PolicyExpiryDate { get; init; } = string.Empty;
        public string NotifyDate { get; init; } = string.Empty;
        public string NotifyMessage { get; init; } = string.Empty;
    }

    #endregion

    #region Driver Interfaces

    /// <summary>
    /// Interface for DB driver 1 (policy cursor operations).
    /// </summary>
    public interface IDbDriver1
    {
        Task<DbOperationResult> OpenCursorAsync(DateTime processDate);
        Task<DbFetchResult> FetchNextPolicyAsync();
        Task<DbOperationResult> CloseCursorAsync();
    }

    /// <summary>
    /// Interface for DB driver 2 (tracking insert operations).
    /// </summary>
    public interface IDbDriver2
    {
        Task<DbOperationResult> InsertTrackingAsync(DateTime processDate, string policyNumber);
    }

    /// <summary>
    /// Interface for file driver 1 (agent file operations).
    /// </summary>
    public interface IFileDriver1
    {
        Task<FileOperationResult> OpenAsync();
        Task<AgentSearchResult> SearchAgentAsync(string agentCode);
        Task<FileOperationResult> CloseAsync();
    }

    /// <summary>
    /// Interface for file driver 2 (notification/report file operations).
    /// </summary>
    public interface IFileDriver2
    {
        Task<FileOperationResult> OpenAsync(string fileName);
        Task<FileOperationResult> WriteCustomerNotifyAsync(string fileName, CustomerNotifyRecord record);
        Task<FileOperationResult> WriteAgentNotifyAsync(string fileName, AgentNotifyRecord record);
        Task<FileOperationResult> WriteReportAsync(string fileName, string reportLine);
        Task<FileOperationResult> CloseAsync(string fileName);
    }

    #endregion

    #region Driver Results

    /// <summary>
    /// Represents the result of a DB operation.
    /// </summary>
    public record DbOperationResult(bool IsSuccess, int SqlCode);

    /// <summary>
    /// Represents the result of a DB fetch operation.
    /// </summary>
    public record DbFetchResult(DbFetchStatus Status, PolicyRecord? Policy, int SqlCode);

    /// <summary>
    /// Status of DB fetch operation.
    /// </summary>
    public enum DbFetchStatus
    {
        Success,
        EndOfData,
        Error
    }

    /// <summary>
    /// Represents the result of a file operation.
    /// </summary>
    public record FileOperationResult(bool IsSuccess, string StatusCode);

    /// <summary>
    /// Represents the result of an agent search.
    /// </summary>
    public record AgentSearchResult(bool IsSuccess, AgentRecord? Agent, string StatusCode);

    #endregion

    #region Report Line Builder

    /// <summary>
    /// Helper class for building report lines.
    /// </summary>
    public static class ReportLineBuilder
    {
        /// <summary>
        /// Builds the main report header line.
        /// </summary>
        public static string BuildMainHeader(DateTime processDate)
        {
            var dateStr = processDate.ToString("MM/dd/yyyy");
            return $"{new string(' ', 30)}30 DAYS POLICY EXPIRY REPORT AS OF {dateStr}{new string(' ', 57)}";
        }

        /// <summary>
        /// Builds the state header line.
        /// </summary>
        public static string BuildStateHeader(string stateCode)
        {
            return $"{new string(' ', 3)}FOR THE STATE OF {stateCode}{new string(' ', 92)}";
        }

        /// <summary>
        /// Builds agent header lines.
        /// </summary>
        public static IEnumerable<string> BuildAgentHeader(AgentRecord agent)
        {
            yield return $"{new string(' ', 3)}AGENT: {agent.AgentCode} - {agent.AgentName}{new string(' ', 65)}";
            yield return $"{new string(' ', 10)}{agent.Address1}{new string(' ', 73)}";
            yield return $"{new string(' ', 10)}{agent.Address2}{new string(' ', 73)}";
            yield return $"{new string(' ', 10)}{agent.City}{new string(' ', 2)}{agent.State}{new string(' ', 2)}{agent.ZipCode}{new string(' ', 73)}";
            yield return $"{new string(' ', 10)}{agent.ContactNumber}{new string(' ', 2)}{agent.Email}{new string(' ', 81)}";
        }

        /// <summary>
        /// Builds policy header lines.
        /// </summary>
        public static IEnumerable<string> BuildPolicyHeader()
        {
            yield return $"{new string(' ', 10)}POLICY NO {new string(' ', 2)}HOLDER NAME{new string(' ', 2)}START DATE{new string(' ', 2)}EXPIRY DATE{new string(' ', 2)}PREMIUM{new string(' ', 1)}";
            yield return $"{new string(' ', 10)}POLICY NO {new string(' ', 2)}-----------{new string(' ', 2)}----------{new string(' ', 2)}-----------{new string(' ', 2)}-------{new string(' ', 1)}";
        }

        /// <summary>
        /// Builds a policy detail line.
        /// </summary>
        public static string BuildPolicyDetailLine(PolicyRecord policy)
        {
            var holderName = $"{policy.HolderFirstName} {policy.HolderMiddleName} {policy.HolderLastName}".Trim();
            return $"{new string(' ', 10)}{policy.PolicyNumber,-10}{new string(' ', 2)}{holderName,-73}{new string(' ', 2)}{policy.StartDate,-10}{new string(' ', 2)}{policy.ExpiryDate,-10}{new string(' ', 3)}{policy.PremiumAmount,10:C}{new string(' ', 1)}";
        }

        /// <summary>
        /// Builds agent summary line.
        /// </summary>
        public static string BuildAgentSummaryLine(string agentCode, int policyCount, decimal totalPremium)
        {
            return $"{new string(' ', 3)}AGENT: {agentCode}{new string(' ', 2)}POLICY COUNT: {policyCount,5}{new string(' ', 2)}POLICY PREMIUM: {totalPremium,9:C}{new string(' ', 1)}";
        }

        /// <summary>
        /// Builds state summary line.
        /// </summary>
        public static string BuildStateSummaryLine(string stateCode, int policyCount, decimal totalPremium)
        {
            return $"{new string(' ', 3)}STATE: {stateCode}{new string(' ', 2)}POLICY COUNT: {policyCount,6}{new string(' ', 2)}POLICY PREMIUM: {totalPremium,9:C}{new string(' ', 69)}";
        }

        /// <summary>
        /// Builds grand summary line.
        /// </summary>
        public static string BuildGrandSummaryLine(int policyCount, decimal totalPremium)
        {
            return $"{new string(' ', 3)}GRAND SUMMARY: {new string(' ', 2)}POLICY COUNT: {policyCount,6}{new string(' ', 2)}POLICY PREMIUM: {totalPremium,9:C}{new string(' ', 69)}";
        }
    }

    #endregion
}