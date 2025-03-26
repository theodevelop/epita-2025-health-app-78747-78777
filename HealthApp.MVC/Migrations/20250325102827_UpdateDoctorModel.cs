using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthApp.MVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Doctors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_IdentityUserId",
                table: "Doctors",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_AspNetUsers_IdentityUserId",
                table: "Doctors",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_AspNetUsers_IdentityUserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_IdentityUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Doctors");
        }
    }
}
