using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionSite.Migrations
{
    /// <inheritdoc />
    public partial class AddMinimumBidIncrementToAuctionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinimumBidIncrement",
                table: "AuctionItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumBidIncrement",
                table: "AuctionItems");
        }
    }
}
