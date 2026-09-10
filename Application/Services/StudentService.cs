using Application.DTOs.Student;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Enums;
using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public StudentService(
            IStudentRepository studentRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository)
        {
            _studentRepository = studentRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<StudentProfileHeaderDto> GetCurrentStudentProfileAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (userId is null)
            {
                throw new UnauthorizedAccessException("User is not authenticated or token is missing.");
            }

            var profile = await _studentRepository.GetStudentProfileHeaderAsync(userId.Value, cancellationToken);

            if (profile is null)
            {
                throw new KeyNotFoundException("Student profile data not found.");
            }

            return profile;
        }

        public async Task UpdateAcademicStatusAsync(Guid studentId, enAcademicStatus newStatus, CancellationToken cancellationToken = default)
        {
            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);

            if (student is null)
            {
                throw new KeyNotFoundException($"Student with ID '{studentId}' not found.");
            }

            student.AcademicStatus = newStatus;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<StudentDetailsDto> GetStudentByUniversityNumberAsync(string universityNumber, CancellationToken cancellationToken = default)
        {
            var studentDetails = await _studentRepository.GetByUniversityNumberAsync(universityNumber, cancellationToken);

            if (studentDetails is null)
            {
                throw new KeyNotFoundException($"No student found with university number: {universityNumber}");
            }

            return studentDetails;
        }

        public async Task<IEnumerable<StudentListDto>> GetStudentsByFilterAsync(StudentFilterDto filter, CancellationToken cancellationToken = default)
        {
            return await _studentRepository.GetStudentsByFilterAsync(filter, cancellationToken);
        }
     
        public async Task<StudentDetailsDto> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            var studentDetails = await _studentRepository.GetStudentWithDetailsAsync(studentId, cancellationToken);

            if (studentDetails is null)
            {
                throw new KeyNotFoundException($"Student with ID '{studentId}' not found.");
            }

            return studentDetails;
        }
        public async Task DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            // 1. جلب الكيان (Tracked Entity)
            var student = await _studentRepository.GetByIdWithUserAsync(studentId, cancellationToken);

            if (student == null)
            {
                throw new KeyNotFoundException($"Student with ID '{studentId}' not found.");
            }

        
            student.User.IsActive = false;
            student.AcademicStatus = enAcademicStatus.Dismissed;

            // 3. حفظ التغييرات - Change Tracker يرصد التعديلات وينفذ UPDATE تلقائياً
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        public async Task UpdateStudentDetailsAsync(Guid studentId, UpdateStudentDto dto, CancellationToken cancellationToken = default)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
            if (student is null)
            {
                throw new KeyNotFoundException($"Student with ID '{studentId}' not found.");
            }

            // 1. تحديث حقول الطالب
            student.SpecializationId = dto.SpecializationId;

            // 2. تحديث حقول المستخدم
            if (student.User != null)
            {
                student.User.FirstName = dto.FirstName;
                student.User.LastName = dto.LastName;
                student.User.PhoneNumber = dto.PhoneNumber;
            }
            else
            {
                var user = await _userRepository.GetByIdAsync(student.UserId, cancellationToken);
                if (user is null)
                {
                    throw new KeyNotFoundException("User not found.");
                }

                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.PhoneNumber = dto.PhoneNumber;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}