using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFileDriver
{
    /// <summary>
    /// Represents an agent record as defined in the COBOL CAGENT copybook.
    /// </summary>
    public record AgentRecord(
        string AgentCode,
        string AgentName,
        string AgentAddress1,
        string AgentAddress2,
        string AgentCity,
        string AgentState,
        string AgentZipCode,
        string AgentStatus,
        string AgentType,
        string AgentEmail,
        string AgentContactNo,
        string AgentStartDate,
        string AgentEndDate
    );

    /// <summary>
    /// Represents the input parameters for agent file operations.
    /// </summary>
    public class AgentFileInput
    {
        /// <summary>
        /// The operation type (OPEN, CLOSE, SEARCH).
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// The agent code to search for.
        /// </summary>
        public string AgentCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the output/result of agent file operations.
    /// </summary>
    public class AgentFileOutput
    {
        /// <summary>
        /// Status code of the operation ("00" = OK, "23" = Not Found, "99" = Invalid Operation, etc.).
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// The returned agent record, if found.
        /// </summary>
        public AgentRecord? AgentRecord { get; set; }
    }

    /// <summary>
    /// Interface for agent file repository abstraction.
    /// </summary>
    public interface IAgentFileRepository
    {
        /// <summary>
        /// Opens the agent file for input operations.
        /// </summary>
        Task OpenAsync();

        /// <summary>
        /// Closes the agent file.
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// Searches for an agent record by agent code.
        /// </summary>
        /// <param name="agentCode">The agent code to search for.</param>
        /// <returns>The agent record if found; otherwise, null.</returns>
        Task<AgentRecord?> SearchAsync(string agentCode);
    }

    /// <summary>
    /// Exception thrown when agent file operations fail.
    /// </summary>
    public class AgentFileException : Exception
    {
        public string StatusCode { get; }

        public AgentFileException(string message, string statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Implements the agent file repository using a file-based storage.
    /// Replace with actual VSAM or database access in production.
    /// </summary>
    public class FileAgentFileRepository : IAgentFileRepository
    {
        private readonly string filePath;
        private FileStream? fileStream;

        /// <summary>
        /// Initializes a new instance of <see cref="FileAgentFileRepository"/>.
        /// </summary>
        /// <param name="filePath">Path to the agent file.</param>
        public FileAgentFileRepository(string filePath)
        {
            this.filePath = filePath;
        }

        /// <inheritdoc/>
        public async Task OpenAsync()
        {
            // Simulate file open (replace with VSAM or DB open logic)
            if (!File.Exists(filePath))
                throw new AgentFileException("Agent file not found.", "23");

            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task CloseAsync()
        {
            fileStream?.Dispose();
            fileStream = null;
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<AgentRecord?> SearchAsync(string agentCode)
        {
            // Simulate indexed file search (replace with VSAM or DB logic)
            // For demonstration, assume CSV file with fields matching AgentRecord order
            if (fileStream == null)
                throw new AgentFileException("Agent file is not open.", "99");

            using var reader = new StreamReader(fileStream, leaveOpen: true);
            fileStream.Seek(0, SeekOrigin.Begin);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var fields = line.Split(',');
                if (fields.Length < 13)
                    continue;

                if (fields[0].Trim().Equals(agentCode, StringComparison.OrdinalIgnoreCase))
                {
                    return new AgentRecord(
                        AgentCode: fields[0].Trim(),
                        AgentName: fields[1].Trim(),
                        AgentAddress1: fields[2].Trim(),
                        AgentAddress2: fields[3].Trim(),
                        AgentCity: fields[4].Trim(),
                        AgentState: fields[5].Trim(),
                        AgentZipCode: fields[6].Trim(),
                        AgentStatus: fields[7].Trim(),
                        AgentType: fields[8].Trim(),
                        AgentEmail: fields[9].Trim(),
                        AgentContactNo: fields[10].Trim(),
                        AgentStartDate: fields[11].Trim(),
                        AgentEndDate: fields[12].Trim()
                    );
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Main driver class for agent file operations, converted from COBOL FLDRIVR1.
    /// </summary>
    public class AgentFileDriver
    {
        private readonly IAgentFileRepository agentFileRepository;
        private readonly ILogger<AgentFileDriver> logger;

        /// <summary>
        /// Initializes a new instance of <see cref="AgentFileDriver"/>.
        /// </summary>
        /// <param name="agentFileRepository">The agent file repository.</param>
        /// <param name="logger">The logger instance.</param>
        public AgentFileDriver(IAgentFileRepository agentFileRepository, ILogger<AgentFileDriver> logger)
        {
            this.agentFileRepository = agentFileRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Executes the requested agent file operation.
        /// </summary>
        /// <param name="input">Input parameters for the operation.</param>
        /// <returns>Output/result of the operation.</returns>
        /// <exception cref="AgentFileException">Thrown when an error occurs during file operations.</exception>
        public async Task<AgentFileOutput> ExecuteAsync(AgentFileInput input)
        {
            var output = new AgentFileOutput();

            try
            {
                switch (input.OperationType?.Trim().ToUpperInvariant())
                {
                    case "OPEN":
                        await OpenAgentFileAsync(output);
                        break;
                    case "CLOSE":
                        await CloseAgentFileAsync(output);
                        break;
                    case "SEARCH":
                        await SearchAgentFileAsync(input.AgentCode, output);
                        break;
                    default:
                        output.StatusCode = "99";
                        logger.LogWarning("Invalid operation type: {OperationType}", input.OperationType);
                        break;
                }
            }
            catch (AgentFileException ex)
            {
                await HandleErrorAsync(input.OperationType, ex.StatusCode, ex.Message, output);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(input.OperationType, "99", ex.Message, output);
            }

            return output;
        }

        /// <summary>
        /// Opens the agent file.
        /// </summary>
        private async Task OpenAgentFileAsync(AgentFileOutput output)
        {
            try
            {
                await agentFileRepository.OpenAsync();
                output.StatusCode = "00";
            }
            catch (AgentFileException ex)
            {
                output.StatusCode = ex.StatusCode;
                throw;
            }
        }

        /// <summary>
        /// Closes the agent file.
        /// </summary>
        private async Task CloseAgentFileAsync(AgentFileOutput output)
        {
            try
            {
                await agentFileRepository.CloseAsync();
                output.StatusCode = "00";
            }
            catch (AgentFileException ex)
            {
                output.StatusCode = ex.StatusCode;
                throw;
            }
        }

        /// <summary>
        /// Searches for an agent record by agent code.
        /// </summary>
        /// <param name="agentCode">The agent code to search for.</param>
        /// <param name="output">The output object to populate.</param>
        private async Task SearchAgentFileAsync(string agentCode, AgentFileOutput output)
        {
            try
            {
                var record = await agentFileRepository.SearchAsync(agentCode);
                if (record is null)
                {
                    output.StatusCode = "23"; // Not found
                    throw new AgentFileException($"Agent record not found for code: {agentCode}", "23");
                }
                output.StatusCode = "00";
                output.AgentRecord = record;
            }
            catch (AgentFileException ex)
            {
                output.StatusCode = ex.StatusCode;
                throw;
            }
        }

        /// <summary>
        /// Handles errors by logging and setting output status code.
        /// </summary>
        /// <param name="operationType">The operation type.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="output">The output object to populate.</param>
        private async Task HandleErrorAsync(string? operationType, string statusCode, string errorMessage, AgentFileOutput output)
        {
            logger.LogError("IN FLDRIVR1");
            logger.LogError("ERROR: {OperationType} ON AGENTVSAM FILE STATUS CODE: {StatusCode}. Message: {ErrorMessage}",
                operationType, statusCode, errorMessage);

            output.StatusCode = statusCode;
            await Task.CompletedTask;

            // In COBOL, ABEND would terminate the program. In .NET, we throw an exception.
            // Optionally, you could rethrow or handle gracefully depending on requirements.
        }
    }

    /// <summary>
    /// Example of setting up dependency injection and running the driver.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point for demonstration.
        /// </summary>
        public static async Task Main(string[] args)
        {
            // Setup DI
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(configure => configure.AddConsole());
            serviceCollection.AddSingleton<IAgentFileRepository>(provider =>
                new FileAgentFileRepository("AGENTVSAM.csv")); // Replace with actual file path
            serviceCollection.AddTransient<AgentFileDriver>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var driver = serviceProvider.GetRequiredService<AgentFileDriver>();

            // Example input
            var input = new AgentFileInput
            {
                OperationType = "SEARCH",
                AgentCode = "A123456789"
            };

            var output = await driver.ExecuteAsync(input);

            Console.WriteLine($"Status Code: {output.StatusCode}");
            if (output.AgentRecord != null)
            {
                Console.WriteLine($"Agent Name: {output.AgentRecord.AgentName}");
                // Output other fields as needed
            }
            else
            {
                Console.WriteLine("Agent not found or error occurred.");
            }
        }
    }
}