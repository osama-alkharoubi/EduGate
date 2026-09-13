using Application.DTOs.Section;
using Application.Exceptions;
using Application.Interfaces.Common;
using Application.Interfaces.Services;

using EduGate.Domain.Entities;
using EduGate.Domain.Interfaces;


namespace EduGate.Application.Services;

public class SectionService : ISectionService
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IUnitOfWork _unitOfWork; // لإدارة الحفظ في قاعدة البيانات

    public SectionService(ISectionRepository sectionRepository, IUnitOfWork unitOfWork)
    {
        _sectionRepository = sectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateSectionAsync(CreateSectionDto dto, CancellationToken cancellationToken = default)
    {
       


        bool sectionExists = await _sectionRepository.IsSectionNumberExistsAsync(
            dto.CourseId,
            dto.SemesterId,
            dto.SectionNumber,null,
            cancellationToken);

        if (sectionExists)
            throw new InvalidOperationException($"Section number {dto.SectionNumber} already exists.");

        // 3. فحص تعارض جدول المدرس عبر الـ Repository
        bool professorHasConflict = await _sectionRepository.HasProfessorScheduleConflictAsync(
            dto.ProfessorId,
            dto.SemesterId,
            dto.DaysOfWeek,
            dto.StartTime,
            dto.EndTime,
            null,
            cancellationToken);

        if (professorHasConflict)
            throw new InvalidOperationException("The assigned professor has a schedule conflict.");

        // 4. حفظ الكيان
        var section = new Section
        {
            SectionId = Guid.NewGuid(),
            CourseId = dto.CourseId,
            SemesterId = dto.SemesterId,
            ProfessorId = dto.ProfessorId,
            SectionNumber = dto.SectionNumber,
            Capacity = dto.Capacity,
            RoomNumber = dto.RoomNumber,
            DaysOfWeek = dto.DaysOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

         _sectionRepository.Add(section);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return section.SectionId;
    }

    public async Task<IEnumerable<AvailableSectionDto>> GetAvailableSectionsForStudentAsync(
        Guid studentId,
        Guid semesterId,
        CancellationToken cancellationToken = default)
    {
        return await _sectionRepository.GetAvailableSectionsForStudentAsync(studentId, semesterId, cancellationToken);
    }

    public async Task UpdateSectionAsync(UpdateSectionDto dto, CancellationToken cancellationToken = default)
    {
  
        var section = await _sectionRepository.GetByIdAsync(dto.SectionId, cancellationToken);
        if (section == null)
            throw new KeyNotFoundException($"Section with ID {dto.SectionId} was not found.");

        int activeEnrollments = await _sectionRepository.GetActiveEnrollmentCountAsync(dto.SectionId, cancellationToken);
        if (dto.Capacity < activeEnrollments)
            throw new InvalidOperationException(
                $"Cannot reduce capacity to {dto.Capacity}. There are already {activeEnrollments} active enrollments.");

        bool sectionNumberExists = await _sectionRepository.IsSectionNumberExistsAsync(
            dto.CourseId,
            dto.SemesterId,
            dto.SectionNumber,
            excludeSectionId: dto.SectionId,
            cancellationToken);

        if (sectionNumberExists)
            throw new ConflictException(
                $"Section number {dto.SectionNumber} already exists for this course in the selected semester.");

     
        bool professorConflict = await _sectionRepository.HasProfessorScheduleConflictAsync(
            dto.ProfessorId,
            dto.SemesterId,
            dto.DaysOfWeek,
            dto.StartTime,
            dto.EndTime,
            excludeSectionId: dto.SectionId,
            cancellationToken);

        if (professorConflict)
            throw new ConflictException(
                "The assigned professor has a schedule conflict with another section at this time.");

        section.CourseId = dto.CourseId;
        section.SemesterId = dto.SemesterId;
        section.ProfessorId = dto.ProfessorId;
        section.SectionNumber = dto.SectionNumber;
        section.Capacity = dto.Capacity;
        section.RoomNumber = dto.RoomNumber;
        section.DaysOfWeek = dto.DaysOfWeek;
        section.StartTime = dto.StartTime;
        section.EndTime = dto.EndTime;
        section.Status = dto.Status;


        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SectionDetailsDto?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var section = await _sectionRepository.GetByIdWithDetailsAsync(sectionId, cancellationToken);
        if (section == null) return null;

        return new SectionDetailsDto
        {
            SectionId = section.SectionId,
            CourseId = section.CourseId,
            CourseName = section.Course.CourseName,
            SectionNumber = section.SectionNumber,
            ProfessorName = $"{section.Professor.User.FirstName} {section.Professor.User.LastName}",
            Capacity = section.Capacity,
            RoomNumber = section.RoomNumber,
            DaysOfWeek = section.DaysOfWeek,
            StartTime = section.StartTime,
            EndTime = section.EndTime
        };
    }
}