using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insurance.Data.Models
{
    /// <summary>
    /// Represents a row in the INSURNCE.TTRAKING DB2 table.
    /// Maps COBOL group item DCLTRAKI and its columns.
    /// </summary>
    [Table("TTRAKING", Schema = "INSURNCE")]
    public record TrackingRecord
    {
        /// <summary>
        /// Gets or sets the policy number.
        /// COBOL: TR-POLICY-NUMBER (PIC X(10))
        /// </summary>
        [Column("TR_POLICY_NUMBER")]
        [Required]
        [StringLength(10)]
        public string PolicyNumber { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the notification date.
        /// COBOL: TR-NOTIFY-DATE (PIC X(10)), DB2: DATE
        /// </summary>
        [Column("TR_NOTIFY_DATE")]
        [Required]
        public DateTime NotifyDate { get; init; }

        /// <summary>
        /// Gets or sets the status.
        /// COBOL: TR-STATUS (PIC X(1))
        /// </summary>
        [Column("TR_STATUS")]
        [Required]
        [StringLength(1)]
        public string Status { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the add timestamp.
        /// COBOL: TR-ADD-TIMESTAMP (PIC X(26)), DB2: TIMESTAMP
        /// </summary>
        [Column("TR_ADD_TIMESTAMP")]
        [Required]
        public DateTime AddTimestamp { get; init; }

        /// <summary>
        /// Gets or sets the update timestamp.
        /// COBOL: TR-UPDATE-TIMESTAMP (PIC X(26)), DB2: TIMESTAMP
        /// </summary>
        [Column("TR_UPDATE_TIMESTAMP")]
        [Required]
        public DateTime UpdateTimestamp { get; init; }
    }
}