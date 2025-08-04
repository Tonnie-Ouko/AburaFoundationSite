using AburaFoundationSite.Data;
using AburaFoundationSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AburaFoundationSite.Pages
{
    public class DonationModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly MpesaStkPushService _mpesaService;

        public DonationModel(AppDbContext context, IConfiguration config, MpesaStkPushService mpesaService)
        {
            _context = context;
            _config = config;
            _mpesaService = mpesaService;
        }

        [BindProperty]
        public Donation Donation { get; set; } = new Donation();

        [BindProperty]
        [Required(ErrorMessage = "Amount is required")]
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

            if (Donation.PaymentMethod == "MPesa")
            {
                var result = await _mpesaService.InitiateStkPushAsync(PhoneNumber, Donation.Amount);
                Console.WriteLine("📲 STK Push Response: " + result);
                TempData["Message"] = "STK Push Sent!";
                return Page();
            }


            var donation = new Donation
            {
                Amount = Amount,
                PhoneNumber = PhoneNumber,
                PaymentMethod = PaymentMethod,
                Date = DateTime.UtcNow
            };

            // ✅ M-Pesa Flow
            if (PaymentMethod == "Mpesa")
            {
                try
                {
                    var result = await _mpesaService.InitiateStkPushAsync(PhoneNumber, Amount);
                    var json = JsonDocument.Parse(result);

                    donation.Status = "Pending";
                    donation.Reference = json.RootElement.GetProperty("CheckoutRequestID").GetString();

                    _context.Donations.Add(donation);
                    await _context.SaveChangesAsync();

                    return RedirectToPage("ThankYou");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Failed to initiate M-Pesa transaction: " + ex.Message);
                    return Page();
                }
            }

            // ✅ PayPal & Card Flow
            else if (PaymentMethod == "PayPal" || PaymentMethod == "Card")
            {
                // These are processed on the frontend. Backend just records on submission.
                donation.Status = "Success";

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                return RedirectToPage("Receipt", new { id = donation.Id });
            }

            // ❌ Unsupported Payment
            donation.Status = "Failed";
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            return RedirectToPage("Error");
        }
    }
}
