using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace InSureX.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // Sequential integer in DB
        public int EmployeeNumber { get; set; }

        // Computed property - shows EMP001, EMP002, ...
        [NotMapped]
        public string EmployeeId => $"EMP{EmployeeNumber.ToString("D3")}";

        public DateTime? LastLoginTime { get; set; }
    }
}
