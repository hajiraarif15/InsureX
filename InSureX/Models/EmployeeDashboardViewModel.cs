using System.Collections.Generic;

namespace InSureX.Models
{
    public class EmployeeDashboardViewModel
    {
        public List<Claim> Claims { get; set; } = new List<Claim>();
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
    }
}
