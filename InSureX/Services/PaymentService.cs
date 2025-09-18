using InSureX.Data;
using InSureX.Models;
using Microsoft.EntityFrameworkCore;

namespace InSureX.Services
{
    public class PaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create a payment for an approved claim
        public async Task<Payment> CreatePaymentAsync(int claimId, decimal amount)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null || claim.Status != "Approved")
                throw new Exception("Invalid claim or claim not approved.");

            var payment = new Payment
            {
                ClaimId = claimId,
                AmountPaid = amount,          
                PaymentDate = DateTime.Now,
                Status = "Pending"            
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        // Mark payment as completed
        public async Task<bool> MarkAsCompletedAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = "Completed";
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return true;
        }

        // Get all payments (Admin view)
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Claim)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}
