using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AuctionSite.Resources;
using System.Globalization;

namespace AuctionSite.Models
{
    public class AuctionItem
    {
        public AuctionItem()
        {
            Bids = new List<Bid>();
        }

        public int Id { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.ProductNameRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(100, ErrorMessageResourceName = nameof(SharedResources.AuctionItem_NameLength), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.ProductName))]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessageResourceName = nameof(SharedResources.AuctionItem_DescriptionLength), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.Description))]
        public string? Description { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.StartingPriceRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [Range(0.01, double.MaxValue, ErrorMessageResourceName = nameof(SharedResources.StartingPriceRange), ErrorMessageResourceType = typeof(SharedResources))]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = nameof(SharedResources.StartingPrice))]
        public decimal StartingPrice { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.MinimumBidIncrementRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [Range(0.01, double.MaxValue, ErrorMessageResourceName = nameof(SharedResources.MinimumBidIncrementRange), ErrorMessageResourceType = typeof(SharedResources))]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = nameof(SharedResources.MinimumBidIncrement))]
        public decimal MinimumBidIncrement { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CurrentBid { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.StartTimeRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.StartTime))]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.EndTimeRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.EndTime))]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsSold { get; set; } = false;

        public string? SellerId { get; set; }
        [ForeignKey("SellerId")]
        public ApplicationUser? Seller { get; set; }

        public string? CurrentBidderId { get; set; }
        [ForeignKey("CurrentBidderId")]
        public ApplicationUser? CurrentBidder { get; set; }

        public DateTime? LastBidTime { get; set; }

        public string? WinnerId { get; set; }
        [ForeignKey("WinnerId")]
        public ApplicationUser? Winner { get; set; }

        public ICollection<Bid> Bids { get; set; }

        [NotMapped]
        public string? EndTimeText
        {
            get
            {
                TimeSpan remaining;
                string prefix;
                var currentCulture = CultureInfo.CurrentUICulture;

                if (StartTime > DateTime.Now)
                {
                    remaining = StartTime - DateTime.Now;
                    prefix = SharedResources.ResourceManager.GetString("TimeUntilStart", currentCulture) +
                             SharedResources.ResourceManager.GetString("ColonSeparator", currentCulture) +
                             SharedResources.ResourceManager.GetString("Space", currentCulture);
                }
                else if (EndTime <= DateTime.Now)
                {
                    return SharedResources.ResourceManager.GetString("AuctionExpiredMessage", currentCulture);
                }
                else
                {
                    remaining = EndTime - DateTime.Now;
                    prefix = SharedResources.ResourceManager.GetString("TimeRemaining", currentCulture) +
                             SharedResources.ResourceManager.GetString("ColonSeparator", currentCulture) +
                             SharedResources.ResourceManager.GetString("Space", currentCulture);
                }

                string formattedTime = "";
                if (remaining.TotalDays >= 1)
                {
                    formattedTime = $"{remaining.Days}{SharedResources.ResourceManager.GetString("DaysShort", currentCulture)}{SharedResources.ResourceManager.GetString("Space", currentCulture)}{remaining.Hours}{SharedResources.ResourceManager.GetString("HoursShort", currentCulture)}";
                }
                else if (remaining.TotalHours >= 1)
                {
                    formattedTime = $"{remaining.Hours}{SharedResources.ResourceManager.GetString("HoursShort", currentCulture)}{SharedResources.ResourceManager.GetString("Space", currentCulture)}{remaining.Minutes}{SharedResources.ResourceManager.GetString("MinutesShort", currentCulture)}";
                }
                else if (remaining.TotalMinutes >= 1)
                {
                    formattedTime = $"{remaining.Minutes}{SharedResources.ResourceManager.GetString("MinutesShort", currentCulture)}{SharedResources.ResourceManager.GetString("Space", currentCulture)}{remaining.Seconds}{SharedResources.ResourceManager.GetString("SecondsShort", currentCulture)}";
                }
                else if (remaining.TotalSeconds > 0)
                {
                    formattedTime = $"{remaining.Seconds}{SharedResources.ResourceManager.GetString("SecondsShort", currentCulture)}";
                }
                else
                {
                    return SharedResources.ResourceManager.GetString("TimeExpiredMessage", currentCulture);
                }
                return prefix + formattedTime;
            }
        }
    }
}
