using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Application.DTO;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;
using System.Security.Claims;

namespace SocialMediaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    PostId = p.PostId,
                    Content = p.Content,
                    UserName = p.User.UserName,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost(CreatePostDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Kimlik kartın geçersiz!");

            var post = new Post
            {
                Content = request.Content,
                UserId = int.Parse(userIdClaim),
                CreatedAt = DateTime.Now
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok("Post başarıyla paylaşıldı!");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound("Post bulunamadı.");

            // Yetki kontrolü: Post benim mi?
            if (post.UserId != userId) return Forbid("Sadece kendi postunu silebilirsin kanka!");

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok("Post uçuruldu.");
        }
    }
}