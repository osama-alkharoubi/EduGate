using EduGate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories
{
    public interface IRoleRepository
    {
       
            Task<Guid?> GetIdByNameAsync(string name, CancellationToken cancellationToken = default);
        
    }
}
