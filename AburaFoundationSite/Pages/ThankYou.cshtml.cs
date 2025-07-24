using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AburaFoundationSite.Data;
using AburaFoundationSite.Models;
using System.Threading.Tasks;

namespace AburaFoundationSite.Pages
{
    public class ThankYouModel : PageModel
    {
        private readonly AppDbContext _context;

        public ThankYouModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Donation Donation { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Donation = await _context.Donations.FindAsync(id);

            if (Donation == null)
                return NotFound();

            return Page();
        }
    }
}
