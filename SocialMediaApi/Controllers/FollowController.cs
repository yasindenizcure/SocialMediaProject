using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Domain.Entities;
using SocialMedia.Infrastructure.Data;
using System.Security.Claims;

namespace SocialMediaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FollowController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("{targetUserId}")]
        public async Task<IActionResult> FollowUser(int targetUserId)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (myId == targetUserId) return BadRequest("Kendini Takip Edemezsin");
            var existingFollow = await _context.Follows.FirstOrDefaultAsync(f=>f.FollowerId == myId && f.FollowingId == targetUserId);
            if (existingFollow != null) 
            {
                _context.Follows.Remove(existingFollow);
                await _context.SaveChangesAsync();
                return Ok("Takipten çıkıldı.");
            }
            var follow = new Follow
            {
                FollowerId = myId,
                FollowingId = targetUserId
            };
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            return Ok("Takip edildi. ✅");
        }
        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(int userId) 
        {
            var followers = await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Select(f => new
                {
                    f.Follower.UserId,
                    f.Follower.UserName,
                    f.FollowDate
                }).ToListAsync();
            return Ok(followers);
        }
        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(int userId) 
        {
            var following = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => new
                {
                    f.Following.UserId,
                    f.Following.UserName,
                    f.FollowDate
                }).ToListAsync();
            return Ok(following);
        }
    }
}
