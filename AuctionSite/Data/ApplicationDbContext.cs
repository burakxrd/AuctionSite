using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AuctionSite.Models;

namespace AuctionSite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AuctionItem> AuctionItems { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
                 .Property(u => u.VirtualBalance)
                 .HasColumnType("decimal(18, 2)");

            builder.Entity<AuctionItem>()
                .Property(a => a.StartingPrice)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<AuctionItem>()
                .Property(a => a.CurrentBid)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<AuctionItem>()
                .HasOne(a => a.Seller)
                .WithMany()
                .HasForeignKey(a => a.SellerId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<AuctionItem>()
                .HasOne(a => a.Winner)
                .WithMany()
                .HasForeignKey(a => a.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Bid>()
                .HasOne(b => b.Bidder)
                .WithMany()
                .HasForeignKey(b => b.BidderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Bid>()
                .HasOne(b => b.AuctionItem)
                .WithMany(ai => ai.Bids)
                .HasForeignKey(b => b.AuctionItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}