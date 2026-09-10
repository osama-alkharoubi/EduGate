using Application.DTOs.Student;
using Domain.Enums;
using EduGate.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
    }

    public async Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .FindAsync(studentId, cancellationToken);
    }

    public async Task<StudentDetailsDto?> GetStudentWithDetailsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
           .AsNoTracking()
           .Where(s => s.StudentId == studentId)
           .Select(s => new StudentDetailsDto
           {
               StudentId = s.StudentId,
               UniversityNumber = s.UniversityNumber,
               FullName = s.User.FirstName + " " + s.User.LastName,
               Email = s.User.Email,
               SpecializationName = s.Specialization.SpecializationName, // تأكد من اسم الحقل عندك
               GPA = s.GPA,
               CompletedCredits = s.CompletedCredits,
               EnrollmentDate = s.EnrollmentDate,
               AcademicStatus = s.AcademicStatus.ToString()
           })
           .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Guid?> GetStudentIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Students.Where(s => s.UserId == userId)
            .Select(s => s.StudentId)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<Student?> GetByIdWithUserAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);
    }
    public async Task<StudentProfileHeaderDto?> GetStudentProfileHeaderAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => s.UserId == userId)
           .Select(s => new StudentProfileHeaderDto
           {
               FullName = s.User.FirstName + " " + s.User.LastName,
               SpecializationName = s.Specialization.SpecializationName, // تأكد من اسم الخاصية في جدول التخصص
               UniversityNumber = s.UniversityNumber,
               Email = s.User.Email,
               AcademicStatus = s.AcademicStatus.ToString()
           })
            .FirstOrDefaultAsync(cancellationToken);
     
    }
    public async Task<IEnumerable<StudentListDto>> GetStudentsByFilterAsync(StudentFilterDto filter, CancellationToken cancellationToken = default)
    {
        // أزلنا الـ Include لأن الـ Select يغني عنها
        var query = _context.Students
            .AsNoTracking()
            .AsQueryable();

        if (filter.SpecializationId.HasValue)
        {
            query = query.Where(s => s.SpecializationId == filter.SpecializationId.Value);
        }

        if (filter.EnrollmentYear.HasValue)
        {
            query = query.Where(s => s.EnrollmentDate.Year == filter.EnrollmentYear.Value);
        }

        if (!string.IsNullOrEmpty(filter.AcademicStatus))
        {
            // تحويل النص إلى Enum قبل إرساله لقاعدة البيانات
            if (Enum.TryParse<enAcademicStatus>(filter.AcademicStatus, true, out var statusEnum))
            {
                query = query.Where(s => s.AcademicStatus == statusEnum);
            }
        }

        if (filter.MinGPA.HasValue)
        {
            query = query.Where(s => s.GPA >= filter.MinGPA.Value);
        }

        if (filter.MaxGPA.HasValue)
        {
            query = query.Where(s => s.GPA <= filter.MaxGPA.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            // فصلنا البحث لعدم إجبار قاعدة البيانات على دمج الحقول
            query = query.Where(s => s.UniversityNumber.Contains(filter.SearchTerm) ||
                                     s.User.FirstName.Contains(filter.SearchTerm) ||
                                     s.User.LastName.Contains(filter.SearchTerm));
        }

        return await query
             .Select(s => new StudentListDto
             {
                 UniversityNumber = s.UniversityNumber,
                 FullName = s.User.FirstName + " " + s.User.LastName, 
                 GPA = s.GPA,
                 AcademicStatus = s.AcademicStatus.ToString()
             })
             .ToListAsync(cancellationToken);
    }
    public async Task<string?> GetLastUniversityNumberAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // بنجيب أكبر رقم جامعي بيبدأ بالـ Prefix تبع السنة والتخصص الحالي
        return await _context.Students
            .Where(s => s.UniversityNumber.StartsWith(prefix))
            .OrderByDescending(s => s.UniversityNumber)
            .Select(s => s.UniversityNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<StudentDetailsDto?> GetByUniversityNumberAsync(string universityNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => s.UniversityNumber == universityNumber)
            .Select(s => new StudentDetailsDto
            {
                StudentId = s.StudentId,
                UniversityNumber = s.UniversityNumber,
                FullName = s.User.FirstName + " " + s.User.LastName,
                Email = s.User.Email,
                SpecializationName = s.Specialization.SpecializationName, 
                GPA = s.GPA,
                CompletedCredits = s.CompletedCredits,
                EnrollmentDate = s.EnrollmentDate,
                AcademicStatus = s.AcademicStatus.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<bool> ExistsByUniversityNumberAsync(string universityNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AnyAsync(s => s.UniversityNumber == universityNumber, cancellationToken);
    }

    
}