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
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    PostId = p.PostId,
                    Content = p.Content,
                    UserName = p.User.UserName,
                    CreatedAt = p.CreatedAt,
                    LikeCount = p.Likes.Count()
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

            if (post.UserId != userId) return Forbid("Sadece kendi postunu silebilirsin kanka!");

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok("Post uçuruldu.");
        }
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int id, CreatePostDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound("Güncellenecek post bulunamadı.");

            if (post.UserId != userId) return Forbid("Başkasının postunu değiştiremezsin!");

            post.Content = request.Content;

            await _context.SaveChangesAsync();
            return Ok("Post başarıyla güncellendi.");
        }
        [HttpGet("my-posts")]
        [Authorize]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var myPosts = await _context.Posts.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt).Select(p => new PostResponseDto
            {
                PostId = p.PostId,
                Content = p.Content,
                UserName = p.User.UserName,
                CreatedAt = p.CreatedAt,
                LikeCount = p.Likes.Count()
            }).ToListAsync();
            return Ok(myPosts);
        }
        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var post = await _context.Posts.AnyAsync(p => p.PostId == postId);
            if (!post) return NotFound("Beğenilmesini istediğin post bulunamadı.");
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok("Beğeni geri çekildi.");
            }
            var newLike = new Like
            {
                PostId = postId,
                UserId = userId,
            };
            _context.Likes.Add(newLike);
            await _context.SaveChangesAsync();
            return Ok("Postu beğendiniz. ❤️");
        }
        [HttpPost("{postId}/comment")]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CreateCommentDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var postExist = await _context.Posts.AnyAsync(p=>p.PostId == postId);
            if (!postExist) return NotFound("Yorum yapmak istediğiniz post bulunamadı.");
            var comment = new Comment
            {
                Content = request.Content,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok("Yorumun başarıyla eklendi! 💬");
        }
        [HttpGet("{postId}/comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int postId) 
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.CommentId,
                    c.UserId,
                    c.CreatedAt,
                    UserName = c.User != null ? c.User.UserName : "Anonim",
                    c.Content
                })
            .ToListAsync();
            return Ok(comments);
        }
        [HttpDelete("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId) 
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var comment = await _context.Comments.Include(c=>c.Post).FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null) return NotFound("Yorum Bulunamadı.");
            if (comment.UserId == userId || comment.Post.UserId == userId)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok("Yorum başarıyla silindi.");
                
            }
            return StatusCode(403, "Bu yorumu silme yetkiniz yok; ne yorum sizin ne de post sahibi sizsiniz.");
        }
        [HttpPut("comments/{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CreateCommentDto request) 
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var comment = await _context.Comments.FirstOrDefaultAsync(c=>c.CommentId == commentId);
            if (comment == null) return NotFound("Yorum Bulunamadı.");
            if (comment.UserId != userId) return StatusCode(403, "Bu yorumu güncelleme yetkiniz yok!");
            comment.Content = request.Content;
            await _context.SaveChangesAsync();
            return Ok("Yorum Güncellendi. ✅");
        }
    }
}