using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthApp.MVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSpecializations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Specializations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Specializations");
        }
    }
}
