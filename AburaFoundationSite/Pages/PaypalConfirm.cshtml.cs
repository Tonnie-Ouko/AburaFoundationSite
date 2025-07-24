using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AburaFoundationSite.Data;
using AburaFoundationSite.Models;
using System;
using System.Threading.Tasks;

namespace AburaFoundationSite.Pages
{
    public class PaypalConfirmModel : PageModel
    {
        private readonly AppDbContext _context;

        public PaypalConfirmModel(AppDbContext context)
        {
            _context = context;
        }

        public class PaypalDonation
        {
            public string OrderId { get; set; }
            public string PayerId { get; set; }
            public decimal Amount { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync([FromBody] PaypalDonation donation)
        {
            var newDonation = new Donation
            {
                Amount = donation.Amount,
                PaymentMethod = "PayPal",
                Status = "Completed",
                Date = DateTime.Now,
                Name = donation.Name,
                Email = donation.Email
            };

            _context.Donations.Add(newDonation);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}
