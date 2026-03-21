namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string message, bool isHtml = true);
    }
}