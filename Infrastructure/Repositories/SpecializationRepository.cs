using Application.DTOs.Curriculum;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Dapper;
using Domain.Entities;
using Domain.Enums;
using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Persistence.Repositories;

public class SpecializationRepository : ISpecializationRepository
{
    private readonly ApplicationDbContext _context;

    public SpecializationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private DbConnection Connection
    {
        get
        {
            return _context.Database.GetDbConnection();
        }
    }

    // ==========================================
    // 1. EF Core (CRUD & Simple Reads)
    // ==========================================

    public async Task<Specialization?> GetSpecializationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Specializations
            .FirstOrDefaultAsync(s => s.SpecializationId == id, cancellationToken);
    }
    public async Task<bool> ExistsByCodeAsync(int code, CancellationToken cancellationToken = default)
    {
        return await _context.Specializations
            .AnyAsync(s => s.Code == code, cancellationToken);
    }
    public async Task<int?> GetCodeById(Guid id, CancellationToken cancellationToken = default)
    {

        return await _context.Specializations
        .Where(s => s.SpecializationId == id).Select(s => (int?)s.Code).FirstOrDefaultAsync(cancellationToken);
       
    }
    public async Task<bool> ExistsByCodeExcludeIdAsync(int code, Guid excludeId, CancellationToken cancellationToken = default)
    {
        return await _context.Specializations
            .AnyAsync(s => s.Code == code && s.SpecializationId != excludeId, cancellationToken);
    }
    public async Task<IEnumerable<Specialization>> GetAllSpecializationsAsync(Guid? departmentId = null, Guid? collegeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Specializations
            .AsNoTracking()
            .AsQueryable();
        query = query.Where(s => s.IsActive);
        if (departmentId.HasValue)
        {
            query = query.Where(s => s.DepartmentId == departmentId.Value);
        }

        if (collegeId.HasValue)
        {
            query = query.Where(s => s.Department.CollegeId == collegeId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public void AddSpecialization(Specialization specialization)
    {
        _context.Specializations.Add(specialization);
    }

    public void UpdateSpecialization(Specialization specialization)
    {
        _context.Specializations.Update(specialization);
    }

    public void DeleteSpecialization(Specialization specialization)
    {
        _context.Specializations.Remove(specialization);
    }

    // ==========================================
    // 2. Dapper (Complex & Fast Queries)
    // ==========================================

    private readonly record struct FlatCourseRow(
        short RequirementType,
        string CourseCode,
        string CourseName,
        byte CreditHours,
        decimal? Grade,
        string Status,
        string? PrerequisitesString
    );

    public async Task<StudentStudyPlanDto?> GetStudentStudyPlanAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        const string sql = """
        -- الاستعلام الأول: معلومات الطالب والتخصص
        SELECT
            u."FirstName" || ' ' || u."LastName" AS "StudentName",
            s."UniversityNumber" AS "StudentIdNumber",
            sp."SpecializationName",
            sp."TotalCredits" AS "PlanTotalHours"
        FROM "Students" s
        JOIN "Users" u ON s."UserId" = u."UserId"
        JOIN "Specializations" sp ON s."SpecializationId" = sp."SpecializationId"
        WHERE s."StudentId" = @StudentId;

        -- الاستعلام الثاني: الخطة والمواد مع تجميع أفضل/آخر تسجيل للمادة
        WITH CoursePrereqs AS (
            SELECT
                cp."CourseId",
                string_agg(pr."CourseCode", ',') AS "PrerequisitesString"
            FROM "CoursePrerequisites" cp
            JOIN "Courses" pr ON cp."PrerequisiteId" = pr."CourseId"
            GROUP BY cp."CourseId"
        ),
        RankedEnrollments AS (
            SELECT
                sec."CourseId",
                e."Grade" AS "Grade",
                sec."SectionId",
                ROW_NUMBER() OVER (
                    PARTITION BY sec."CourseId"
                    ORDER BY
                        CASE WHEN e."Grade" IS NOT NULL THEN 1 ELSE 2 END,
                        e."Grade" DESC NULLS LAST
                ) AS rn
            FROM "Enrollments" e
            JOIN "Sections" sec ON e."SectionId" = sec."SectionId"
            WHERE e."StudentId" = @StudentId
        ),
        StudentEnrollments AS (
            SELECT "CourseId", "Grade", "SectionId"
            FROM RankedEnrollments
            WHERE rn = 1
        )
        SELECT
            sc."RequirementType",
            c."CourseCode",
            c."CourseName",
            c."CreditHours",
            se."Grade",
            CASE
                WHEN se."Grade" >= 50 THEN 'Passed'
                WHEN se."Grade" < 50 THEN 'Failed'
                WHEN se."SectionId" IS NOT NULL AND se."Grade" IS NULL THEN 'InProgress'
                ELSE 'NotTaken'
            END AS "Status",
            pr."PrerequisitesString"
        FROM "Students" st
        JOIN "SpecializationCourses" sc ON sc."SpecializationId" = st."SpecializationId"
        JOIN "Courses" c ON c."CourseId" = sc."CourseId"
        LEFT JOIN CoursePrereqs pr ON pr."CourseId" = c."CourseId"
        LEFT JOIN StudentEnrollments se ON se."CourseId" = c."CourseId"
        WHERE st."StudentId" = @StudentId
        ORDER BY sc."RequirementType", c."CourseCode";
        """;

        var connection = Connection;
        bool shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        StudentStudyPlanDto? planDto;
        List<FlatCourseRow> rows;

        try
        {
            var command = new CommandDefinition(sql, new { StudentId = studentId }, cancellationToken: cancellationToken);
            await using var multi = await connection.QueryMultipleAsync(command);

            planDto = await multi.ReadSingleOrDefaultAsync<StudentStudyPlanDto>();
            if (planDto == null) return null;

            rows = (await multi.ReadAsync<FlatCourseRow>()).ToList();
        }
        finally
        {
            if (shouldClose && connection.State != ConnectionState.Closed)
                await connection.CloseAsync();
        }

        // المعالجة وحساب الساعات تتم بعد إغلاق الاتصال وتحريره لحوض الاتصالات (Connection Pool)
        int completedHours = 0;
        var categoriesMap = new Dictionary<short, StudyPlanCategoryDto>(6);

        foreach (var r in rows)
        {
            if (r.Status == "Passed")
                completedHours += r.CreditHours;

            if (!categoriesMap.TryGetValue(r.RequirementType, out var category))
            {
                category = new StudyPlanCategoryDto
                {
                    CategoryName = ((enRequirementType)r.RequirementType).ToString(),
                    RequiredHours = 0,
                    Courses = new List<StudyPlanCourseItemDto>(15)
                };
                categoriesMap[r.RequirementType] = category;
            }

            category.RequiredHours += r.CreditHours;
            category.Courses.Add(new StudyPlanCourseItemDto
            {
                CourseCode = r.CourseCode,
                CourseName = r.CourseName,
                CreditHours = r.CreditHours,
                Grade = r.Grade,
                Status = r.Status,
                Prerequisites = string.IsNullOrEmpty(r.PrerequisitesString)
                    ? []
                    : r.PrerequisitesString.Split(',').ToList()
            });
        }

        planDto.CompletedHours = completedHours;
        planDto.Categories = categoriesMap.Values.ToList();

        return planDto;
    }
    public async Task<IEnumerable<CourseTreeNodeDto>> GetPrerequisiteTreeAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        const string sql = """
        WITH RECURSIVE CourseTree AS (
            -- الأساس: المادة المطلوبة نفسها
            SELECT 
                c."CourseId",
                c."CourseCode",
                c."CourseName",
                CAST(NULL AS uuid) AS "ParentCourseId",
                0 AS "Depth"
            FROM "Courses" c
            WHERE c."CourseId" = @CourseId

            UNION ALL

            -- التكرار: جلب متطلبات المادة السابقة وتفريعاتها
            SELECT 
                prereq."CourseId",
                prereq."CourseCode",
                prereq."CourseName",
                tree."CourseId" AS "ParentCourseId",
                tree."Depth" + 1
            FROM "CoursePrerequisites" cp
            JOIN "Courses" prereq ON cp."PrerequisiteId" = prereq."CourseId"
            JOIN CourseTree tree ON cp."CourseId" = tree."CourseId"
            WHERE tree."Depth" < 10 -- حماية من الحلقات اللانهائية (Infinite Loops)
        )
        SELECT DISTINCT 
            "CourseId",
            "CourseCode",
            "CourseName",
            "ParentCourseId",
            "Depth"
        FROM CourseTree
        ORDER BY "Depth" ASC;
        """;

        var connection = Connection;
        bool shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var command = new CommandDefinition(sql, new { CourseId = courseId }, cancellationToken: cancellationToken);
            return await connection.QueryAsync<CourseTreeNodeDto>(command);
        }
        finally
        {
            if (shouldClose && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }
}