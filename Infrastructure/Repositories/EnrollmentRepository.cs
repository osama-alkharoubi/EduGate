using Application.DTOs.Enrollment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.User;
using Dapper;
using Domain.Enums;
using EduGate.Domain.Entities;
using EduGate.Domain.Interfaces;
using EduGate.Infrastructure.Persistence;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EduGate.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    public EnrollmentRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
    private DbConnection Connection
    {
        get
        {
            return _context.Database.GetDbConnection();
        }
    }
    public async Task<int> ExecuteBulkGpaRecalculationAsync(Guid semesterId, CancellationToken cancellationToken = default)
    {

        const string sql = @"WITH TargetStudents AS (
    -- 0. تحديد الطلاب اللي عندهم علامات مسجلة بهاد الفصل
    SELECT DISTINCT e.""StudentId""
    FROM ""Enrollments"" e
    INNER JOIN ""Sections"" s ON e.""SectionId"" = s.""SectionId""
    WHERE s.""SemesterId"" = @SemesterId
      AND e.""Grade"" IS NOT NULL
      AND e.""Status"" = ANY(@IncludedStatuses)
),
SemesterStats AS (
    -- 1. حساب المعدل والساعات للفصل الحالي فقط
    SELECT 
        e.""StudentId"",
        ROUND(SUM(e.""Grade"" * c.""CreditHours"")::numeric / NULLIF(SUM(c.""CreditHours""), 0), 2) AS semester_gpa,
        COALESCE(SUM(CASE WHEN e.""Status"" = @CompletedStatus THEN c.""CreditHours"" ELSE 0 END), 0) AS semester_credits
    FROM ""Enrollments"" e
    INNER JOIN ""Sections"" s ON e.""SectionId"" = s.""SectionId""
    INNER JOIN ""Courses"" c ON s.""CourseId"" = c.""CourseId""
    WHERE s.""SemesterId"" = @SemesterId
      AND e.""Grade"" IS NOT NULL
      AND e.""Status"" = ANY(@IncludedStatuses)
    GROUP BY e.""StudentId""
),
RankedCumulativeCourses AS (
    -- 2. جلب كل المواد في تاريخ الطالب (لحساب التراكمي وأخذ أعلى علامة)
    SELECT 
        e.""StudentId"",
        e.""Grade"",
        e.""Status"",
        c.""CreditHours"",
        ROW_NUMBER() OVER (
            PARTITION BY e.""StudentId"", s.""CourseId"" 
            ORDER BY e.""Grade"" DESC NULLS LAST
        ) as rn
    FROM ""Enrollments"" e
    INNER JOIN ""Sections"" s ON e.""SectionId"" = s.""SectionId""
    INNER JOIN ""Courses"" c ON s.""CourseId"" = c.""CourseId""
    WHERE e.""Grade"" IS NOT NULL
      AND e.""Status"" = ANY(@IncludedStatuses)
      AND e.""StudentId"" IN (SELECT ""StudentId"" FROM TargetStudents)
),
CumulativeStats AS (
    -- 3. تجميع التراكمي بناءً على المادة الناجحة/الأعلى فقط (rn = 1)
    SELECT 
        ""StudentId"",
        ROUND(SUM(""Grade"" * ""CreditHours"")::numeric / NULLIF(SUM(""CreditHours""), 0), 2) AS cumulative_gpa,
        COALESCE(SUM(CASE WHEN ""Status"" = @CompletedStatus THEN ""CreditHours"" ELSE 0 END), 0) AS total_credits
    FROM RankedCumulativeCourses
    WHERE rn = 1
    GROUP BY ""StudentId""
),
WarningEval AS (
    -- 4. دمج البيانات الفصلية والتراكمية والطالة الحالية للطالب
    SELECT 
        t.""StudentId"",
        sem.semester_gpa,
        sem.semester_credits,
        cum.cumulative_gpa,
        cum.total_credits,
        st.""AcademicWarningsCount"" AS current_warnings,
        st.""AcademicStatus"" AS current_status,
        COALESCE(ss.""WarningIssued"", FALSE) AS already_issued
    FROM TargetStudents t
    INNER JOIN SemesterStats sem ON t.""StudentId"" = sem.""StudentId""
    INNER JOIN CumulativeStats cum ON t.""StudentId"" = cum.""StudentId""
    INNER JOIN ""Students"" st ON st.""StudentId"" = t.""StudentId""
    LEFT JOIN ""StudentSemesters"" ss ON ss.""StudentId"" = t.""StudentId"" AND ss.""SemesterId"" = @SemesterId
),
UpsertSemesters AS (
    -- 5. إدخال أو تحديث إحصائيات الطالب للفصل
    INSERT INTO ""StudentSemesters"" (
        ""StudentSemesterId"",
        ""StudentId"",
        ""SemesterId"",
        ""SemesterGPA"",
        ""CumulativeGPA"",
        ""SemesterCompletedCredits"",
        ""TotalCompletedCredits"",
        ""AcademicStatus"",
        ""WarningIssued"",
        ""CreatedAt""
    )
    SELECT 
        gen_random_uuid(),
        w.""StudentId"",
        @SemesterId,
        w.semester_gpa,
        w.cumulative_gpa,
        w.semester_credits,
        w.total_credits,
        -- AcademicStatus للأرشيف
        CASE
           WHEN EXISTS (
               SELECT 1 
               FROM ""Enrollments"" e 
               INNER JOIN ""Sections"" s ON e.""SectionId"" = s.""SectionId"" 
               WHERE s.""SemesterId"" = @SemesterId 
                 AND e.""StudentId"" = w.""StudentId"" 
                 AND e.""Status"" = 6
           ) THEN w.current_status
           WHEN w.cumulative_gpa < 60.0 AND (
               CASE WHEN w.already_issued = FALSE THEN w.current_warnings + 1 ELSE w.current_warnings END
           ) >= 3 THEN 3
           ELSE 1
        END,
        -- WarningIssued: إذا في Incomplete بنحافظ على حالته السابقة لتجنب الازدواجية
        CASE 
             WHEN EXISTS (
                SELECT 1 
                FROM ""Enrollments"" e 
                INNER JOIN ""Sections"" s ON e.""SectionId"" = s.""SectionId"" 
                WHERE s.""SemesterId"" = @SemesterId 
                  AND e.""StudentId"" = w.""StudentId"" 
                  AND e.""Status"" = 6
            ) THEN w.already_issued
            WHEN w.cumulative_gpa < 60.0 THEN TRUE 
            ELSE FALSE 
         END,
        @Now
    FROM WarningEval w
    ON CONFLICT (""StudentId"", ""SemesterId"") 
    DO UPDATE SET
        ""SemesterGPA"" = EXCLUDED.""SemesterGPA"",
        ""CumulativeGPA"" = EXCLUDED.""CumulativeGPA"",
        ""SemesterCompletedCredits"" = EXCLUDED.""SemesterCompletedCredits"",
        ""TotalCompletedCredits"" = EXCLUDED.""TotalCompletedCredits"",
        ""AcademicStatus"" = EXCLUDED.""AcademicStatus"",
        ""WarningIssued"" = EXCLUDED.""WarningIssued"",
        ""UpdatedAt"" = @Now
    RETURNING ""StudentId""
)
-- 6. تحديث ملف الطالب الأساسي بناءً على التراكمي والأولويات الصارمة
UPDATE ""Students"" SET 
    ""GPA"" = w.cumulative_gpa,
    ""CompletedCredits"" = w.total_credits,
    ""AcademicWarningsCount"" = CASE 
        -- 1. تجميد العداد كأولوية قصوى لو عنده مادة Incomplete
        WHEN EXISTS (
            SELECT 1 
            FROM ""Enrollments"" e 
            INNER JOIN ""Sections"" sec ON e.""SectionId"" = sec.""SectionId"" 
            WHERE sec.""SemesterId"" = @SemesterId 
              AND e.""StudentId"" = w.""StudentId"" 
              AND e.""Status"" = 6
        ) THEN w.current_warnings
        -- 2. تصفير الإنذارات إذا المعدل ارتفع وتم حسم كل مواده
        WHEN w.cumulative_gpa >= 60.0 THEN 0
        -- 3. زيادة العداد إذا المعدل تحت 60 وما عنده Incomplete
        WHEN w.already_issued = FALSE THEN LEAST(w.current_warnings + 1, 3)
        ELSE w.current_warnings
    END,
    ""AcademicStatus"" = CASE
        WHEN w.total_credits >= sp.""TotalCredits"" THEN 4
        -- 2. تجميد الحالة على ما هي عليه إذا وجدت مادة معلقة
        WHEN EXISTS (
            SELECT 1 
            FROM ""Enrollments"" e
            INNER JOIN ""Sections"" sec ON e.""SectionId"" = sec.""SectionId""
            WHERE sec.""SemesterId"" = @SemesterId 
               AND e.""StudentId"" = w.""StudentId"" 
               AND e.""Status"" = 6
        ) THEN w.current_status
        -- 3. الفصل الأكاديمي (الوصول إلى 3 إنذارات أو أكثر)
        WHEN w.cumulative_gpa < 60.0 AND (
            CASE 
                WHEN w.already_issued = FALSE THEN LEAST(w.current_warnings + 1, 3)
                ELSE LEAST(w.current_warnings, 3)
            END
        ) >= 3 THEN 3
        -- 4. في كل الحالات الأخرى يظل الطالب منتظماً ونشطاً
        ELSE 1
    END,
    ""IsGraduating"" = CASE 
         WHEN (sp.""TotalCredits"" - w.total_credits) BETWEEN 12 AND 21 THEN TRUE
        ELSE FALSE 
     END
FROM WarningEval w 
INNER JOIN UpsertSemesters ups ON ups.""StudentId"" = w.""StudentId"",
""Specializations"" sp
WHERE ""Students"".""StudentId"" = w.""StudentId""
  AND ""Students"".""SpecializationId"" = sp.""SpecializationId"";";
        const string pendingGradesCheckSql = @"
    SELECT COUNT(1)
    FROM ""Enrollments"" e
    INNER JOIN ""Sections"" s ON e.""SectionId"" = s.""SectionId""
    WHERE s.""SemesterId"" = @SemesterId
      AND e.""Status"" = 1 -- Enrolled
      AND e.""Grade"" IS NULL;";
        var conn = Connection;
        var wasClosed = conn.State == ConnectionState.Closed;

        try
        {
            if (wasClosed)
                await conn.OpenAsync(cancellationToken);


            var pendingCount = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
     pendingGradesCheckSql,
     new { SemesterId = semesterId },
     cancellationToken: cancellationToken
 ));

            if (pendingCount > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot finalize semester. There are still {pendingCount} student enrollments without submitted grades.");
            }
            var affectedRows = await conn.ExecuteAsync(new CommandDefinition(
                sql,
                new {
                    SemesterId = semesterId,
                    IncludedStatuses = new[]
        {
            (short)enEnrollmentStatus.Completed,
            (short)enEnrollmentStatus.Failed
        },
                    CompletedStatus = (short)enEnrollmentStatus.Completed,
                    Now = DateTime.UtcNow
                },
                cancellationToken: cancellationToken
            ));

            return affectedRows;
        }
        finally
        {
            if (wasClosed && conn.State != ConnectionState.Closed)
                await conn.CloseAsync();
        }
    }
    public async Task<Enrollment?> GetBySectionAndStudentIdAsync(Guid sectionId, Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.SectionId == sectionId && e.StudentId == studentId, cancellationToken);
    }
    public async Task<List<Enrollment>> GetEnrollmentsBySectionIdAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Where(e => e.SectionId == sectionId && e.Status == enEnrollmentStatus.Enrolled)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Guid>> GetActiveStudentSectionIdsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId && e.Status == enEnrollmentStatus.Enrolled) // 1 = Enrolled
            .Select(e => e.SectionId)
            .ToListAsync(cancellationToken);
    }
    public async Task<bool> HasBatchScheduleConflictAsync(
             Guid studentId,
             List<Guid> sectionIds,
             List<Guid> sectionIdsToDrop,
             CancellationToken cancellationToken = default)
    {
        if (sectionIds == null || !sectionIds.Any())
            return false;

        const string sql = """
        WITH NewSections AS (
            SELECT 
                s."SectionId", 
                s."SemesterId", 
                s."StartTime", 
                s."EndTime", 
                string_to_array(s."DaysOfWeek", ',') AS "Days"
            FROM "Sections" s
            JOIN "Semesters" sem ON s."SemesterId" = sem."SemesterId"
            WHERE s."SectionId" = ANY(@SectionIds) 
              AND sem."IsActive" = true
        ),
        ExistingSections AS (
            SELECT 
                s."SectionId", 
                s."SemesterId", 
                s."StartTime", 
                s."EndTime", 
                string_to_array(s."DaysOfWeek", ',') AS "Days"
            FROM "Sections" s
            JOIN "Enrollments" e ON s."SectionId" = e."SectionId"
            JOIN "Semesters" sem ON s."SemesterId" = sem."SemesterId"
            WHERE e."StudentId" = @StudentId 
              AND e."Status" = @ActiveStatus 
              AND sem."IsActive" = true
              AND NOT (s."SectionId" = ANY(@SectionIdsToDrop))
        )
        SELECT EXISTS (
            -- 1. فحص التعارض داخل الشُعب المرفوعة مع بعضها
            SELECT 1
            FROM NewSections a
            JOIN NewSections b ON a."SectionId" < b."SectionId" 
                              AND a."SemesterId" = b."SemesterId" -- إغلاق الثغرة الدفاعية
            WHERE a."StartTime" < b."EndTime" 
              AND a."EndTime" > b."StartTime"
              AND a."Days" && b."Days"
        )
        OR EXISTS (
            -- 2. فحص التعارض بين الشُعب المرفوعة والمواد المسجلة مسبقاً
            SELECT 1
            FROM NewSections n
            JOIN ExistingSections e ON n."SemesterId" = e."SemesterId"
            WHERE n."StartTime" < e."EndTime" 
              AND n."EndTime" > e."StartTime"
              AND n."Days" && e."Days"
              AND n."SectionId" <> e."SectionId" -- إغلاق ثغرة التعارض الوهمي مع نفس الشعبة
        );
        """;

        var connection = _context.Database.GetDbConnection();
        bool shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var command = new CommandDefinition(
                sql,
                new
                {
                    StudentId = studentId,
                    SectionIds = sectionIds.Distinct().ToArray(),
                    SectionIdsToDrop = (sectionIdsToDrop ?? new List<Guid>()).Distinct().ToArray(),
                    ActiveStatus = (int)enEnrollmentStatus.Enrolled
                },
                cancellationToken: cancellationToken);

            return await connection.QuerySingleAsync<bool>(command);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }
    public async Task ModifyScheduleAtomicAsync(
        Guid studentId,
        List<Guid> sectionsToAdd,
        List<Guid> sectionsToDrop,
        CancellationToken cancellationToken = default)
    {
        // فحص التضارب المبكر: منع إرسال نفس الشعبة للإضافة والحذف في نفس الطلب
        if (sectionsToAdd != null && sectionsToDrop != null && sectionsToAdd.Intersect(sectionsToDrop).Any())
        {
            throw new InvalidOperationException("A section cannot be added and dropped in the same request.");
        }

        var connection = _context.Database.GetDbConnection();
        bool shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose) await connection.OpenAsync(cancellationToken);

        await using var efTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var transaction = efTransaction.GetDbTransaction();

        try
        {
            var currentUserId = _currentUserService.UserId;
            var now = DateTime.UtcNow;
            int activeStatus = (int)enEnrollmentStatus.Enrolled;
            int droppedStatus = (int)enEnrollmentStatus.Dropped;

            // 1. Drop Sections
            if (sectionsToDrop != null && sectionsToDrop.Any())
            {
                var distinctDropIds = sectionsToDrop.Distinct().ToArray();
                const string dropSql = """
                UPDATE "Enrollments" 
                SET "Status" = @DroppedStatus,
                    "UpdatedAt" = @Now,
                    "UpdatedBy" = @UserId
                WHERE "StudentId" = @StudentId
                   AND "SectionId" = ANY(@SectionsToDrop)
                   AND "Status" = @ActiveStatus
                RETURNING "SectionId";
                """;

                var droppedSectionIds = (await connection.QueryAsync<Guid>(new CommandDefinition(
                    dropSql,
                    new
                    {
                        StudentId = studentId,
                        SectionsToDrop = distinctDropIds,
                        Now = now,
                        UserId = currentUserId,
                        ActiveStatus = activeStatus,
                        DroppedStatus = droppedStatus
                    },
                    transaction,
                    cancellationToken: cancellationToken))).ToList();

                var missingDropIds = distinctDropIds.Except(droppedSectionIds).ToList();
                if (missingDropIds.Any())
                {
                    throw new InvalidOperationException($"Cannot drop sections that are not actively enrolled: {string.Join(", ", missingDropIds)}");
                }
            }

            // 2. Add Sections مع التصفية الصحيحة
            if (sectionsToAdd != null && sectionsToAdd.Any())
            {
                var sortedSectionIds = sectionsToAdd.Distinct().OrderBy(id => id).ToArray();

                const string getAlreadyEnrolledSql = """
                SELECT "SectionId" 
                FROM "Enrollments" 
                WHERE "StudentId" = @StudentId 
                  AND "SectionId" = ANY(@SectionsToAdd) 
                  AND "Status" = @ActiveStatus;
                """;

                var alreadyEnrolledIds = (await connection.QueryAsync<Guid>(new CommandDefinition(
                    getAlreadyEnrolledSql,
                    new
                    {
                        StudentId = studentId,
                        SectionsToAdd = sortedSectionIds,
                        ActiveStatus = activeStatus
                    },
                    transaction,
                    cancellationToken: cancellationToken))).ToHashSet();

                var effectiveSectionsToAdd = sortedSectionIds.Where(id => !alreadyEnrolledIds.Contains(id)).ToArray();
                const string validateMajorCoursesSql = """
    SELECT c."CourseCode"
    FROM "Sections" s
    JOIN "Courses" c ON s."CourseId" = c."CourseId"
    JOIN "Students" st ON st."StudentId" = @StudentId
    LEFT JOIN "SpecializationCourses" sc 
        ON sc."CourseId" = s."CourseId" 
       AND sc."SpecializationId" = st."SpecializationId"
    WHERE s."SectionId" = ANY(@SectionsToAdd)
      AND sc."CourseId" IS NULL;
""";

                var invalidCourses = (await connection.QueryAsync<string>(new CommandDefinition(
                    validateMajorCoursesSql,
                    new { StudentId = studentId, SectionsToAdd = effectiveSectionsToAdd },
                    transaction,
                    cancellationToken: cancellationToken
                ))).ToList();

                if (invalidCourses.Any())
                {
                    throw new InvalidOperationException(
                        $"Registration denied: The following courses are not in your study plan: {string.Join(", ", invalidCourses)}");
                }
                if (effectiveSectionsToAdd.Length > 0)
                {
                    const string lockAndCheckSql = """
                    WITH LockedSections AS (
                        SELECT 
                             s."SectionId", 
                             s."Capacity", 
                             sem."IsActive"
                        FROM "Sections" s
                        JOIN "Semesters" sem ON s."SemesterId" = sem."SemesterId"
                        WHERE s."SectionId" = ANY(@SectionsToAdd)
                        ORDER BY s."SectionId"
                        FOR UPDATE OF s
                    )
                    SELECT 
                         ls."SectionId",
                         ls."Capacity",
                         COUNT(e."EnrollmentId") AS "ActiveCount",
                         ls."IsActive"
                    FROM LockedSections ls
                    LEFT JOIN "Enrollments" e 
                         ON ls."SectionId" = e."SectionId" 
                        AND e."Status" = @ActiveStatus
                    GROUP BY ls."SectionId", ls."Capacity", ls."IsActive";
                    """;

                    var sections = (await connection.QueryAsync<(Guid SectionId, int Capacity, int ActiveCount, bool IsActive)>(
                        new CommandDefinition(
                            lockAndCheckSql,
                            new { SectionsToAdd = effectiveSectionsToAdd, ActiveStatus = activeStatus }, // تم التصحيح هنا
                            transaction,
                            cancellationToken: cancellationToken)
                    )).ToList();

                    // التحقق باستخدام طول المصفوفة المفلترة فقط
                    if (sections.Count != effectiveSectionsToAdd.Length)
                        throw new KeyNotFoundException("One or more selected sections were not found.");

                    if (sections.Any(s => !s.IsActive))
                        throw new InvalidOperationException("Registration denied: One or more sections do not belong to the active semester.");

                    var fullSection = sections.FirstOrDefault(s => s.ActiveCount >= s.Capacity);
                    if (fullSection != default)
                        throw new InvalidOperationException($"Section {fullSection.SectionId} is fully booked.");

                    // 3. الإدخال الفعلي للشعب الجديدة فقط
                    const string upsertSql = """
                    INSERT INTO "Enrollments" (
                        "EnrollmentId", 
                        "StudentId", 
                        "SectionId", 
                        "Status", 
                        "CreatedAt", 
                        "CreatedBy"
                    )
                    SELECT 
                         gen_random_uuid(), 
                         @StudentId, 
                         unnest(@SectionsToAdd), 
                         @ActiveStatus, 
                         @Now, 
                         @UserId
                    ON CONFLICT ("StudentId", "SectionId")
                    DO UPDATE SET 
                         "Status" = @ActiveStatus,
                         "UpdatedAt" = EXCLUDED."CreatedAt",
                         "UpdatedBy" = EXCLUDED."CreatedBy";
                    """;

                    await connection.ExecuteAsync(new CommandDefinition(
                        upsertSql,
                        new
                        {
                            StudentId = studentId,
                            SectionsToAdd = effectiveSectionsToAdd, // تم التصحيح هنا
                            Now = now,
                            UserId = currentUserId,
                            ActiveStatus = activeStatus
                        },
                        transaction,
                        cancellationToken: cancellationToken));
                }
            }

            await efTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await efTransaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

  
}



