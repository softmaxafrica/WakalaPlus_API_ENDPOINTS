using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WakalaPlus.Migrations
{
    /// <inheritdoc />
    public partial class CustomerDetailsAddition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Customers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nida",
                table: "Customers",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegDate",
                table: "Customers",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Nida",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RegDate",
                table: "Customers");
        }
    }
}
