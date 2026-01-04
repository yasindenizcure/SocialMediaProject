using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Infrastructure.Data;

namespace SocialMediaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("profile/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    FollowersCount = u.Followers.Count(),
                    FollowingCount = u.Following.Count(),
                    PostsCount = u.Posts.Count(),
                }).FirstOrDefaultAsync();
            if (user == null) return NotFound("Kullanıcı bulunamadı.");
            return Ok(user);
;        }
    }
}
