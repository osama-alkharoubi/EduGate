using Application.DTOs;
using Application.DTOs.Auth;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Domain.Enums;
using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;   
using System.Text;
namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleRepository _roleRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISpecializationRepository _SpecializationRepository;
        public AuthService(IUserRepository userRepository, IJwtProvider jwtProvider, IPasswordHasher passwordHasher,IUnitOfWork unitOfWork,IRefreshTokenRepository refreshTokensRepository, IRoleRepository roleRepository, IStudentRepository studentRepository, ISpecializationRepository specializationRepository)
        {
            _userRepository = userRepository;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
            _refreshTokenRepository = refreshTokensRepository;  
            _unitOfWork = unitOfWork;
            _roleRepository = roleRepository;
            _studentRepository = studentRepository;
            _SpecializationRepository = specializationRepository;
        }
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetForAuthByEmailAsync(request.Email, cancellationToken);

            if (user == null || !_passwordHasher.Verify(request.Password, user.HashPassword))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
          var accessToken=  await _jwtProvider.GenerateTokenAsync(user);
            var RefreshToken = _jwtProvider.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                Token = RefreshToken,
                CreatedOnUtc = DateTime.UtcNow,
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _refreshTokenRepository.Add(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = RefreshToken
            };
        }
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
        {
            // 1. البحث عن التوكن مع بيانات المستخدم (باستخدام الـ Projection)
            var result = await _refreshTokenRepository.GetByTokenWithUserAsync(request.RefreshToken, cancellationToken);

            // 2. التحقق من وجود التوكن، وأنه غير مبطل، ولم تنتهِ صلاحيته
            if (result == null || result.RefreshToken.IsRevoked || result.RefreshToken.ExpiresOnUtc < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }
            _refreshTokenRepository.Revoke(result.RefreshToken.Id);
     
            // 4. توليد Access Token و Refresh Token جديدين
            var newAccessToken = await _jwtProvider.GenerateTokenAsync(result.User);
            var newRefreshTokenString = _jwtProvider.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = result.User.UserId,
                Token = newRefreshTokenString,
                CreatedOnUtc = DateTime.UtcNow,
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7), // نفس المدة التي حددناها سابقاً
                IsRevoked = false
            };

            // 5. إضافة التوكن الجديد إلى الذاكرة (Change Tracker)
            _refreshTokenRepository.Add(newRefreshTokenEntity);

            // 6. ترحيل التغييرات (Update للقديم و Insert للجديد) دفعة واحدة لقاعدة البيانات
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString
            };
        }
        public async Task<RegisterStudentResponseDto> RegisterStudentAsync(RegisterStudentRequestDto request, CancellationToken cancellationToken = default)
        {
            // 1. فحص التكرار: التأكد من أن الإيميل أو اسم المستخدم غير موجود مسبقاً
            // (يجب أن تضيف دالة ExistsByEmailOrUserNameAsync في IUserRepository)
            var userExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
            if (userExists)
            {
                throw new ConflictException(
             "Email is already in use.");
            }

            // 2. تشفير كلمة المرور
            var hashedPassword = _passwordHasher.Hash(request.Password);

            // 3. إنشاء كيان المستخدم الجديد
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                HashPassword = hashedPassword,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                IsActive = true // تفعيل الحساب فوراً لتجاوز الـ Query Filter
            };
            string generatedUniversityNumber = await GenerateUniversityNumberAsync(request.SpecializationId, cancellationToken);

            var student = new Student
            {
                StudentId = Guid.NewGuid(),
                UserId = newUser.UserId, // الـ Id الخاص باليوزر الذي يتم إنشاؤه في نفس اللحظة
                SpecializationId = request.SpecializationId,
                UniversityNumber = generatedUniversityNumber,
                GPA = 0,
                CompletedCredits = 0,
                EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AcademicStatus = enAcademicStatus.Active // تأكد من اسم الحالة في الـ Enum لديك
            };
            newUser.Student = student;
            // 4. جلب رتبة الطالب من قاعدة البيانات لربطها بالمستخدم
            // (يجب أن يكون لديك دالة تجلب الرتبة بناءً على اسمها من مستودع الرتب)
            var studentRole = await _roleRepository.GetIdByNameAsync("Student", cancellationToken);
            if (studentRole == null)
            {
                throw new NotFoundException("Default Student role not found.");
            }
            newUser.UserRoles.Add(new UserRole
            {
                UserId = newUser.UserId,
                RoleId = studentRole.Value
            });

            // 5. إضافة المستخدم للذاكرة
            _userRepository.Add(newUser);

          

          
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RegisterStudentResponseDto
            {
                UserId = newUser.UserId,
                UserName = newUser.UserName,
                Email = newUser.Email,
               UniversityNumber = student.UniversityNumber,
                Message = "Student registered successfully."
            };
        }
        private async Task<string> GenerateUniversityNumberAsync(Guid SpecializationId, CancellationToken cancellationToken)
        {
            var currentYear = DateTime.UtcNow.Year.ToString();
         
            var specCode = await _SpecializationRepository.GetCodeById(SpecializationId, cancellationToken);
            if (specCode == null) {
                throw new NotFoundException($"Specialization with ID '{SpecializationId}' not found.");
            }
            var prefix = $"{currentYear}{specCode.Value:D3}";

            var lastNumber = await _studentRepository.GetLastUniversityNumberAsync(prefix, cancellationToken);
            var nextSequence = 1;

            if (!string.IsNullOrEmpty(lastNumber) && lastNumber.Length > prefix.Length)
            {
                var sequencePart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(sequencePart, out var lastSequence))
                {
                    nextSequence = lastSequence + 1;
                }
            }

            return $"{prefix}{nextSequence:D4}";
        }
    }
}
