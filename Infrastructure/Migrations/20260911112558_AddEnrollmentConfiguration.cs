using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_GPA_Range",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SpecializationCourses_RequirementType_Valid",
                table: "SpecializationCourses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedSemester_Range",
                table: "SpecializationCourses");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_Status_Valid",
                table: "Enrollments");

            migrationBuilder.AlterColumn<decimal>(
                name: "GPA",
                table: "Students",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)",
                oldPrecision: 3,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_GPA_Range",
                table: "Students",
                sql: "\"GPA\" BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedSemester_Range",
                table: "SpecializationCourses",
                sql: "\"SuggestedSemester\" BETWEEN 1 AND 3");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_IsActive",
                table: "Semesters",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments",
                columns: new[] { "StudentId", "SectionId" },
                unique: true,
                filter: "\"Status\" <> 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_Status_Valid",
                table: "Enrollments",
                sql: "\"Status\" BETWEEN 1 AND 6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_GPA_Range",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedSemester_Range",
                table: "SpecializationCourses");

            migrationBuilder.DropIndex(
                name: "IX_Semesters_IsActive",
                table: "Semesters");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_Status_Valid",
                table: "Enrollments");

            migrationBuilder.AlterColumn<decimal>(
                name: "GPA",
                table: "Students",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_GPA_Range",
                table: "Students",
                sql: "\"GPA\" BETWEEN 0 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SpecializationCourses_RequirementType_Valid",
                table: "SpecializationCourses",
                sql: "\"RequirementType\" BETWEEN 1 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedSemester_Range",
                table: "SpecializationCourses",
                sql: "\"SuggestedSemester\" BETWEEN 1 AND 2");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments",
                columns: new[] { "StudentId", "SectionId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_Status_Valid",
                table: "Enrollments",
                sql: "\"Status\" BETWEEN 1 AND 5");
        }
    }
}
