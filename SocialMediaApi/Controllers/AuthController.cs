using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTO;
using SocialMedia.Application.Services;
using SocialMedia.Infrastructure.Data;

namespace SocialMediaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var result = await _authService.Register(request);
            return result == "Kayıt başarılı!" ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var result = await _authService.Login(request.Email, request.Password);

            // Eğer sonuç bir hata mesajıysa (Token değilse)
            if (result == "Kullanıcı bulunamadı!" || result == "Hatalı şifre!")
                return BadRequest(result);

            return Ok(new { token = result }); // Token'ı bir obje içinde dönmek daha şıktır
        }
    }
}