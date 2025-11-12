using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Insurance.Tracking
{
    /// <summary>
    /// Represents the input parameters for the tracking operation.
    /// </summary>
    public record TrackingOperationInput(
        string OperationType,
        string ProcessDate,
        string PolicyNumber
    );

    /// <summary>
    /// Represents the output result of the tracking operation.
    /// </summary>
    public record TrackingOperationResult(
        int SqlCode
    );

    /// <summary>
    /// Represents a tracking record in the INSURNCE.TTRAKING table.
    /// </summary>
    public record TrackingRecord(
        string PolicyNumber,
        string NotifyDate,
        string Status,
        DateTime AddTimestamp,
        DateTime UpdateTimestamp
    );

    /// <summary>
    /// Interface for tracking repository to abstract database operations.
    /// </summary>
    public interface ITrackingRepository
    {
        /// <summary>
        /// Retrieves a tracking record by policy number.
        /// </summary>
        /// <param name="policyNumber">The policy number to search for.</param>
        /// <returns>The tracking record if found; otherwise, null.</returns>
        Task<TrackingRecord?> GetTrackingRecordAsync(string policyNumber);

        /// <summary>
        /// Inserts a new tracking record.
        /// </summary>
        /// <param name="record">The tracking record to insert.</param>
        /// <returns>The SQL code resulting from the operation.</returns>
        Task<int> InsertTrackingRecordAsync(TrackingRecord record);

        /// <summary>
        /// Updates an existing tracking record.
        /// </summary>
        /// <param name="record">The tracking record to update.</param>
        /// <returns>The SQL code resulting from the operation.</returns>
        Task<int> UpdateTrackingRecordAsync(TrackingRecord record);
    }

    /// <summary>
    /// Implementation of <see cref="ITrackingRepository"/> using ADO.NET.
    /// </summary>
    public class TrackingRepository : ITrackingRepository
    {
        private readonly DbConnection _connection;

        public TrackingRepository(DbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<TrackingRecord?> GetTrackingRecordAsync(string policyNumber)
        {
            const string sql = @"
                SELECT TR_POLICY_NUMBER, TR_NOTIFY_DATE, TR_STATUS, TR_ADD_TIMESTAMP, TR_UPDATE_TIMESTAMP
                FROM INSURNCE.TTRAKING
                WHERE TR_POLICY_NUMBER = @PolicyNumber
            ";

            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            var param = cmd.CreateParameter();
            param.ParameterName = "@PolicyNumber";
            param.DbType = DbType.String;
            param.Value = policyNumber;
            cmd.Parameters.Add(param);

            await _connection.OpenAsync();

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new TrackingRecord(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetDateTime(4)
                );
            }
            return null;
        }

        public async Task<int> InsertTrackingRecordAsync(TrackingRecord record)
        {
            const string sql = @"
                INSERT INTO INSURNCE.TTRAKING (
                    TR_POLICY_NUMBER,
                    TR_NOTIFY_DATE,
                    TR_STATUS,
                    TR_ADD_TIMESTAMP,
                    TR_UPDATE_TIMESTAMP
                ) VALUES (
                    @PolicyNumber,
                    @NotifyDate,
                    @Status,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                )
            ";

            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(CreateParameter(cmd, "@PolicyNumber", DbType.String, record.PolicyNumber));
            cmd.Parameters.Add(CreateParameter(cmd, "@NotifyDate", DbType.String, record.NotifyDate));
            cmd.Parameters.Add(CreateParameter(cmd, "@Status", DbType.String, record.Status));

            try
            {
                await _connection.OpenAsync();
                var affected = await cmd.ExecuteNonQueryAsync();
                return affected == 1 ? 0 : -1; // 0 for success, -1 for failure
            }
            catch (DbException ex)
            {
                // Log or handle exception as needed
                return ex.ErrorCode;
            }
        }

        public async Task<int> UpdateTrackingRecordAsync(TrackingRecord record)
        {
            const string sql = @"
                UPDATE INSURNCE.TTRAKING
                SET
                    TR_NOTIFY_DATE = @NotifyDate,
                    TR_STATUS = @Status,
                    TR_UPDATE_TIMESTAMP = CURRENT_TIMESTAMP
                WHERE TR_POLICY_NUMBER = @PolicyNumber
            ";

            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.Add(CreateParameter(cmd, "@NotifyDate", DbType.String, record.NotifyDate));
            cmd.Parameters.Add(CreateParameter(cmd, "@Status", DbType.String, record.Status));
            cmd.Parameters.Add(CreateParameter(cmd, "@PolicyNumber", DbType.String, record.PolicyNumber));

            try
            {
                await _connection.OpenAsync();
                var affected = await cmd.ExecuteNonQueryAsync();
                return affected == 1 ? 0 : -1; // 0 for success, -1 for failure
            }
            catch (DbException ex)
            {
                // Log or handle exception as needed
                return ex.ErrorCode;
            }
        }

        private static DbParameter CreateParameter(DbCommand cmd, string name, DbType type, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.DbType = type;
            param.Value = value ?? DBNull.Value;
            return param;
        }
    }

    /// <summary>
    /// Service class that implements the main logic for inserting or updating insurance tracking records.
    /// </summary>
    public class TrackingDriverService
    {
        private readonly ITrackingRepository _repository;
        private readonly ILogger<TrackingDriverService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingDriverService"/> class.
        /// </summary>
        /// <param name="repository">The tracking repository.</param>
        /// <param name="logger">The logger instance.</param>
        public TrackingDriverService(ITrackingRepository repository, ILogger<TrackingDriverService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the tracking operation (insert or update) based on the input parameters.
        /// </summary>
        /// <param name="input">The input parameters for the operation.</param>
        /// <returns>The result containing the SQL code.</returns>
        /// <exception cref="ArgumentException">Thrown when the operation type is invalid.</exception>
        public async Task<TrackingOperationResult> ExecuteAsync(TrackingOperationInput input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            int sqlCode = 0;

            try
            {
                // Validate operation type
                var operationType = input.OperationType?.Trim().ToUpperInvariant();
                if (operationType != "INSERT" && operationType != "UPDATE")
                {
                    _logger.LogError("Invalid operation type: {OperationType}", input.OperationType);
                    sqlCode = -1;
                    return new TrackingOperationResult(sqlCode);
                }

                // Select tracking record
                var trackingRecord = await _repository.GetTrackingRecordAsync(input.PolicyNumber);

                // Populate tracking record for insert/update
                var recordToPersist = new TrackingRecord(
                    PolicyNumber: input.PolicyNumber,
                    NotifyDate: input.ProcessDate,
                    Status: "A",
                    AddTimestamp: DateTime.UtcNow,
                    UpdateTimestamp: DateTime.UtcNow
                );

                if (trackingRecord is null)
                {
                    // Not present in tracking: perform insert
                    sqlCode = await _repository.InsertTrackingRecordAsync(recordToPersist);
                    if (sqlCode != 0)
                    {
                        _logger.LogError("Error inserting into TTRAKING. SQLCODE: {SqlCode}", sqlCode);
                        // Optionally, throw or handle as needed
                    }
                }
                else
                {
                    // Present in tracking: perform update
                    sqlCode = await _repository.UpdateTrackingRecordAsync(recordToPersist);
                    if (sqlCode != 0)
                    {
                        _logger.LogError("Error updating TTRAKING. SQLCODE: {SqlCode}", sqlCode);
                        // Optionally, throw or handle as needed
                    }
                }
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred during tracking operation.");
                sqlCode = dbEx.ErrorCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during tracking operation.");
                sqlCode = -99999; // Arbitrary error code for unexpected errors
            }

            return new TrackingOperationResult(sqlCode);
        }
    }
}