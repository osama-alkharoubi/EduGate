using Application.DTOs.Student;
using EduGate.Domain.Entities;

public interface IStudentRepository
{
    // 1. العمليات الأساسية (Basic Operations)
    void Add(Student student);
  
    // 2. الاستعلامات الأساسية
    Task<Student?> GetByIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    public Task<Student?> GetByIdWithUserAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<StudentProfileHeaderDto?> GetStudentProfileHeaderAsync(Guid userId, CancellationToken cancellationToken);
    Task<IEnumerable<StudentListDto>> GetStudentsByFilterAsync(StudentFilterDto filter, CancellationToken cancellationToken = default);
    // 3. الاستعلامات الأكاديمية المتخصصة
    Task<StudentDetailsDto?> GetStudentWithDetailsAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<string?> GetLastUniversityNumberAsync(string prefix, CancellationToken cancellationToken = default);
    Task<StudentDetailsDto?> GetByUniversityNumberAsync(string universityNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUniversityNumberAsync(string universityNumber, CancellationToken cancellationToken = default);
    Task<Guid?> GetStudentIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

}