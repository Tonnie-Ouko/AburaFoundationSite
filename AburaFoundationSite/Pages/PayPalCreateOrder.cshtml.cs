using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AburaFoundationSite.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace AburaFoundationSite.Pages
{
    public class PayPalCreateOrderModel : PageModel
    {
        private readonly PayPalService _paypalService;

        public PayPalCreateOrderModel(PayPalService paypalService)
        {
            _paypalService = paypalService;
        }

        public class CreateOrderRequest
        {
            public string Amount { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<CreateOrderRequest>(body);

            var amount = decimal.Parse(data.Amount); // 🔧 Convert to decimal
            var orderId = await _paypalService.CreateOrderAsync(amount);

            return new JsonResult(new { id = orderId });
        }
    }
}
