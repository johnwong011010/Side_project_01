using System.ComponentModel.DataAnnotations;

namespace OTP.Model
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string Detail { get; set; }
        public string Token { get; set; }
        public RefreshToken? RefreshToken { get; set; }
    }
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expire { get; set; }
        public DateTime CreateAt { get; set; }
        public string ReplaceToken { get; set; }
    }
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByToken(string token);
        Task Add(string id, RefreshToken token);
        Task Update(string id, RefreshToken token);
    }
    public class RefreshTokenRequset
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
    public class AuthResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
