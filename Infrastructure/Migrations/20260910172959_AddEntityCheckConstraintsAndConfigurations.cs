using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityCheckConstraintsAndConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Specializations_DepartmentId",
                table: "Specializations");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CollegeId",
                table: "Departments");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<decimal>(
                name: "GPA",
                table: "Students",
                type: "numeric(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Semesters",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRevoked",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<short>(
                name: "Status",
                table: "Enrollments",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "Enrollments",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_AcademicStatus_Valid",
                table: "Students",
                sql: "\"AcademicStatus\" BETWEEN 1 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_CompletedCredits_NonNegative",
                table: "Students",
                sql: "\"CompletedCredits\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Students_GPA_Range",
                table: "Students",
                sql: "\"GPA\" BETWEEN 0 AND 4");

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_DepartmentId_SpecializationName",
                table: "Specializations",
                columns: new[] { "DepartmentId", "SpecializationName" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Specializations_Code_Range",
                table: "Specializations",
                sql: "\"Code\" BETWEEN 1 AND 999");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Specializations_TotalCredits_Range",
                table: "Specializations",
                sql: "\"TotalCredits\" BETWEEN 60 AND 250");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SpecializationCourses_RequirementType_Valid",
                table: "SpecializationCourses",
                sql: "\"RequirementType\" BETWEEN 1 AND 4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedSemester_Range",
                table: "SpecializationCourses",
                sql: "\"SuggestedSemester\" BETWEEN 1 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedYear_Positive",
                table: "SpecializationCourses",
                sql: "\"SuggestedYear\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Semesters_EndDate_After_StartDate",
                table: "Semesters",
                sql: "\"EndDate\" > \"StartDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sections_Capacity_Positive",
                table: "Sections",
                sql: "\"Capacity\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sections_EndTime_After_StartTime",
                table: "Sections",
                sql: "\"EndTime\" > \"StartTime\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Sections_SectionNumber_Positive",
                table: "Sections",
                sql: "\"SectionNumber\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RefreshTokens_ExpiresAfterCreated",
                table: "RefreshTokens",
                sql: "\"ExpiresOnUtc\" > \"CreatedOnUtc\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Professors_AcademicRank_Valid",
                table: "Professors",
                sql: "\"AcademicRank\" BETWEEN 1 AND 4");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments",
                columns: new[] { "StudentId", "SectionId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_Grade_Range",
                table: "Enrollments",
                sql: "\"Grade\" IS NULL OR (\"Grade\" >= 0 AND \"Grade\" <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_Status_Valid",
                table: "Enrollments",
                sql: "\"Status\" BETWEEN 1 AND 5");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CollegeId_DepartmentName",
                table: "Departments",
                columns: new[] { "CollegeId", "DepartmentName" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Courses_CreditHours_Positive",
                table: "Courses",
                sql: "\"CreditHours\" > 0 AND \"CreditHours\" <= 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoursePrerequisites_DifferentCourses",
                table: "CoursePrerequisites",
                sql: "\"CourseId\" <> \"PrerequisiteId\"");

            migrationBuilder.CreateIndex(
                name: "IX_Colleges_CollegeName",
                table: "Colleges",
                column: "CollegeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_AcademicStatus_Valid",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_CompletedCredits_NonNegative",
                table: "Students");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Students_GPA_Range",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Specializations_DepartmentId_SpecializationName",
                table: "Specializations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Specializations_Code_Range",
                table: "Specializations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Specializations_TotalCredits_Range",
                table: "Specializations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SpecializationCourses_RequirementType_Valid",
                table: "SpecializationCourses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedSemester_Range",
                table: "SpecializationCourses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SpecializationCourses_SuggestedYear_Positive",
                table: "SpecializationCourses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Semesters_EndDate_After_StartDate",
                table: "Semesters");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sections_Capacity_Positive",
                table: "Sections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sections_EndTime_After_StartTime",
                table: "Sections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Sections_SectionNumber_Positive",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Roles_RoleName",
                table: "Roles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RefreshTokens_ExpiresAfterCreated",
                table: "RefreshTokens");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Professors_AcademicRank_Valid",
                table: "Professors");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId_SectionId",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_Grade_Range",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_Status_Valid",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CollegeId_DepartmentName",
                table: "Departments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Courses_CreditHours_Positive",
                table: "Courses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoursePrerequisites_DifferentCourses",
                table: "CoursePrerequisites");

            migrationBuilder.DropIndex(
                name: "IX_Colleges_CollegeName",
                table: "Colleges");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GPA",
                table: "Students",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(3,2)",
                oldPrecision: 3,
                oldScale: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Semesters",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Roles",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRevoked",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<short>(
                name: "Status",
                table: "Enrollments",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)1);

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "Enrollments",
                type: "numeric(4,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Courses",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_DepartmentId",
                table: "Specializations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CollegeId",
                table: "Departments",
                column: "CollegeId");
        }
    }
}
