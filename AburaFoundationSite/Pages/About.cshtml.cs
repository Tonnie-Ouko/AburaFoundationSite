using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AburaFoundationSite.Data;
using AburaFoundationSite.Models;

namespace AburaFoundationSite.Pages
{
    public class AboutModel : PageModel
    {
        private readonly AppDbContext _context;

        public AboutModel(AppDbContext context)
        {
            _context = context;
        }

        public List<TeamMember> Team { get; set; } = new();

        public async Task OnGetAsync()
        {
            Team = await _context.TeamMembers.ToListAsync();
        }
    }
}
