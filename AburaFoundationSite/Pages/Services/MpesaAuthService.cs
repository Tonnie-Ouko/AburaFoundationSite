using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class MpesaAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public MpesaAuthService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var consumerKey = _config["Mpesa:ConsumerKey"];
        var consumerSecret = _config["Mpesa:ConsumerSecret"];
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        var response = await _httpClient.GetAsync("https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials");

        response.EnsureSuccessStatusCode(); // ❗ Good practice

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("access_token").GetString();
    }

    public async Task<string> StkPushAsync(string phoneNumber, decimal amount)
    {
        var token = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var shortcode = _config["Mpesa:ShortCode"];
        var passkey = _config["Mpesa:PassKey"];
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var password = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shortcode}{passkey}{timestamp}"));

        var payload = new
        {
            BusinessShortCode = shortcode,
            Password = password,
            Timestamp = timestamp,
            TransactionType = "CustomerPayBillOnline",
            Amount = amount,
            PartyA = phoneNumber,
            PartyB = shortcode,
            PhoneNumber = phoneNumber,
            CallBackURL = _config["Mpesa:CallbackUrl"],
            AccountReference = "AburaDonation",
            TransactionDesc = "Donation"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var res = await _httpClient.PostAsync(
            "https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest",
            content
        );

        res.EnsureSuccessStatusCode(); // ❗ Optional: ensure it doesn’t silently fail

        return await res.Content.ReadAsStringAsync();
    }
}
