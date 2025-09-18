using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InSureX.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        public string? EmployeeId { get; set; } 

        [ForeignKey("EmployeeId")]
        public ApplicationUser? Employee { get; set; } 

        [Required]
        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Total Amount is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public decimal? ApprovedAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Claim details are required.")]
        public string Details { get; set; } = string.Empty;

        public string? HospitalName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? TreatmentDate { get; set; }
        public string? BillFilePath { get; set; } 
    }

}
