using ServerMonitorApp.Domain.Models;
using System.Security.Claims;

namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
