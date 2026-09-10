using Domain.Entities;
using EduGate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<College> Colleges => Set<College>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<Professor> Professors => Set<Professor>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CoursePrerequisite> CoursePrerequisites => Set<CoursePrerequisite>();
        public DbSet<SpecializationCourse> SpecializationCourses => Set<SpecializationCourse>();
        public DbSet<Semester> Semesters => Set<Semester>();
        public DbSet<Section> Sections => Set<Section>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost; Port=5432; Database=EduGateDb; Username=postgres; Password=mysecretpassword");
            }
        }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            // 1. GUIDs ثابتة عشان ما تتغير بكل Migration
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var studentRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var professorRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            // 2. زرع الرتب
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = adminRoleId, RoleName = "Admin" },
                new Role { RoleId = studentRoleId, RoleName = "Student" },
                new Role { RoleId = professorRoleId, RoleName = "Professor" }
            );

            // 3. زرع المستخدم المسؤول (Admin)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = adminUserId,
                    UserName = "admin",
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@edugate.com",
                    HashPassword = "$2a$11$0H5A9efIwT9HTz0AOWYaGOVJfE7tJGG3/XJHtqgwfz/OJs4ulwPae", // Admin@Secure123
                    PhoneNumber = "0790000000",
                    IsActive = true
                }
            );

            // 4. ربط الـ Admin برتبة الـ Admin
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    UserId = adminUserId,
                    RoleId = adminRoleId
                }
            );
         
        }
    }
}
