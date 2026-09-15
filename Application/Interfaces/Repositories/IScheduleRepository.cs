using Application.DTOs.Schedule;

namespace Application.Interfaces.Repositories;

public interface IScheduleRepository
{
    Task<IReadOnlyList<StudentScheduleDto>> GetActiveSemesterScheduleAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentScheduleDto>> GetScheduleBySemesterIdAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfessorScheduleDto>> GetProfessorTeachingScheduleAsync(
    Guid professorId,
    Guid? semesterId = null,
    CancellationToken cancellationToken = default);
}