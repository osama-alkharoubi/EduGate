using Application.DTOs.Schedule;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;

    public ScheduleService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }
    public async Task<IReadOnlyList<ProfessorScheduleDto>> GetProfessorScheduleAsync(
        Guid professorId,
        Guid? semesterId = null,
        CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.GetProfessorTeachingScheduleAsync(professorId, semesterId, cancellationToken);
    }
    public async Task<IReadOnlyList<StudentScheduleDto>> GetActiveSemesterScheduleAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.GetActiveSemesterScheduleAsync(studentId, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentScheduleDto>> GetScheduleBySemesterIdAsync(Guid studentId, Guid semesterId, CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.GetScheduleBySemesterIdAsync(studentId, semesterId, cancellationToken);
    }
}