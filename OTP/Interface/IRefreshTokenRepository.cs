namespace OTP.Interface
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetRefreshToken(string id);
        Task AddRefreshToken(string id, RefreshToken token);
        Task UpdateRefreshToken(string id, RefreshToken token);
    }
    public class RefreshToken
    {
        public string Id { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string? CreateIP { get; set; }
        public string? RevokeIP { get; set; }
        public DateTime Expire { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? RevokeTime { get; set; }
        public string ReplaceToken { get; set; } = null!;
        public bool IsExpire => DateTime.UtcNow >= Expire;
        public bool IsRevoke => RevokeTime == null && !IsExpire;
    }
}
