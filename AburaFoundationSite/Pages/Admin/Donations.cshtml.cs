using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AburaFoundationSite.Data;
using AburaFoundationSite.Models;
using System.Globalization;
using System.Text;

namespace AburaFoundationSite.Pages.Admin
{
    public class DonationsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DonationsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Donation> PagedDonations { get; set; } = new();
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Export { get; set; }

        public async Task<IActionResult> OnGetAsync(int page = 1)
        {
            Page = page;
            var query = _context.Donations.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(d =>
                    d.PhoneNumber.Contains(SearchTerm) ||
                    d.Reference.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(Status))
            {
                query = query.Where(d => d.Status == Status);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(d => d.Date >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(d => d.Date <= EndDate.Value);
            }

            query = query.OrderByDescending(d => d.Date);

            if (Export)
            {
                var allDonations = await query.ToListAsync();
                var csv = GenerateCsv(allDonations);
                var bytes = Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", "donations.csv");
            }

            int pageSize = 10;
            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            PagedDonations = await query
                .Skip((Page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Page();
        }

        private string GenerateCsv(List<Donation> donations)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Date,Amount,Phone,PaymentMethod,Status,Reference");

            foreach (var d in donations)
            {
                sb.AppendLine($"{d.Date.ToString("yyyy-MM-dd HH:mm")},{d.Amount},{d.PhoneNumber},{d.PaymentMethod},{d.Status},{d.Reference}");
            }

            return sb.ToString();
        }
    }
}
