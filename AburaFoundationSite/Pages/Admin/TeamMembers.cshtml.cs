using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AburaFoundationSite.Models;
using AburaFoundationSite.Data;
using Microsoft.AspNetCore.Authorization;

namespace AburaFoundationSite.Pages.Admin;

[Authorize]
public class AdminTeamMembersModel : PageModel
{
    private readonly AppDbContext _context;

    public AdminTeamMembersModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public TeamMember TeamMember { get; set; } = new();

    public List<TeamMember> Members { get; set; } = [];

    public bool IsEditing => EditId.HasValue;
    public bool IsAdding => Request.Query.ContainsKey("add");

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    public async Task OnGetAsync()
    {
        Members = _context.TeamMembers.OrderBy(t => t.FullName).ToList();

        if (EditId.HasValue)
        {
            TeamMember = await _context.TeamMembers.FindAsync(EditId);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Members = _context.TeamMembers.ToList();
            return Page();
        }

        if (TeamMember.Id == 0)
        {
            _context.TeamMembers.Add(TeamMember);
        }
        else
        {
            var existing = await _context.TeamMembers.FindAsync(TeamMember.Id);
            if (existing != null)
            {
                existing.FullName = TeamMember.FullName;
                existing.Role = TeamMember.Role;
                existing.PhotoUrl = TeamMember.PhotoUrl;
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var member = await _context.TeamMembers.FindAsync(id);
        if (member != null)
        {
            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
