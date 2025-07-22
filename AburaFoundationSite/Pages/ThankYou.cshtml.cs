using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Mail;

namespace AburaFoundationSite.Pages
{
    public class ThankYouModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public decimal Amount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Method { get; set; } = string.Empty;

        public string DisplayMethod => Method.ToLower() switch
        {
            "paypal" => "PayPal",
            "mpesa" => "M-Pesa",
            _ => "Unknown"
        };

        public void OnGet()
        {
            SendConfirmationEmail(Amount, DisplayMethod);
        }

        private void SendConfirmationEmail(decimal amount, string method)
        {
            try
            {
                var fromEmail = "donations@abura.org";
                var toEmail = "recipient@email.com"; // Replace with donor email when captured
                var smtpHost = "smtp.yourmailhost.com";
                var smtpPort = 587;

                var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = "Your Donation Receipt – Abura Foundation",
                    Body = $@"
Thank you for your donation!

Amount: {amount:C}
Method: {method}
Date: {DateTime.Now:dd MMM yyyy, HH:mm}

We appreciate your support!
– Abura Foundation Team",
                    IsBodyHtml = false
                };

                var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential("your-smtp-username", "your-smtp-password"),
                    EnableSsl = true
                };

                client.Send(message);
            }
            catch (Exception ex)
            {
                // Optional: Log email error
            }
        }
    }
}
