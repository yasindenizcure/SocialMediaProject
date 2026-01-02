using SocialMedia.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SocialMedia.Application.Services
{
    public interface IAuthService
    {
        Task<string> Register(RegisterDto request);
        Task<string> Login(string email, string password);
    }
}
