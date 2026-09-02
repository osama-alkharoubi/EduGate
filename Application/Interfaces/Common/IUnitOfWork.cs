using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Common
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
