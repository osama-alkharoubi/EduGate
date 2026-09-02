using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.User
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }

        string? Email { get; }

        string? Role { get; }

        bool IsAuthenticated { get; }
    }
}
