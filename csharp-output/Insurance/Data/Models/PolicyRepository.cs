#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Insurance.Data.Models
{
    /// <summary>
    /// Represents an insurance policy record mapped to the INSURNCE.TPOLICY DB2 table.
    /// </summary>
    [Table("TPOLICY", Schema = "INSURNCE")]
    public record Policy
    {
        /// <summary>
        /// Gets or sets the unique policy number.
        /// </summary>
        [Key]
        [Column("POLICY_NUMBER")]
        [Required]
        [StringLength(10)]
        public string PolicyNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's first name.
        /// </summary>
        [Column("POLICY_HOLDER_FNAME")]
        [Required]
        [StringLength(35)]
        public string PolicyHolderFirstName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's middle initial.
        /// </summary>
        [Column("POLICY_HOLDER_MNAME")]
        [Required]
        [StringLength(1)]
        public string PolicyHolderMiddleName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's last name.
        /// </summary>
        [Column("POLICY_HOLDER_LNAME")]
        [Required]
        [StringLength(35)]
        public string PolicyHolderLastName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the beneficiary's name.
        /// </summary>
        [Column("POLICY_BENEF_NAME")]
        [Required]
        [StringLength(60)]
        public string PolicyBeneficiaryName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the beneficiary's relation to the policy holder.
        /// </summary>
        [Column("POLICY_BENEF_RELATION")]
        [Required]
        [StringLength(15)]
        public string PolicyBeneficiaryRelation { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the first line of the policy holder's address.
        /// </summary>
        [Column("POLICY_HOLDER_ADDR_1")]
        [Required]
        [StringLength(100)]
        public string PolicyHolderAddress1 { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the second line of the policy holder's address.
        /// </summary>
        [Column("POLICY_HOLDER_ADDR_2")]
        [Required]
        [StringLength(100)]
        public string PolicyHolderAddress2 { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's city.
        /// </summary>
        [Column("POLICY_HOLDER_CITY")]
        [Required]
        [StringLength(30)]
        public string PolicyHolderCity { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's state.
        /// </summary>
        [Column("POLICY_HOLDER_STATE")]
        [Required]
        [StringLength(2)]
        public string PolicyHolderState { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's ZIP code.
        /// </summary>
        [Column("POLICY_HOLDER_ZIP_CD")]
        [Required]
        [StringLength(10)]
        public string PolicyHolderZipCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's date of birth.
        /// </summary>
        [Column("POLICY_HOLDER_DOB")]
        [Required]
        [StringLength(10)]
        public string PolicyHolderDateOfBirth { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's gender.
        /// </summary>
        [Column("POLICY_HOLDER_GENDER")]
        [Required]
        [StringLength(8)]
        public string PolicyHolderGender { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's phone number.
        /// </summary>
        [Column("POLICY_HOLDER_PHONE")]
        [Required]
        [StringLength(10)]
        public string PolicyHolderPhone { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy holder's email address.
        /// </summary>
        [Column("POLICY_HOLDER_EMAIL")]
        [Required]
        [StringLength(30)]
        public string PolicyHolderEmail { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment frequency for the policy.
        /// </summary>
        [Column("POLICY_PAYMENT_FREQ")]
        [Required]
        [StringLength(10)]
        public string PolicyPaymentFrequency { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment method for the policy.
        /// </summary>
        [Column("POLICY_PAYMENT_METHOD")]
        [Required]
        [StringLength(8)]
        public string PolicyPaymentMethod { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the underwriter for the policy.
        /// </summary>
        [Column("POLICY_UNDERWRITER")]
        [Required]
        [StringLength(50)]
        public string PolicyUnderwriter { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the terms and conditions of the policy.
        /// </summary>
        [Column("POLICY_TERMS_COND")]
        [Required]
        [StringLength(200)]
        public string PolicyTermsAndConditions { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the policy has been claimed.
        /// </summary>
        [Column("POLICY_CLAIMED")]
        [Required]
        [StringLength(1)]
        public string PolicyClaimed { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the discount code applied to the policy.
        /// </summary>
        [Column("POLICY_DISCOUNT_CODE")]
        [Required]
        [StringLength(10)]
        public string PolicyDiscountCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the premium amount for the policy.
        /// </summary>
        [Column("POLICY_PREMIUM_AMOUNT", TypeName = "decimal(7,2)")]
        [Required]
        public decimal PolicyPremiumAmount { get; init; }

        /// <summary>
        /// Gets or sets the coverage amount for the policy.
        /// </summary>
        [Column("POLICY_COVERAGE_AMOUNT", TypeName = "decimal(10,2)")]
        [Required]
        public decimal PolicyCoverageAmount { get; init; }

        /// <summary>
        /// Gets or sets the type of the policy.
        /// </summary>
        [Column("POLICY_TYPE")]
        [Required]
        [StringLength(50)]
        public string PolicyType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the start date of the policy.
        /// </summary>
        [Column("POLICY_START_DATE")]
        [Required]
        public DateTime PolicyStartDate { get; init; }

        /// <summary>
        /// Gets or sets the expiry date of the policy.
        /// </summary>
        [Column("POLICY_EXPIRY_DATE")]
        [Required]
        public DateTime PolicyExpiryDate { get; init; }

        /// <summary>
        /// Gets or sets the status of the policy.
        /// </summary>
        [Column("POLICY_STATUS")]
        [Required]
        [StringLength(1)]
        public string PolicyStatus { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the agent code associated with the policy.
        /// </summary>
        [Column("POLICY_AGENT_CODE")]
        [Required]
        [StringLength(10)]
        public string PolicyAgentCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the notification flag for the policy.
        /// </summary>
        [Column("POLICY_NOTIFY_FLAG")]
        [Required]
        [StringLength(1)]
        public string PolicyNotifyFlag { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the policy was added.
        /// </summary>
        [Column("POLICY_ADD_TIMESTAMP")]
        [Required]
        public DateTime PolicyAddTimestamp { get; init; }

        /// <summary>
        /// Gets or sets the timestamp when the policy was last updated.
        /// </summary>
        [Column("POLICY_UPDATE_TIMESTAMP")]
        [Required]
        public DateTime PolicyUpdateTimestamp { get; init; }
    }
}

namespace Insurance.Data.Repositories
{
    using Insurance.Data.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Defines the contract for accessing insurance policy data.
    /// </summary>
    public interface IPolicyRepository
    {
        /// <summary>
        /// Retrieves a policy by its unique policy number.
        /// </summary>
        /// <param name="policyNumber">The policy number.</param>
        /// <returns>The matching <see cref="Policy"/> or <c>null</c> if not found.</returns>
        Task<Policy?> GetPolicyByNumberAsync(string policyNumber);

        /// <summary>
        /// Retrieves all policies.
        /// </summary>
        /// <returns>A list of <see cref="Policy"/> records.</returns>
        Task<IReadOnlyList<Policy>> GetAllPoliciesAsync();

        /// <summary>
        /// Adds a new policy.
        /// </summary>
        /// <param name="policy">The policy to add.</param>
        /// <returns>The added <see cref="Policy"/>.</returns>
        Task<Policy> AddPolicyAsync(Policy policy);

        /// <summary>
        /// Updates an existing policy.
        /// </summary>
        /// <param name="policy">The policy to update.</param>
        /// <returns>The updated <see cref="Policy"/>.</returns>
        Task<Policy> UpdatePolicyAsync(Policy policy);

        /// <summary>
        /// Deletes a policy by its policy number.
        /// </summary>
        /// <param name="policyNumber">The policy number.</param>
        /// <returns><c>true</c> if deleted; otherwise, <c>false</c>.</returns>
        Task<bool> DeletePolicyAsync(string policyNumber);
    }

    /// <summary>
    /// Provides database access for insurance policies using dependency injection and async patterns.
    /// </summary>
    public class PolicyRepository : IPolicyRepository
    {
        private readonly InsuranceDbContext _dbContext;
        private readonly ILogger<PolicyRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public PolicyRepository(InsuranceDbContext dbContext, ILogger<PolicyRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Policy?> GetPolicyByNumberAsync(string policyNumber)
        {
            try
            {
                return await _dbContext.Policies
                    .FindAsync(policyNumber)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policy with number {PolicyNumber}", policyNumber);
                throw new DataAccessException($"Failed to retrieve policy {policyNumber}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Policy>> GetAllPoliciesAsync()
        {
            try
            {
                return await _dbContext.Policies
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all policies");
                throw new DataAccessException("Failed to retrieve policies", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Policy> AddPolicyAsync(Policy policy)
        {
            try
            {
                var entry = await _dbContext.Policies.AddAsync(policy).ConfigureAwait(false);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                return entry.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding policy {PolicyNumber}", policy.PolicyNumber);
                throw new DataAccessException($"Failed to add policy {policy.PolicyNumber}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Policy> UpdatePolicyAsync(Policy policy)
        {
            try
            {
                _dbContext.Policies.Update(policy);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                return policy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating policy {PolicyNumber}", policy.PolicyNumber);
                throw new DataAccessException($"Failed to update policy {policy.PolicyNumber}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeletePolicyAsync(string policyNumber)
        {
            try
            {
                var policy = await GetPolicyByNumberAsync(policyNumber).ConfigureAwait(false);
                if (policy is null)
                    return false;

                _dbContext.Policies.Remove(policy);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy {PolicyNumber}", policyNumber);
                throw new DataAccessException($"Failed to delete policy {policyNumber}", ex);
            }
        }
    }

    /// <summary>
    /// Custom exception for data access errors.
    /// </summary>
    public class DataAccessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

namespace Insurance.Data
{
    using Insurance.Data.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Represents the database context for insurance data.
    /// </summary>
    public class InsuranceDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the insurance policies.
        /// </summary>
        public DbSet<Policy> Policies { get; set; } = null!;

        /// <summary>
        /// Configures the database schema.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Additional configuration if needed
        }
    }
}