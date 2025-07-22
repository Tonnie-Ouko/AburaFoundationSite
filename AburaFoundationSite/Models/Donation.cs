namespace AburaFoundationSite.Models
{
    public class Donation
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        // Optional fields for additional tracking
        public string? TransactionId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
