using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class MpesaStkPushService
{
    private readonly MpesaAuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public MpesaStkPushService(MpesaAuthService authService, HttpClient httpClient, IConfiguration config)
    {
        _authService = authService;
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> PushAsync(string phoneNumber, decimal amount)
    {
        try
        {
            var accessToken = await _authService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var shortCode = _config["Mpesa:ShortCode"];
            var passKey = _config["Mpesa:PassKey"];
            var callbackUrl = _config["Mpesa:CallbackUrl"];

            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shortCode}{passKey}{timestamp}"));

            var payload = new
            {
                BusinessShortCode = shortCode,
                Password = password,
                Timestamp = timestamp,
                TransactionType = "CustomerPayBillOnline",
                Amount = amount,
                PartyA = phoneNumber,
                PartyB = shortCode,
                PhoneNumber = phoneNumber,
                CallBackURL = callbackUrl,
                AccountReference = "AburaDonation",
                TransactionDesc = "Donation Payment"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Optional: Log the failed response
                Console.WriteLine("❌ M-Pesa STK push failed: " + responseString);
            }

            return responseString;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error in STK push: " + ex.Message);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
