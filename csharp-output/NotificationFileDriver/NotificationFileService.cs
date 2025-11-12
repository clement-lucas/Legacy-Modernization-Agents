using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NotificationFileDriver
{
    /// <summary>
    /// Represents the type of file operation to perform.
    /// </summary>
    public enum FileOperationType
    {
        Open,
        Close,
        Write
    }

    /// <summary>
    /// Represents the result of a file operation.
    /// </summary>
    public record FileOperationResult(string StatusCode, string? ErrorMessage = null);

    /// <summary>
    /// Represents an agent notification record.
    /// </summary>
    public record AgentNotifyRecord(
        string AgentCode,
        string AgentName,
        string AgentAddress1,
        string AgentAddress2,
        string AgentCity,
        string AgentState,
        string AgentPolicyNumber,
        string AgentPolicyFName,
        string AgentPolicyMName,
        string AgentPolicyLName,
        string AgentPolicyStartDate,
        string AgentPolicyExpiryDate,
        string AgentNotifyDate,
        string AgentNotifyMessages
    );

    /// <summary>
    /// Represents a customer notification record.
    /// </summary>
    public record CustomerNotifyRecord(
        string CustPolicyNumber,
        string CustFName,
        string CustMName,
        string CustLName,
        string CustPolicyStartDate,
        string CustPolicyExpiryDate,
        string CustNotifyDate,
        string CustNotifyMessages,
        string CustAgentCode,
        string CustAgentName,
        string CustStatutoryMessage
    );

    /// <summary>
    /// Represents a notification report record.
    /// </summary>
    public record NotifyReportRecord(
        string ReportLine
    );

    /// <summary>
    /// Provides file operations for notification files.
    /// </summary>
    public interface INotificationFileService
    {
        Task<FileOperationResult> OpenFileAsync(string fileName);
        Task<FileOperationResult> CloseFileAsync(string fileName);
        Task<FileOperationResult> WriteAgentRecordAsync(string fileName, AgentNotifyRecord record);
        Task<FileOperationResult> WriteCustomerRecordAsync(string fileName, CustomerNotifyRecord record);
        Task<FileOperationResult> WriteReportRecordAsync(string fileName, NotifyReportRecord record);
    }

    /// <summary>
    /// Implements file operations for notification files.
    /// </summary>
    public class NotificationFileService : INotificationFileService
    {
        private readonly ILogger<NotificationFileService> _logger;

        // File streams are kept open between Open/Close calls.
        private FileStream? _agentFileStream;
        private FileStream? _customerFileStream;
        private FileStream? _reportFileStream;

        private const string AgentFilePhysicalName = "AGENTFLE";
        private const string CustomerFilePhysicalName = "CUSTFLE";
        private const string ReportFilePhysicalName = "RPTFLE";

        public NotificationFileService(ILogger<NotificationFileService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<FileOperationResult> OpenFileAsync(string fileName)
        {
            try
            {
                switch (fileName)
                {
                    case "AGENT-NOTIFY-FILE":
                        _agentFileStream = new FileStream(AgentFilePhysicalName, FileMode.Create, FileAccess.Write, FileShare.None);
                        break;
                    case "CUSTOMER-NOTIFY-FILE":
                        _customerFileStream = new FileStream(CustomerFilePhysicalName, FileMode.Create, FileAccess.Write, FileShare.None);
                        break;
                    case "NOTIFY-REPORT-FILE":
                        _reportFileStream = new FileStream(ReportFilePhysicalName, FileMode.Create, FileAccess.Write, FileShare.None);
                        break;
                    default:
                        return new FileOperationResult("99", $"Unknown file name: {fileName}");
                }
                return new FileOperationResult("00");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening file {FileName}", fileName);
                return new FileOperationResult("99", ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<FileOperationResult> CloseFileAsync(string fileName)
        {
            try
            {
                switch (fileName)
                {
                    case "AGENT-NOTIFY-FILE":
                        _agentFileStream?.Dispose();
                        _agentFileStream = null;
                        break;
                    case "CUSTOMER-NOTIFY-FILE":
                        _customerFileStream?.Dispose();
                        _customerFileStream = null;
                        break;
                    case "NOTIFY-REPORT-FILE":
                        _reportFileStream?.Dispose();
                        _reportFileStream = null;
                        break;
                    default:
                        return new FileOperationResult("99", $"Unknown file name: {fileName}");
                }
                return new FileOperationResult("00");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing file {FileName}", fileName);
                return new FileOperationResult("99", ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<FileOperationResult> WriteAgentRecordAsync(string fileName, AgentNotifyRecord record)
        {
            try
            {
                if (_agentFileStream == null)
                    return new FileOperationResult("99", "Agent file not open.");

                var line = FormatAgentRecord(record);
                var bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
                await _agentFileStream.WriteAsync(bytes, 0, bytes.Length);
                await _agentFileStream.FlushAsync();
                return new FileOperationResult("00");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing agent record to file {FileName}", fileName);
                return new FileOperationResult("99", ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<FileOperationResult> WriteCustomerRecordAsync(string fileName, CustomerNotifyRecord record)
        {
            try
            {
                if (_customerFileStream == null)
                    return new FileOperationResult("99", "Customer file not open.");

                var line = FormatCustomerRecord(record);
                var bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
                await _customerFileStream.WriteAsync(bytes, 0, bytes.Length);
                await _customerFileStream.FlushAsync();
                return new FileOperationResult("00");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing customer record to file {FileName}", fileName);
                return new FileOperationResult("99", ex.Message);
            }
        }

        /// <inheritdoc />
        public async Task<FileOperationResult> WriteReportRecordAsync(string fileName, NotifyReportRecord record)
        {
            try
            {
                if (_reportFileStream == null)
                    return new FileOperationResult("99", "Report file not open.");

                var line = record.ReportLine;
                var bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
                await _reportFileStream.WriteAsync(bytes, 0, bytes.Length);
                await _reportFileStream.FlushAsync();
                return new FileOperationResult("00");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing report record to file {FileName}", fileName);
                return new FileOperationResult("99", ex.Message);
            }
        }

        /// <summary>
        /// Formats an agent notification record as a fixed-width string.
        /// </summary>
        private static string FormatAgentRecord(AgentNotifyRecord record)
        {
            // COBOL fields are fixed-width; pad/truncate as needed.
            return string.Concat(
                record.AgentCode.PadRight(10).Substring(0, 10),
                record.AgentName.PadRight(45).Substring(0, 45),
                record.AgentAddress1.PadRight(50).Substring(0, 50),
                record.AgentAddress2.PadRight(50).Substring(0, 50),
                record.AgentCity.PadRight(20).Substring(0, 20),
                record.AgentState.PadRight(2).Substring(0, 2),
                record.AgentPolicyNumber.PadRight(10).Substring(0, 10),
                record.AgentPolicyFName.PadRight(35).Substring(0, 35),
                record.AgentPolicyMName.PadRight(1).Substring(0, 1),
                record.AgentPolicyLName.PadRight(35).Substring(0, 35),
                record.AgentPolicyStartDate.PadRight(10).Substring(0, 10),
                record.AgentPolicyExpiryDate.PadRight(10).Substring(0, 10),
                record.AgentNotifyDate.PadRight(10).Substring(0, 10),
                record.AgentNotifyMessages.PadRight(100).Substring(0, 100)
            );
        }

        /// <summary>
        /// Formats a customer notification record as a fixed-width string.
        /// </summary>
        private static string FormatCustomerRecord(CustomerNotifyRecord record)
        {
            return string.Concat(
                record.CustPolicyNumber.PadRight(10).Substring(0, 10),
                record.CustFName.PadRight(35).Substring(0, 35),
                record.CustMName.PadRight(1).Substring(0, 1),
                record.CustLName.PadRight(35).Substring(0, 35),
                record.CustPolicyStartDate.PadRight(10).Substring(0, 10),
                record.CustPolicyExpiryDate.PadRight(10).Substring(0, 10),
                record.CustNotifyDate.PadRight(10).Substring(0, 10),
                record.CustNotifyMessages.PadRight(100).Substring(0, 100),
                record.CustAgentCode.PadRight(10).Substring(0, 10),
                record.CustAgentName.PadRight(45).Substring(0, 45),
                record.CustStatutoryMessage.PadRight(100).Substring(0, 100)
            );
        }
    }

    /// <summary>
    /// Encapsulates the main driver logic for notification file operations.
    /// </summary>
    public class NotificationFileDriver
    {
        private readonly INotificationFileService _fileService;
        private readonly ILogger<NotificationFileDriver> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationFileDriver"/> class.
        /// </summary>
        /// <param name="fileService">The notification file service.</param>
        /// <param name="logger">The logger.</param>
        public NotificationFileDriver(INotificationFileService fileService, ILogger<NotificationFileDriver> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Executes the requested file operation.
        /// </summary>
        /// <param name="fileName">Logical file name (e.g., "AGENT-NOTIFY-FILE").</param>
        /// <param name="operationType">Operation type ("OPEN", "CLOSE", "WRITE").</param>
        /// <param name="agentRecord">Agent record (for WRITE).</param>
        /// <param name="customerRecord">Customer record (for WRITE).</param>
        /// <param name="reportRecord">Report record (for WRITE).</param>
        /// <returns>File operation result.</returns>
        public async Task<FileOperationResult> ExecuteAsync(
            string fileName,
            string operationType,
            AgentNotifyRecord? agentRecord = null,
            CustomerNotifyRecord? customerRecord = null,
            NotifyReportRecord? reportRecord = null)
        {
            FileOperationType? opType = operationType.ToUpperInvariant() switch
            {
                "OPEN" => FileOperationType.Open,
                "CLOSE" => FileOperationType.Close,
                "WRITE" => FileOperationType.Write,
                _ => null
            };

            if (opType == null)
            {
                _logger.LogError("Invalid operation type: {OperationType}", operationType);
                return new FileOperationResult("99", $"Invalid operation type: {operationType}");
            }

            try
            {
                switch (opType)
                {
                    case FileOperationType.Open:
                        return await _fileService.OpenFileAsync(fileName);

                    case FileOperationType.Close:
                        return await _fileService.CloseFileAsync(fileName);

                    case FileOperationType.Write:
                        return fileName switch
                        {
                            "AGENT-NOTIFY-FILE" when agentRecord != null =>
                                await _fileService.WriteAgentRecordAsync(fileName, agentRecord),
                            "CUSTOMER-NOTIFY-FILE" when customerRecord != null =>
                                await _fileService.WriteCustomerRecordAsync(fileName, customerRecord),
                            "NOTIFY-REPORT-FILE" when reportRecord != null =>
                                await _fileService.WriteReportRecordAsync(fileName, reportRecord),
                            _ =>
                                new FileOperationResult("99", $"Missing or invalid record for file: {fileName}")
                        };

                    default:
                        return new FileOperationResult("99", "Unknown operation.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing operation {OperationType} on file {FileName}", operationType, fileName);
                return new FileOperationResult("99", ex.Message);
            }
        }
    }
}