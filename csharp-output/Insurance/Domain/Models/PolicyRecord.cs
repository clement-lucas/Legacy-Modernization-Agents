using System;
using System.Globalization;

namespace Insurance.Domain.Models
{
    /// <summary>
    /// Represents a single insurance policy record, containing all relevant details about the policy,
    /// policy holder, beneficiary, payment, underwriting, terms, status, and audit timestamps.
    /// </summary>
    public record PolicyRecord
    {
        /// <summary>
        /// Unique policy number.
        /// </summary>
        public string? PolicyNumber { get; init; }

        /// <summary>
        /// Policy holder's first name.
        /// </summary>
        public string? PolicyHolderFirstName { get; init; }

        /// <summary>
        /// Policy holder's middle initial.
        /// </summary>
        public string? PolicyHolderMiddleName { get; init; }

        /// <summary>
        /// Policy holder's last name.
        /// </summary>
        public string? PolicyHolderLastName { get; init; }

        /// <summary>
        /// Beneficiary's full name.
        /// </summary>
        public string? PolicyBeneficiaryName { get; init; }

        /// <summary>
        /// Beneficiary's relationship to the policy holder.
        /// </summary>
        public string? PolicyBeneficiaryRelation { get; init; }

        /// <summary>
        /// Policy holder's address line 1.
        /// </summary>
        public string? PolicyHolderAddress1 { get; init; }

        /// <summary>
        /// Policy holder's address line 2.
        /// </summary>
        public string? PolicyHolderAddress2 { get; init; }

        /// <summary>
        /// Policy holder's city.
        /// </summary>
        public string? PolicyHolderCity { get; init; }

        /// <summary>
        /// Policy holder's state code.
        /// </summary>
        public string? PolicyHolderState { get; init; }

        /// <summary>
        /// Policy holder's ZIP code.
        /// </summary>
        public string? PolicyHolderZipCode { get; init; }

        /// <summary>
        /// Policy holder's date of birth (format: yyyy-MM-dd or as stored).
        /// </summary>
        public string? PolicyHolderDateOfBirth { get; init; }

        /// <summary>
        /// Policy holder's gender.
        /// </summary>
        public string? PolicyHolderGender { get; init; }

        /// <summary>
        /// Policy holder's phone number.
        /// </summary>
        public string? PolicyHolderPhone { get; init; }

        /// <summary>
        /// Policy holder's email address.
        /// </summary>
        public string? PolicyHolderEmail { get; init; }

        /// <summary>
        /// Payment frequency (e.g., Monthly, Quarterly).
        /// </summary>
        public string? PolicyPaymentFrequency { get; init; }

        /// <summary>
        /// Payment method (e.g., Direct Debit, Credit Card).
        /// </summary>
        public string? PolicyPaymentMethod { get; init; }

        /// <summary>
        /// Underwriter's name.
        /// </summary>
        public string? PolicyUnderwriter { get; init; }

        /// <summary>
        /// Policy terms and conditions.
        /// </summary>
        public string? PolicyTermsAndConditions { get; init; }

        /// <summary>
        /// Indicates if the policy has been claimed ('Y' or 'N').
        /// </summary>
        public string? PolicyClaimed { get; init; }

        /// <summary>
        /// Discount code applied to the policy.
        /// </summary>
        public string? PolicyDiscountCode { get; init; }

        /// <summary>
        /// Premium amount for the policy.
        /// </summary>
        public decimal PolicyPremiumAmount { get; init; }

        /// <summary>
        /// Type of policy (e.g., Life, Auto).
        /// </summary>
        public string? PolicyType { get; init; }

        /// <summary>
        /// Policy start date (format: yyyy-MM-dd or as stored).
        /// </summary>
        public string? PolicyStartDate { get; init; }

        /// <summary>
        /// Policy expiry date (format: yyyy-MM-dd or as stored).
        /// </summary>
        public string? PolicyExpiryDate { get; init; }

        /// <summary>
        /// Policy status (e.g., Active, Cancelled).
        /// </summary>
        public string? PolicyStatus { get; init; }

        /// <summary>
        /// Agent code associated with the policy.
        /// </summary>
        public string? PolicyAgentCode { get; init; }

        /// <summary>
        /// Notification flag ('Y' or 'N').
        /// </summary>
        public string? PolicyNotifyFlag { get; init; }

        /// <summary>
        /// Timestamp when the policy was added (format: yyyy-MM-dd HH:mm:ss.fff or as stored).
        /// </summary>
        public string? PolicyAddTimestamp { get; init; }

        /// <summary>
        /// Timestamp when the policy was last updated (format: yyyy-MM-dd HH:mm:ss.fff or as stored).
        /// </summary>
        public string? PolicyUpdateTimestamp { get; init; }

        /// <summary>
        /// Gets the parsed date of birth, if available and valid.
        /// </summary>
        /// <returns>DateTime if parseable, otherwise null.</returns>
        public DateTime? GetPolicyHolderDateOfBirth()
        {
            if (DateTime.TryParseExact(PolicyHolderDateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
                return dob;
            return null;
        }

        /// <summary>
        /// Gets the parsed policy start date, if available and valid.
        /// </summary>
        public DateTime? GetPolicyStartDate()
        {
            if (DateTime.TryParseExact(PolicyStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                return startDate;
            return null;
        }

        /// <summary>
        /// Gets the parsed policy expiry date, if available and valid.
        /// </summary>
        public DateTime? GetPolicyExpiryDate()
        {
            if (DateTime.TryParseExact(PolicyExpiryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiryDate))
                return expiryDate;
            return null;
        }

        /// <summary>
        /// Indicates whether the policy has been claimed.
        /// </summary>
        public bool IsClaimed => string.Equals(PolicyClaimed, "Y", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates whether notifications are enabled for the policy.
        /// </summary>
        public bool IsNotificationEnabled => string.Equals(PolicyNotifyFlag, "Y", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the parsed add timestamp, if available and valid.
        /// </summary>
        public DateTime? GetAddTimestamp()
        {
            if (DateTime.TryParse(PolicyAddTimestamp, out var ts))
                return ts;
            return null;
        }

        /// <summary>
        /// Gets the parsed update timestamp, if available and valid.
        /// </summary>
        public DateTime? GetUpdateTimestamp()
        {
            if (DateTime.TryParse(PolicyUpdateTimestamp, out var ts))
                return ts;
            return null;
        }
    }
}