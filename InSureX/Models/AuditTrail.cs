using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InSureX.Models
{
    public class AuditTrail
    {
        [Key]
        public int AuditId { get; set; }

        public string ActionBy { get; set; } = string.Empty;

        [Required]
        public string ActionType { get; set; } = string.Empty;

        [Required]
        public string TableAffected { get; set; } = string.Empty;

        public int? ClaimId { get; set; } 
        public string? EmployeeId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("EmployeeId")]
        public ApplicationUser? Employee { get; set; }
    }
}
