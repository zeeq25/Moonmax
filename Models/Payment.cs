using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int InvoiceID { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // Cash or PDC

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } // Date payment was received

        // ===== PDC-SPECIFIC FIELDS =====

        [StringLength(100)]
        public string CheckNumber { get; set; }

        [StringLength(200)]
        public string BankName { get; set; }

        public DateTime? CheckDate { get; set; } // Date written on the check (maturity date)

        [StringLength(50)]
        public string PDCStatus { get; set; } // Received, Deposited, Cleared, Bounced, Cancelled

        public DateTime? DepositDate { get; set; } // Date check was deposited to bank

        public DateTime? ClearanceDate { get; set; } // Date check cleared

        [StringLength(500)]
        public string BounceReason { get; set; } // Reason if check bounced

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? BankCharges { get; set; } // Bank charges if check bounced

        // ===== TRACKING FIELDS =====

        [StringLength(100)]
        public string ReferenceNumber { get; set; } // Transaction/Receipt number

        [StringLength(500)]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int ProcessedByUserID { get; set; }

        public int? DepositedByUserID { get; set; } // User who deposited the check

        public int? ClearedByUserID { get; set; } // User who confirmed clearance

        // Navigation Properties
        [ForeignKey("InvoiceID")]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey("ProcessedByUserID")]
        public virtual User ProcessedByUser { get; set; }

        [ForeignKey("DepositedByUserID")]
        public virtual User DepositedByUser { get; set; }

        [ForeignKey("ClearedByUserID")]
        public virtual User ClearedByUser { get; set; }
    }
}