
using InSureX.Data;
using InSureX.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Claim = InSureX.Models.Claim;
using System.Text;

namespace InSureX.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly NotificationService _notificationService;

        public AdminController(
            ApplicationDbContext context,
            AuditService auditService,
            NotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public IActionResult Index() => RedirectToAction("AdminDashboard");

        //Admin Dashboard 
        public async Task<IActionResult> AdminDashboard()
        {
            ViewBag.TotalClaims = await _context.Claims.CountAsync();
            ViewBag.PendingClaims = await _context.Claims.CountAsync(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = await _context.Claims.CountAsync(c => c.Status == "Approved");
            ViewBag.RejectedClaims = await _context.Claims.CountAsync(c => c.Status == "Rejected");
            ViewBag.TotalClaimedAmount = await _context.Claims.SumAsync(c => (decimal?)c.Amount) ?? 0m;
            ViewBag.TotalApprovedAmount = await _context.Claims.SumAsync(c => (decimal?)c.ApprovedAmount) ?? 0m;
            ViewBag.TotalEmployees = await _context.Users.CountAsync();

            return View();
        }

        // Manage All Claims 
        public async Task<IActionResult> ManageClaims()
        {
            var claims = await _context.Claims
                .Include(c => c.Employee)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            return View(claims);
        }

        // Approve Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id, decimal approvedAmount)
        {
            var claim = await _context.Claims.Include(c => c.Employee).FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = $"Claim #{id} not found.";
                return RedirectToAction("ManageClaims");
            }

            if (approvedAmount <= 0 || approvedAmount > claim.Amount)
            {
                TempData["ErrorMessage"] = "Invalid approved amount.";
                return RedirectToAction("ManageClaims");
            }

            claim.Status = "Approved";
            claim.ApprovedAmount = approvedAmount;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            // Audit
            await _auditService.LogAsync("Approved", "Claims", claim.ClaimId, claim.EmployeeId, User);

            if (!string.IsNullOrEmpty(claim.EmployeeId))
                await _notificationService.AddNotificationAsync(claim.EmployeeId, $"Your claim #{claim.ClaimId} has been approved by Admin.");

            TempData["SuccessMessage"] = $"Claim #{id} approved.";
            return RedirectToAction("ManageClaims");
        }

        // Reject Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id)
        {
            var claim = await _context.Claims.Include(c => c.Employee).FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = $"Claim #{id} not found.";
                return RedirectToAction("ManageClaims");
            }

            claim.Status = "Rejected";
            claim.ApprovedAmount = 0;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Rejected", "Claims", claim.ClaimId, claim.EmployeeId, User);

            if (!string.IsNullOrEmpty(claim.EmployeeId))
                await _notification_service_safe_notify(claim.EmployeeId, $"Your claim #{claim.ClaimId} has been rejected by Admin.");

            TempData["ErrorMessage"] = $"Claim #{id} rejected.";
            return RedirectToAction("ManageClaims");
        }

        private async Task _notification_service_safe_notify(string userId, string message)
        {
            try
            {
                await _notificationService.AddNotificationAsync(userId, message);
            }
            catch
            {
                
            }
        }

        // Reports

        public async Task<IActionResult> Reports(string status, string employeeEmail, DateTime? startDate, DateTime? endDate, string? export)
        {
            var query = _context.Claims.Include(c => c.Employee).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (!string.IsNullOrEmpty(employeeEmail))
                query = query.Where(c => c.Employee.Email.Contains(employeeEmail));

            if (startDate.HasValue)
                query = query.Where(c => c.SubmittedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.SubmittedDate <= endDate.Value);

            var claims = await query.OrderByDescending(c => c.SubmittedDate).ToListAsync();

            ViewBag.TotalClaims = claims.Count;
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");
            ViewBag.TotalAmount = claims.Sum(c => c.Amount);
            ViewBag.TotalApproved = claims.Sum(c => c.ApprovedAmount ?? 0m);

            // Export to CSV if requested
            if (!string.IsNullOrEmpty(export) && export.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                return ExportClaimsToCsv(claims);
            }

            return View(claims);
        }

        private FileContentResult ExportClaimsToCsv(List<Claim> claims)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ClaimId,EmployeeNumber,EmployeeName,EmployeeEmail,Details,Amount,ApprovedAmount,Status,SubmittedDate,BillFilePath");

            foreach (var c in claims)
            {
                var empNumber = c.Employee != null
                    ? $"EMP{c.Employee.EmployeeNumber:000}"
                    : "EMP000";

                var empName = c.Employee?.FullName?.Replace(",", " ") ?? "";
                var empEmail = c.Employee?.Email ?? "";
                var submitted = c.SubmittedDate.ToString("yyyy-MM-dd HH:mm:ss");
                var bill = c.BillFilePath?.Replace(",", " ") ?? "";

                sb.AppendLine($"{c.ClaimId},{empNumber},{EscapeCsv(empName)},{EscapeCsv(empEmail)},{EscapeCsv(c.Details)},{c.Amount},{c.ApprovedAmount ?? 0m},{c.Status},{submitted},{EscapeCsv(bill)}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"claims_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        private static string EscapeCsv(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return $"\"{s.Replace("\"", "\"\"")}\"";
        }


        //Audit Logs 
        public async Task<IActionResult> AuditLogs(string search, int page = 1, int pageSize = 10)
        {
            var query = _context.AuditTrails
                .Include(a => a.Employee)
                .AsQueryable();

            //  Search filter 
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.ActionBy.Contains(search) ||
                    a.ActionType.Contains(search) ||
                    a.TableAffected.Contains(search) ||
                    (a.Employee != null && a.Employee.Email.Contains(search)) ||
                    a.EmployeeId.Contains(search) 
                );
            }

            // Pagination
            var totalRecords = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.Search = search;

            return View(logs);
        }



        // Download uploaded file
        public async Task<IActionResult> DownloadFile(int id)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null || string.IsNullOrEmpty(claim.BillFilePath))
                return NotFound();

            var fileName = Path.GetFileName(claim.BillFilePath);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, fileName);
        }

    }
}
