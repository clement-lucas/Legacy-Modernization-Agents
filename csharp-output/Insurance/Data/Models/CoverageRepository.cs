using System;

namespace Insurance.Data.Models
{
    /// <summary>
    /// Represents a coverage record mapped from the INSURNCE.TCOVERAG DB2 table.
    /// </summary>
    /// <remarks>
    /// This record corresponds to the COBOL group variable DCLCOVGE and the DB2 table structure.
    /// </remarks>
    public sealed record CoverageRecord
    {
        /// <summary>
        /// Gets the policy number associated with the coverage.
        /// </summary>
        /// <remarks>
        /// COBOL: COVERAGE-POL-NUM (PIC X(10)), DB2: CHAR(10) NOT NULL
        /// </remarks>
        public string CoveragePolicyNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets the status of the coverage.
        /// </summary>
        /// <remarks>
        /// COBOL: COVERAGE-STATUS (PIC X(10)), DB2: CHAR(10) NOT NULL
        /// </remarks>
        public string CoverageStatus { get; init; } = string.Empty;

        /// <summary>
        /// Gets the start date of the coverage in ISO format (yyyy-MM-dd).
        /// </summary>
        /// <remarks>
        /// COBOL: COVERAGE-START-DT (PIC X(10)), DB2: CHAR(10) NOT NULL
        /// </remarks>
        public string CoverageStartDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the end date of the coverage in ISO format (yyyy-MM-dd).
        /// </summary>
        /// <remarks>
        /// COBOL: COVERAGE-END-DT (PIC X(10)), DB2: CHAR(10) NOT NULL
        /// </remarks>
        public string CoverageEndDate { get; init; } = string.Empty;

        /// <summary>
        /// Gets the timestamp when the coverage record was added.
        /// </summary>
        /// <remarks>
        /// COBOL: COVERAGE-ADD-TS (PIC X(26)), DB2: TIMESTAMP NOT NULL WITH DEFAULT
        /// </remarks>
        public DateTime CoverageAddedTimestamp { get; init; }

        /// <summary>
        /// Gets the timestamp when the coverage record was last updated.
        /// </summary>
        /// <remarks>
        /// COBOL: COVERAGE-UPDATE-TS (PIC X(26)), DB2: TIMESTAMP NOT NULL WITH DEFAULT
        /// </remarks>
        public DateTime CoverageUpdatedTimestamp { get; init; }
    }
}

namespace Insurance.Data.Repositories
{
    using Insurance.Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the contract for accessing coverage records from the data store.
    /// </summary>
    public interface ICoverageRepository
    {
        /// <summary>
        /// Asynchronously retrieves all coverage records.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with a list of coverage records as result.</returns>
        Task<IReadOnlyList<CoverageRecord>> GetAllCoveragesAsync();

        /// <summary>
        /// Asynchronously retrieves a coverage record by policy number.
        /// </summary>
        /// <param name="policyNumber">The policy number to search for.</param>
        /// <returns>A task representing the asynchronous operation, with the coverage record as result, or null if not found.</returns>
        Task<CoverageRecord?> GetCoverageByPolicyNumberAsync(string policyNumber);
    }

    /// <summary>
    /// Provides access to coverage records in the data store.
    /// </summary>
    /// <remarks>
    /// Implements error handling using exceptions and logging.
    /// </remarks>
    public class CoverageRepository : ICoverageRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<CoverageRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageRepository"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection to use.</param>
        /// <param name="logger">The logger instance for error logging.</param>
        public CoverageRepository(IDbConnection dbConnection, ILogger<CoverageRepository> logger)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CoverageRecord>> GetAllCoveragesAsync()
        {
            try
            {
                // Example using Dapper for simplicity; replace with EF Core or other ORM as needed.
                var sql = @"
                    SELECT
                        COVERAGE_POL_NUM AS CoveragePolicyNumber,
                        COVERAGE_STATUS AS CoverageStatus,
                        COVERAGE_START_DT AS CoverageStartDate,
                        COVERAGE_END_DT AS CoverageEndDate,
                        COVERAGE_ADD_TS AS CoverageAddedTimestamp,
                        COVERAGE_UPDATE_TS AS CoverageUpdatedTimestamp
                    FROM INSURNCE.TCOVERAG
                ";

                var result = await _dbConnection.QueryAsync<CoverageRecord>(sql);
                return result.AsList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve coverage records.");
                throw new DataAccessException("An error occurred while retrieving coverage records.", ex);
            }
        }

        /// <inheritdoc />
        public async Task<CoverageRecord?> GetCoverageByPolicyNumberAsync(string policyNumber)
        {
            try
            {
                var sql = @"
                    SELECT
                        COVERAGE_POL_NUM AS CoveragePolicyNumber,
                        COVERAGE_STATUS AS CoverageStatus,
                        COVERAGE_START_DT AS CoverageStartDate,
                        COVERAGE_END_DT AS CoverageEndDate,
                        COVERAGE_ADD_TS AS CoverageAddedTimestamp,
                        COVERAGE_UPDATE_TS AS CoverageUpdatedTimestamp
                    FROM INSURNCE.TCOVERAG
                    WHERE COVERAGE_POL_NUM = @PolicyNumber
                ";

                var result = await _dbConnection.QuerySingleOrDefaultAsync<CoverageRecord>(
                    sql, new { PolicyNumber = policyNumber });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve coverage record for policy number {PolicyNumber}.", policyNumber);
                throw new DataAccessException($"An error occurred while retrieving coverage record for policy number {policyNumber}.", ex);
            }
        }
    }

    /// <summary>
    /// Represents errors that occur during data access operations.
    /// </summary>
    public class DataAccessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataAccessException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}