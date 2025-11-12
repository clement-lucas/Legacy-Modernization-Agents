using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Insurance.DataDriver
{
    /// <summary>
    /// Represents the request parameters for the PolicyDriver operation.
    /// </summary>
    public record PolicyDriverRequest
    {
        /// <summary>
        /// The operation type: OPEN, FETCH, or CLOSE.
        /// </summary>
        public string OperationType { get; init; } = string.Empty;

        /// <summary>
        /// The process date in yyyy-MM-dd format.
        /// </summary>
        public string ProcessDate { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents the response from the PolicyDriver operation.
    /// </summary>
    public record PolicyDriverResponse
    {
        /// <summary>
        /// The SQL status code (0 = success, 100 = no more rows, negative = error).
        /// </summary>
        public int SqlCode { get; init; }

        /// <summary>
        /// The policy data buffer (serialized as string, e.g., JSON).
        /// </summary>
        public string? PolicyData { get; init; }
    }

    /// <summary>
    /// Represents a health insurance policy record.
    /// </summary>
    public record PolicyRecord
    {
        public string PolicyNumber { get; init; } = string.Empty;
        public string PolicyHolderFirstName { get; init; } = string.Empty;
        public string PolicyHolderMiddleName { get; init; } = string.Empty;
        public string PolicyHolderLastName { get; init; } = string.Empty;
        public string PolicyBeneficiaryName { get; init; } = string.Empty;
        public string PolicyBeneficiaryRelation { get; init; } = string.Empty;
        public string PolicyHolderAddress1 { get; init; } = string.Empty;
        public string PolicyHolderAddress2 { get; init; } = string.Empty;
        public string PolicyHolderCity { get; init; } = string.Empty;
        public string PolicyHolderState { get; init; } = string.Empty;
        public string PolicyHolderZipCode { get; init; } = string.Empty;
        public DateTime PolicyHolderDateOfBirth { get; init; }
        public string PolicyHolderGender { get; init; } = string.Empty;
        public string PolicyHolderPhone { get; init; } = string.Empty;
        public string PolicyHolderEmail { get; init; } = string.Empty;
        public string PolicyPaymentFrequency { get; init; } = string.Empty;
        public string PolicyPaymentMethod { get; init; } = string.Empty;
        public string PolicyUnderwriter { get; init; } = string.Empty;
        public string PolicyTermsConditions { get; init; } = string.Empty;
        public bool PolicyClaimed { get; init; }
        public string PolicyDiscountCode { get; init; } = string.Empty;
        public decimal PolicyPremiumAmount { get; init; }
        public decimal PolicyCoverageAmount { get; init; }
        public string PolicyType { get; init; } = string.Empty;
        public DateTime PolicyStartDate { get; init; }
        public DateTime PolicyExpiryDate { get; init; }
        public string PolicyStatus { get; init; } = string.Empty;
        public string PolicyAgentCode { get; init; } = string.Empty;
        public bool PolicyNotifyFlag { get; init; }
        public DateTime PolicyAddTimestamp { get; init; }
        public DateTime PolicyUpdateTimestamp { get; init; }
    }

    /// <summary>
    /// Enumerates the supported operations for the PolicyDriver.
    /// </summary>
    public enum PolicyDriverOperation
    {
        Open,
        Fetch,
        Close
    }

    /// <summary>
    /// Provides operations for managing health insurance policy records via database cursor.
    /// </summary>
    public class PolicyDriver : IAsyncDisposable
    {
        private readonly DbConnection _dbConnection;
        private readonly ILogger<PolicyDriver> _logger;
        private DbCommand? _policyCursorCommand;
        private DbDataReader? _policyCursorReader;
        private bool _cursorOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDriver"/> class.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="logger">The logger instance.</param>
        public PolicyDriver(DbConnection dbConnection, ILogger<PolicyDriver> logger)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the requested operation (OPEN, FETCH, CLOSE) on the policy cursor.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <returns>The response containing SQL code and policy data.</returns>
        public async Task<PolicyDriverResponse> ExecuteAsync(PolicyDriverRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var operation = ParseOperationType(request.OperationType);
            int sqlCode = 0;
            string? policyData = null;

            try
            {
                switch (operation)
                {
                    case PolicyDriverOperation.Open:
                        sqlCode = await OpenPolicyCursorAsync(request.ProcessDate);
                        break;

                    case PolicyDriverOperation.Fetch:
                        (sqlCode, policyData) = await FetchPolicyCursorAsync();
                        break;

                    case PolicyDriverOperation.Close:
                        sqlCode = await ClosePolicyCursorAsync();
                        break;

                    default:
                        _logger.LogError("Invalid operation type: {OperationType}", request.OperationType);
                        sqlCode = -1;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during PolicyDriver operation: {OperationType}", request.OperationType);
                sqlCode = -2; // Custom error code for exception
            }

            return new PolicyDriverResponse
            {
                SqlCode = sqlCode,
                PolicyData = policyData
            };
        }

        /// <summary>
        /// Opens the policy cursor for fetching records.
        /// </summary>
        /// <param name="processDate">The process date in yyyy-MM-dd format.</param>
        /// <returns>SQL code (0 = success, negative = error).</returns>
        private async Task<int> OpenPolicyCursorAsync(string processDate)
        {
            if (_cursorOpen)
            {
                _logger.LogWarning("Policy cursor is already open.");
                return 0;
            }

            await EnsureConnectionOpenAsync();

            // Parse process date
            if (!DateTime.TryParse(processDate, out var processDateValue))
            {
                _logger.LogError("Invalid process date format: {ProcessDate}", processDate);
                return -3;
            }

            // Prepare SQL command (cursor)
            string sql = @"
                SELECT 
                    POLICY_NUMBER,
                    POLICY_HOLDER_FNAME,
                    POLICY_HOLDER_MNAME,
                    POLICY_HOLDER_LNAME,
                    POLICY_BENEF_NAME,
                    POLICY_BENEF_RELATION,
                    POLICY_HOLDER_ADDR_1,
                    POLICY_HOLDER_ADDR_2,
                    POLICY_HOLDER_CITY,
                    POLICY_HOLDER_STATE,
                    POLICY_HOLDER_ZIP_CD,
                    POLICY_HOLDER_DOB,
                    POLICY_HOLDER_GENDER,
                    POLICY_HOLDER_PHONE,
                    POLICY_HOLDER_EMAIL,
                    POLICY_PAYMENT_FREQUENCY,
                    POLICY_PAYMENT_METHOD,
                    POLICY_UNDERWRITER,
                    POLICY_TERMS_CONDITIONS,
                    POLICY_CLAIMED,
                    POLICY_DISCOUNT_CODE,
                    POLICY_PREMIUM_AMOUNT,
                    POLICY_COVERAGE_AMOUNT,
                    POLICY_TYPE,
                    POLICY_START_DATE,
                    POLICY_EXPIRY_DATE,
                    POLICY_STATUS,
                    POLICY_AGENT_CODE,
                    POLICY_NOTIFY_FLAG,
                    POLICY_ADD_TIMESTAMP,
                    POLICY_UPDATE_TIMESTAMP
                FROM INSURNCE.TPOLICY p
                INNER JOIN INSURNCE.TCOVERAG c ON p.POLICY_NUMBER = c.COVERAGE_POL_NUM
                WHERE p.POLICY_STATUS = 'A'
                  AND p.POLICY_HOLDER_STATE IN ('CA', 'MN', 'NY')
                  AND DATEDIFF(DAY, @ProcessDate, p.POLICY_EXPIRY_DATE) BETWEEN 30 AND 35
                  AND p.POLICY_TYPE = 'HEALTH'
                  AND c.COVERAGE_STATUS = 'ACTIVE'
                  AND c.COVERAGE_TYPE = 'CHARGEABLE'
                  AND NOT EXISTS (
                      SELECT 1 FROM INSURNCE.TTRAKING t
                      WHERE t.TR_POLICY_NUMBER = p.POLICY_NUMBER
                        AND t.TR_STATUS = 'A'
                  )
                ORDER BY p.POLICY_HOLDER_STATE, p.POLICY_AGENT_CODE
            ";

            _policyCursorCommand = _dbConnection.CreateCommand();
            _policyCursorCommand.CommandText = sql;
            _policyCursorCommand.CommandType = CommandType.Text;

            var processDateParam = _policyCursorCommand.CreateParameter();
            processDateParam.ParameterName = "@ProcessDate";
            processDateParam.DbType = DbType.Date;
            processDateParam.Value = processDateValue;
            _policyCursorCommand.Parameters.Add(processDateParam);

            try
            {
                _policyCursorReader = await _policyCursorCommand.ExecuteReaderAsync();
                _cursorOpen = true;
                return 0;
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Error opening policy cursor.");
                return dbEx.ErrorCode != 0 ? dbEx.ErrorCode : -4;
            }
        }

        /// <summary>
        /// Fetches the next policy record from the cursor.
        /// </summary>
        /// <returns>
        /// Tuple: SQL code (0 = success, 100 = no more rows, negative = error), and serialized policy data.
        /// </returns>
        private async Task<(int SqlCode, string? PolicyData)> FetchPolicyCursorAsync()
        {
            if (!_cursorOpen || _policyCursorReader is null)
            {
                _logger.LogError("Policy cursor is not open.");
                return (-5, null);
            }

            try
            {
                if (await _policyCursorReader.ReadAsync())
                {
                    var policy = MapPolicyRecord(_policyCursorReader);
                    // Serialize to JSON or other format as needed
                    string policyData = System.Text.Json.JsonSerializer.Serialize(policy);
                    return (0, policyData);
                }
                else
                {
                    // No more rows
                    await ClosePolicyCursorAsync();
                    return (100, null);
                }
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Error fetching from policy cursor.");
                return (dbEx.ErrorCode != 0 ? dbEx.ErrorCode : -6, null);
            }
        }

        /// <summary>
        /// Closes the policy cursor.
        /// </summary>
        /// <returns>SQL code (0 = success, negative = error).</returns>
        private async Task<int> ClosePolicyCursorAsync()
        {
            if (!_cursorOpen)
            {
                _logger.LogWarning("Policy cursor is not open.");
                return 0;
            }

            try
            {
                if (_policyCursorReader is not null)
                {
                    await _policyCursorReader.DisposeAsync();
                    _policyCursorReader = null;
                }
                if (_policyCursorCommand is not null)
                {
                    await _policyCursorCommand.DisposeAsync();
                    _policyCursorCommand = null;
                }
                _cursorOpen = false;
                return 0;
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Error closing policy cursor.");
                return dbEx.ErrorCode != 0 ? dbEx.ErrorCode : -7;
            }
        }

        /// <summary>
        /// Maps a data reader row to a <see cref="PolicyRecord"/>.
        /// </summary>
        /// <param name="reader">The data reader.</param>
        /// <returns>The mapped policy record.</returns>
        private static PolicyRecord MapPolicyRecord(DbDataReader reader)
        {
            return new PolicyRecord
            {
                PolicyNumber = reader.GetString(reader.GetOrdinal("POLICY_NUMBER")),
                PolicyHolderFirstName = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_FNAME")),
                PolicyHolderMiddleName = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_MNAME")),
                PolicyHolderLastName = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_LNAME")),
                PolicyBeneficiaryName = reader.GetString(reader.GetOrdinal("POLICY_BENEF_NAME")),
                PolicyBeneficiaryRelation = reader.GetString(reader.GetOrdinal("POLICY_BENEF_RELATION")),
                PolicyHolderAddress1 = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_ADDR_1")),
                PolicyHolderAddress2 = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_ADDR_2")),
                PolicyHolderCity = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_CITY")),
                PolicyHolderState = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_STATE")),
                PolicyHolderZipCode = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_ZIP_CD")),
                PolicyHolderDateOfBirth = reader.GetDateTime(reader.GetOrdinal("POLICY_HOLDER_DOB")),
                PolicyHolderGender = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_GENDER")),
                PolicyHolderPhone = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_PHONE")),
                PolicyHolderEmail = reader.GetString(reader.GetOrdinal("POLICY_HOLDER_EMAIL")),
                PolicyPaymentFrequency = reader.GetString(reader.GetOrdinal("POLICY_PAYMENT_FREQUENCY")),
                PolicyPaymentMethod = reader.GetString(reader.GetOrdinal("POLICY_PAYMENT_METHOD")),
                PolicyUnderwriter = reader.GetString(reader.GetOrdinal("POLICY_UNDERWRITER")),
                PolicyTermsConditions = reader.GetString(reader.GetOrdinal("POLICY_TERMS_CONDITIONS")),
                PolicyClaimed = reader.GetBoolean(reader.GetOrdinal("POLICY_CLAIMED")),
                PolicyDiscountCode = reader.GetString(reader.GetOrdinal("POLICY_DISCOUNT_CODE")),
                PolicyPremiumAmount = reader.GetDecimal(reader.GetOrdinal("POLICY_PREMIUM_AMOUNT")),
                PolicyCoverageAmount = reader.GetDecimal(reader.GetOrdinal("POLICY_COVERAGE_AMOUNT")),
                PolicyType = reader.GetString(reader.GetOrdinal("POLICY_TYPE")),
                PolicyStartDate = reader.GetDateTime(reader.GetOrdinal("POLICY_START_DATE")),
                PolicyExpiryDate = reader.GetDateTime(reader.GetOrdinal("POLICY_EXPIRY_DATE")),
                PolicyStatus = reader.GetString(reader.GetOrdinal("POLICY_STATUS")),
                PolicyAgentCode = reader.GetString(reader.GetOrdinal("POLICY_AGENT_CODE")),
                PolicyNotifyFlag = reader.GetBoolean(reader.GetOrdinal("POLICY_NOTIFY_FLAG")),
                PolicyAddTimestamp = reader.GetDateTime(reader.GetOrdinal("POLICY_ADD_TIMESTAMP")),
                PolicyUpdateTimestamp = reader.GetDateTime(reader.GetOrdinal("POLICY_UPDATE_TIMESTAMP"))
            };
        }

        /// <summary>
        /// Parses the operation type string to the corresponding enum value.
        /// </summary>
        /// <param name="operationType">The operation type string.</param>
        /// <returns>The parsed <see cref="PolicyDriverOperation"/>.</returns>
        private static PolicyDriverOperation ParseOperationType(string operationType) =>
            operationType?.Trim().ToUpperInvariant() switch
            {
                "OPEN" => PolicyDriverOperation.Open,
                "FETCH" => PolicyDriverOperation.Fetch,
                "CLOSE" => PolicyDriverOperation.Close,
                _ => throw new ArgumentException($"Invalid operation type: {operationType}", nameof(operationType))
            };

        /// <summary>
        /// Ensures the database connection is open.
        /// </summary>
        private async Task EnsureConnectionOpenAsync()
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_policyCursorReader is not null)
                await _policyCursorReader.DisposeAsync();
            if (_policyCursorCommand is not null)
                await _policyCursorCommand.DisposeAsync();
            if (_dbConnection.State == ConnectionState.Open)
                await _dbConnection.CloseAsync();
        }
    }
}