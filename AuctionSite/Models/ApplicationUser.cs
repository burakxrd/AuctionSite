using Microsoft.AspNetCore.Identity;

namespace AuctionSite.Models
{
    public class ApplicationUser : IdentityUser 
    {
        public string? PendingNewEmail { get; set; }

        public decimal VirtualBalance { get; set; } = 1000m; 

    }
}