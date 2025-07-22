using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionSite.Models
{
    public class Bid
    {
        public int Id { get; set; }

        [Required]
        public int AuctionItemId { get; set; }
        [ForeignKey("AuctionItemId")]
        public AuctionItem? AuctionItem { get; set; }

        [Required]
        public string? BidderId { get; set; }
        [ForeignKey("BidderId")]
        public ApplicationUser? Bidder { get; set; }

        [Required(ErrorMessage = "BidAmountRequired")]
        [Range(0.01, double.MaxValue, ErrorMessage = "BidAmountRange")] 
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime BidTime { get; set; }
    }
}
