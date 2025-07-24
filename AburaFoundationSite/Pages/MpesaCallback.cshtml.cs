using AburaFoundationSite.Data;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using AburaFoundationSite.Models;


namespace AburaFoundationSite.Pages
{

    [IgnoreAntiforgeryToken]
    public class MpesaCallbackModel : PageModel
    {
        private readonly AppDbContext _context;

        public MpesaCallbackModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            var doc = JsonDocument.Parse(json);
            var callback = doc.RootElement.GetProperty("Body").GetProperty("stkCallback");

            string checkoutRequestId = callback.GetProperty("CheckoutRequestID").GetString();
            int resultCode = callback.GetProperty("ResultCode").GetInt32();

            var donation = _context.Donations.FirstOrDefault(d => d.Reference == checkoutRequestId);
            if (donation != null)
            {
                donation.Status = resultCode == 0 ? "Success" : "Failed";
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { message = "Callback received" });
        }
    }
}