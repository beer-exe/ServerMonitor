using ServerMonitorApp.Application.Common.Interfaces;

namespace ServerMonitorApp.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, BCrypt.Net.HashType.SHA512);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash, BCrypt.Net.HashType.SHA512);
        }
    }
}
