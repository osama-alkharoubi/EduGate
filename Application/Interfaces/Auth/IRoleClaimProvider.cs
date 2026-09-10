using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Application.Interfaces.Auth
{
    public interface IRoleClaimProvider
    {
        
        string TargetRole { get; }
        Task<Claim> GetRoleClaimAsync(Guid userId);
    }
}
