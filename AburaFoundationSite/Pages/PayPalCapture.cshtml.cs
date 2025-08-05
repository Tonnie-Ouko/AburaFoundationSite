using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AburaFoundationSite.Services;
using System.Threading.Tasks;

namespace AburaFoundationSite.Pages
{
    public class PayPalCaptureModel : PageModel
    {
        private readonly PayPalService _paypalService;

        public PayPalCaptureModel(PayPalService paypalService)
        {
            _paypalService = paypalService;
        }

        public async Task<IActionResult> OnGetAsync(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest("Missing order ID.");
            }

            var result = await _paypalService.CaptureOrderAsync(orderId);

            // TODO: Save to Donations DB if needed (based on result)
            return new JsonResult(result);
        }
    }
}