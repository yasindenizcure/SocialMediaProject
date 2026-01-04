using Microsoft.AspNetCore.Mvc;
using SocialMedia.Application.DTO;

namespace SocialMedia.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PostController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task <IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("SocialMediaApi");
            var token = Request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("Post");
            if (response.IsSuccessStatusCode) 
            {
                var posts = await response.Content.ReadFromJsonAsync<List<PostResponseDto>>();
                return View(posts);
            }
            return View(new List<PostResponseDto>());
        }
    }
}
