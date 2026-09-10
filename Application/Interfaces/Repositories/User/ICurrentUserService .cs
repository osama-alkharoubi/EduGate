using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.User
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }

        string? Email { get; }

        IEnumerable<string> Roles { get; }

        bool IsAuthenticated { get; }
        string? GetClaimValue(string claimType);
        Guid? GetClaimAsGuid(string claimType);
    }
}
