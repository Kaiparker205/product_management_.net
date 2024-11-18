using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTestApp.Migrations
{
    /// <inheritdoc />
    public partial class updOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateLivraison",
                table: "ProductOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ProductOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateLivraison",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProductOrders");
        }
    }
}
