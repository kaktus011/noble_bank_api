using Microsoft.AspNetCore.Identity;

namespace NobleBank.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}".Trim();

        public ICollection<Card> Cards { get; set; } = [];

        public ICollection<Loan> Loans { get; set; } = [];

        public ICollection<Post> Posts { get; set; } = [];
    }
}