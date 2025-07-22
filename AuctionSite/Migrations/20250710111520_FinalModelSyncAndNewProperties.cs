using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionSite.Migrations
{
    /// <inheritdoc />
    public partial class FinalModelSyncAndNewProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SellerId",
                table: "AuctionItems",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CurrentBidderId",
                table: "AuctionItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastBidTime",
                table: "AuctionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "AuctionItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_AuctionItems_CurrentBidderId",
                table: "AuctionItems",
                column: "CurrentBidderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionItems_AspNetUsers_CurrentBidderId",
                table: "AuctionItems",
                column: "CurrentBidderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionItems_AspNetUsers_CurrentBidderId",
                table: "AuctionItems");

            migrationBuilder.DropIndex(
                name: "IX_AuctionItems_CurrentBidderId",
                table: "AuctionItems");

            migrationBuilder.DropColumn(
                name: "CurrentBidderId",
                table: "AuctionItems");

            migrationBuilder.DropColumn(
                name: "LastBidTime",
                table: "AuctionItems");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "AuctionItems");

            migrationBuilder.AlterColumn<string>(
                name: "SellerId",
                table: "AuctionItems",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
