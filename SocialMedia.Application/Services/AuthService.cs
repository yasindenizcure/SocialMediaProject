using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTO;
using SocialMedia.Application.Helpers;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;

namespace SocialMedia.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> Register(RegisterDto request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
                return "Bu email zaten kayıtlı!";

            PasswordHelper.CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "Kayıt başarılı!";
        }

        public async Task<string> Login(string email, string password)
        {
            return "";
        }
    }
}