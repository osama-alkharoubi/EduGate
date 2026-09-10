using Application.Interfaces.Common;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // هذه الدالة ستقوم بترحيل كل التغييرات المتراكمة في الذاكرة إلى قاعدة البيانات
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
