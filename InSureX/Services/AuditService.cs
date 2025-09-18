using InSureX.Data;
using InSureX.Models;
using System.Security.Claims;

namespace InSureX.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Logs any action into AuditTrail table
        public async Task LogAsync(
            string actionType,
            string tableAffected,
            int? claimId,
            string? employeeId,
            ClaimsPrincipal user)
        {
            try
            {
                var audit = new AuditTrail
                {
                    ActionType = actionType,
                    TableAffected = tableAffected,
                    ClaimId = claimId,
                    EmployeeId = employeeId,
                    ActionBy = user.Identity?.Name ?? "System",
                    Timestamp = DateTime.UtcNow 
                };

                _context.AuditTrails.Add(audit);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditService] Error logging action: {ex.Message}");
            }
        }
    }
}
