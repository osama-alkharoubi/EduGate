using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnrollmentSchemaAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments");

            migrationBuilder.AddColumn<short>(
                name: "Status",
                table: "Sections",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sections_Status",
                table: "Sections",
                sql: "\"Status\" IN (1, 2, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments",
                columns: new[] { "StudentId", "SectionId" },
                unique: true,
                filter: "\"Status\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Sections_Status",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Sections");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments",
                columns: new[] { "StudentId", "SectionId" },
                unique: true,
                filter: "\"Status\" <> 2");
        }
    }
}
