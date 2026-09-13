using Application.DTOs.Section;
using EduGate.Domain.Entities;

namespace EduGate.Domain.Interfaces;

public interface ISectionRepository
{
    Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // بنحتاجها وقت التسجيل عشان نجيب معلومات المادة والفصل وأوقات الدوام
    Task<Section?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasProfessorScheduleConflictAsync(
            Guid professorId,
            Guid semesterId,
            string daysOfWeek,
            TimeOnly startTime,
            TimeOnly endTime,
            Guid? excludeSectionId = null,
            CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableSectionDto>> GetAvailableSectionsForStudentAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default);

    // بنحتاجها لعرض الشعب المتاحة للطالب لمادة معينة في الفصل الحالي
    Task<IEnumerable<Section>> GetAvailableSectionsAsync(Guid courseId, Guid semesterId, CancellationToken cancellationToken = default);

    // فحص إذا رقم الشعبة موجود مسبقاً عشان نمنع التكرار (Unique Check)
    Task<bool> IsSectionNumberExistsAsync(
          Guid courseId,
          Guid semesterId,
          int sectionNumber,
          Guid? excludeSectionId = null,
          CancellationToken cancellationToken = default);

    Task<int> GetActiveEnrollmentCountAsync(
        Guid sectionId,
        CancellationToken cancellationToken = default);

    void Add(Section section);
    void Update(Section section);
    void Delete(Section section);
}