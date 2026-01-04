namespace SocialMedia.Web.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class TokenResponse
    {
          public string Token { get; set; } = string.Empty;
    }
}
