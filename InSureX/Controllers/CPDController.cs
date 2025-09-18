
using InSureX.Data;
using InSureX.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Claim = InSureX.Models.Claim;
using System.Linq;

namespace InSureX.Controllers
{
    [Authorize(Roles = "CPD")]
    public class CPDController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;
        private readonly NotificationService _notificationService;

        public CPDController(ApplicationDbContext context, AuditService auditService, NotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public IActionResult Index() => RedirectToAction("CPDDashboard");

        // Dashboard
        public async Task<IActionResult> CPDDashboard()
        {
            ViewBag.TotalClaims = await _context.Claims.CountAsync();
            ViewBag.PendingClaims = await _context.Claims.CountAsync(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = await _context.Claims.CountAsync(c => c.Status == "Approved");
            ViewBag.RejectedClaims = await _context.Claims.CountAsync(c => c.Status == "Rejected");
            return View();
        }

        // Pending Claims
        public async Task<IActionResult> ManageClaims()
        {
            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Where(c => c.Status == "Pending")
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            return View(claims);
        }

        // Processed Claims
        public async Task<IActionResult> ProcessedClaims()
        {
            var claims = await _context.Claims
                .Include(c => c.Employee)
                .Where(c => c.Status != "Pending")
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
            if (claim == null) return RedirectToAction("ManageClaims");

            claim.Status = "Approved";
            claim.ApprovedAmount = approvedAmount;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Approved", "Claims", claim.ClaimId, claim.EmployeeId, User);

            if (claim.Employee != null)
                await _notificationService.AddNotificationAsync(claim.Employee.Id, $"Claim #{claim.ClaimId} approved by CPD.");

            return RedirectToAction("ProcessedClaims");
        }

        // Reject Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id)
        {
            var claim = await _context.Claims.Include(c => c.Employee).FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null) return RedirectToAction("ManageClaims");

            claim.Status = "Rejected";
            claim.ApprovedAmount = 0;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Rejected", "Claims", claim.ClaimId, claim.EmployeeId, User);

            if (claim.Employee != null)
                await _notificationService.AddNotificationAsync(claim.Employee.Id, $"Claim #{claim.ClaimId} rejected by CPD.");

            return RedirectToAction("ProcessedClaims");
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

            return PhysicalFile(filePath, "application/octet-stream", fileName);
        }
    }
}
