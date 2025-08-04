using Microsoft.Extensions.Logging; // ✅ Add this at the top
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AburaFoundationSite.Models;


public class MpesaStkPushService
{
    private readonly MpesaAuthService _authService;
    private readonly MpesaSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<MpesaStkPushService> _logger; // ✅ Add this

    public MpesaStkPushService(
        MpesaAuthService authService,
        HttpClient httpClient,
        IOptions<MpesaSettings> options,
        IConfiguration config,
        ILogger<MpesaStkPushService> logger) // ✅ Add logger to constructor
    {
        _authService = authService;
        _httpClient = httpClient;
        _settings = options.Value;
        _config = config;
        _logger = logger;
    }

    public async Task<string> InitiateStkPushAsync(string phoneNumber, decimal amount, string accountReference = "Abura Donation", string transactionDesc = "Charity Donation")
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
                BusinessShortCode = _settings.ShortCode,
                Password = password,
                Timestamp = timestamp,
                TransactionType = "CustomerPayBillOnline",
                Amount = amount,
                PartyA = phoneNumber,
                PartyB = _settings.ShortCode,
                PhoneNumber = phoneNumber,
                CallBackURL = callbackUrl,
                AccountReference = accountReference,
                TransactionDesc = transactionDesc
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ✅ Determine the URL based on environment
            var baseUrl = _config["Mpesa:Environment"] == "Production"
                ? "https://api.safaricom.co.ke"
                : "https://sandbox.safaricom.co.ke";

            var response = await _httpClient.PostAsync($"{baseUrl}/mpesa/stkpush/v1/processrequest", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ M-Pesa STK push failed: {Response}", responseString);
            }
            else
            {
                _logger.LogInformation("✅ STK Push Initiated Successfully: {Response}", responseString);
            }

            return responseString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in STK push");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
