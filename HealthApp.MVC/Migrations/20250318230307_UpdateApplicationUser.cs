using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthApp.MVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "NotificationId",
                table: "Notifications",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Notifications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleType",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Specialization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSpecialization",
                columns: table => new
                {
                    DoctorsId = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecializationsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSpecialization", x => new { x.DoctorsId, x.SpecializationsId });
                    table.ForeignKey(
                        name: "FK_DoctorSpecialization_Doctors_DoctorsId",
                        column: x => x.DoctorsId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorSpecialization_Specialization_SpecializationsId",
                        column: x => x.SpecializationsId,
                        principalTable: "Specialization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_IdentityUserId",
                table: "Patients",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSpecialization_SpecializationsId",
                table: "DoctorSpecialization",
                column: "SpecializationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_AspNetUsers_IdentityUserId",
                table: "Patients",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_AspNetUsers_IdentityUserId",
                table: "Patients");

            migrationBuilder.DropTable(
                name: "DoctorSpecialization");

            migrationBuilder.DropTable(
                name: "Specialization");

            migrationBuilder.DropIndex(
                name: "IX_Patients_IdentityUserId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleType",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Notifications",
                newName: "NotificationId");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Doctors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
