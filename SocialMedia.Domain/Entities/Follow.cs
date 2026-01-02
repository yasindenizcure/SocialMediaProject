using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Domain.Entities
{
    public class Follow
    {
        public int FollowerId { get; set; }
        public User Follower { get; set; } = null!;
        public int FollowingId { get; set; }
        public User Following { get; set; } = null!;
        public DateTime FollowDate { get; set; } = DateTime.UtcNow;
    }
}
