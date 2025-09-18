using InSureX.Data;
using InSureX.Models;
using InSureX.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InSureX.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public EmployeeController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // Employee Dashboard
        public async Task<IActionResult> EmployeeDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var claims = await _context.Claims
                .Where(c => c.EmployeeId == userId)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            var dashboardModel = new EmployeeDashboardViewModel
            {
                Claims = claims,
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Status == "Pending"),
                ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                RejectedClaims = claims.Count(c => c.Status == "Rejected")
            };

            // Log dashboard view
            await _auditService.LogAsync("Viewed Dashboard", "Claims", null, userId, User);

            return View(dashboardModel);
        }

        // GET: Submit new claim
        [HttpGet]
        public IActionResult CreateClaim()
        {
            return View();
        }

        // POST: Submit new claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClaim(Models.Claim claim, IFormFile? BillFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                claim.EmployeeId = userId;
                claim.Status = "Pending";
                claim.SubmittedDate = DateTime.UtcNow;

                // Handle bill upload
                if (BillFile != null && BillFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    // Create the folder if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique filename to avoid overwriting
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(BillFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await BillFile.CopyToAsync(stream);
                    }

                    // Save relative path in DB 
                    claim.BillFilePath = "/uploads/" + uniqueFileName;
                }

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                // Audit log
                await _auditService.LogAsync("Created", "Claims", claim.ClaimId, userId, User);

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(EmployeeDashboard));
            }

            return View(claim);
        }

    }
}
