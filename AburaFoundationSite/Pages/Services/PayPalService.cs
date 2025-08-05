using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AburaFoundationSite.Services
{
    public class PayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public PayPalService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = _config["PayPal:ClientId"];
            var secret = _config["PayPal:ClientSecret"];
            var env = _config["PayPal:Environment"];
            var baseUrl = env == "live"
                ? "https://api.paypal.com"
                : "https://api.sandbox.paypal.com";

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authString);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<string> CaptureOrderAsync(string orderId)
        {
            var token = await GetAccessTokenAsync();
            var env = _config["PayPal:Environment"];
            var baseUrl = env == "live"
                ? "https://api.paypal.com"
                : "https://api.sandbox.paypal.com";

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{orderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        // ✅ ADDING CreateOrderAsync for initiating a payment
        public async Task<string> CreateOrderAsync(decimal amount)
        {
            var token = await GetAccessTokenAsync();
            var env = _config["PayPal:Environment"];
            var baseUrl = env == "live"
                ? "https://api.paypal.com"
                : "https://api.sandbox.paypal.com";

            var currency = _config["PayPal:Currency"] ?? "USD"; // Optional: fallback to USD
            var returnUrl = _config["PayPal:ReturnUrl"] ?? "https://home.abura.org/PayPalCapture";
            var cancelUrl = _config["PayPal:CancelUrl"] ?? "https://home.abura.org/Donation";

            var payload = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2")
                        }
                    }
                },
                application_context = new
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
