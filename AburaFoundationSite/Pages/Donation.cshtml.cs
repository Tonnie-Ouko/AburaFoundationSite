using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using AburaFoundationSite.Data; // Your actual namespace
using AburaFoundationSite.Models; // Assuming Donation model is here
using System.Threading.Tasks;
using System;

namespace AburaFoundationSite.Pages
{
    public class DonationModel : PageModel
    {
        private readonly AppDbContext _context;

        public DonationModel(AppDbContext context)
        {

            _context = context;
        }

        [BindProperty]
        public Donation Donation { get; set; } = new Donation();

        [Required]
        [Range(1, 1000000, ErrorMessage = "Please enter a valid amount.")]
        public decimal Amount { get; set; }

        [BindProperty]
        public string PhoneNumber { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var donation = new Donation
            {
                Amount = Amount,
                PhoneNumber = PhoneNumber,
                PaymentMethod = PaymentMethod,
                Date = DateTime.UtcNow
            };

            // ========== Handle M-Pesa Logic ==========
            if (PaymentMethod == "Mpesa")
            {
                // Later: Trigger STK Push API here
                // For now: simulate M-Pesa success
                donation.Status = "Pending"; // or "Success" if fake simulate
            }

            // ========== Save to DB ==========
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // ========== Redirect to Receipt ==========
            return RedirectToPage("Receipt", new { id = donation.Id });
        }
    }
}
