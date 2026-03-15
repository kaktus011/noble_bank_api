using Microsoft.AspNetCore.Identity;

namespace NobleBank.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {

        }

        public IEnumerable<Card> Cards { get; set; }

        public IEnumerable<Loan> Loans { get; set; }

        public IEnumerable<Post> Posts { get; set; }
    }
}