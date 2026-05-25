using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}