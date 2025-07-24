namespace AburaFoundationSite.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string PhotoUrl { get; set; } // e.g., "/images/team/jane.jpg"
    }

}
