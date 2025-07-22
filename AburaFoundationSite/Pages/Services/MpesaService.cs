using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using System.Threading.Tasks;

public class MpesaService
{
    public async Task<bool> SendSTKPush(string phoneNumber, decimal amount)
    {
        // ✅ Log / simulate the push request (for now)
        Console.WriteLine($"[MpesaService] STK Push requested to: {phoneNumber}, Amount: {amount}");

        // 🔄 TODO: Integrate actual M-Pesa Daraja API logic here
        await Task.Delay(500); // simulate delay

        // ✅ Return simulated success
        return true;
    }
}


namespace AburaFoundationSite.Services
{
    public class MpesaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _consumerKey = "YOUR_CONSUMER_KEY";
        private readonly string _consumerSecret = "YOUR_CONSUMER_SECRET";
        private readonly string _shortCode = "YOUR_SHORTCODE"; // 174379 or business shortcode
        private readonly string _passkey = "YOUR_PASSKEY";
        private readonly string _callbackUrl = "https://yourdomain.com/api/mpesa/callback";

        public MpesaService()
        {
            _httpClient = new HttpClient();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var byteArray = Encoding.ASCII.GetBytes($"{_consumerKey}:{_consumerSecret}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await _httpClient.GetAsync("https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials");
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(result);
            return json.GetProperty("access_token").GetString();
        }

        public async Task<bool> SendStkPushAsync(string phoneNumber, decimal amount)
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(_shortCode + _passkey + timestamp));

            var payload = new
            {
                BusinessShortCode = _shortCode,
                Password = password,
                Timestamp = timestamp,
                TransactionType = "CustomerPayBillOnline",
                Amount = amount,
                PartyA = FormatPhone(phoneNumber),
                PartyB = _shortCode,
                PhoneNumber = FormatPhone(phoneNumber),
                CallBackURL = _callbackUrl,
                AccountReference = "AburaDonation",
                TransactionDesc = "Abura Foundation Donation"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest", content);
            var result = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode;
        }

        private string FormatPhone(string phone)
        {
            if (phone.StartsWith("0")) return "254" + phone.Substring(1);
            if (phone.StartsWith("+")) return phone.Substring(1);
            return phone;
        }
    }
}
