using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WakalaPlus.Migrations
{
    /// <inheritdoc />
    public partial class customerRegDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "Customers",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "Customers",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "Customers");
        }
    }
}
