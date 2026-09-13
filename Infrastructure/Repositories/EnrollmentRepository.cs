using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.User;
using Dapper;
using Domain.Enums;
using EduGate.Domain.Interfaces;
using EduGate.Infrastructure.Persistence;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        List<Guid> sectionsToDrop,
        List<Guid> sectionsToAdd,
        CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        bool shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose) await connection.OpenAsync(cancellationToken);

        // 1. بدء الـ Transaction من EF Core لضمان مشاركة السياق مع أي استدعاء داخل الـ Scope
        await using var efTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var transaction = efTransaction.GetDbTransaction();

        try
        {
            var currentUserId = _currentUserService.UserId;
            var now = DateTime.UtcNow;
            int activeStatus = (int)enEnrollmentStatus.Enrolled;
            int droppedStatus = (int)enEnrollmentStatus.Dropped;

            // 2. Drop Sections مع إرجاع الـ IDs المحذوفة فعلياً لمعرفة المفقود بدقة
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

            // 3. Lock and Validate Capacity
            if (sectionsToAdd != null && sectionsToAdd.Any())
            {
                var sortedSectionIds = sectionsToAdd.Distinct().OrderBy(id => id).ToArray();

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
                        new { SectionsToAdd = sortedSectionIds, ActiveStatus = activeStatus },
                        transaction,
                        cancellationToken: cancellationToken)
                )).ToList();

                if (sections.Count != sortedSectionIds.Length)
                    throw new KeyNotFoundException("One or more selected sections were not found.");

                if (sections.Any(s => !s.IsActive))
                    throw new InvalidOperationException("Registration denied: One or more sections do not belong to the active semester.");

                var fullSection = sections.FirstOrDefault(s => s.ActiveCount >= s.Capacity);
                if (fullSection != default)
                    throw new InvalidOperationException($"Section {fullSection.SectionId} is fully booked.");

                // 4. UPSERT مع معالجة إعادة التسجيل
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
                        SectionsToAdd = sortedSectionIds,
                        Now = now,
                        UserId = currentUserId,
                        ActiveStatus = activeStatus
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            // 5. التثبيت عبر EF Core Transaction
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
   
    
