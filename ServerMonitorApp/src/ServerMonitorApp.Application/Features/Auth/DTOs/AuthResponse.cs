namespace ServerMonitorApp.Application.Features.Auth.DTOs
{
    public class AuthResponse
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
