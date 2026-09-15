using Application.DTOs.Schedule;

namespace Application.Interfaces.Services;

public interface IScheduleService
{
    Task<IReadOnlyList<StudentScheduleDto>> GetActiveSemesterScheduleAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentScheduleDto>> GetScheduleBySemesterIdAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfessorScheduleDto>> GetProfessorScheduleAsync(
        Guid professorId,
        Guid? semesterId = null,
        CancellationToken cancellationToken = default);
}