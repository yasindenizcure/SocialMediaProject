using Microsoft.AspNetCore.Mvc;
using SocialMedia.Web.Models;
using System.Threading.Tasks;

namespace SocialMedia.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet]
        public IActionResult Login() 
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) 
        {
            var client = _httpClientFactory.CreateClient("SocialMediaApi");
            var response = await client.PostAsJsonAsync("Auth/login", model);
            if (response.IsSuccessStatusCode) 
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                HttpContext.Response.Cookies.Append("JwtToken", result.Token, new CookieOptions { HttpOnly = true });
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Kullanıcı Adı veya Şifre hatalı!";
            return View(model);

        }


    }
}
