using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OTP.Interface;
namespace OTP.Model
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IMongoCollection<User> _collection;
        public RefreshTokenRepository(IOptionsMonitor<UserDB> monitor)
        {
            var client = new MongoClient(monitor.CurrentValue.ConnectionString);
            var database = client.GetDatabase(monitor.CurrentValue.DatabaseName);
            _collection = database.GetCollection<User>(monitor.CurrentValue.CollectionName);
        }
        public async Task<RefreshToken?> GetRefreshToken(string id)
        {
            var user = await _collection.Find(x => x.RefreshToken.Value.Id == id).FirstOrDefaultAsync();
            return user.RefreshToken;
        }
        public async Task AddRefreshToken(string id, RefreshToken token)
        {
            var user = await _collection.Find(x => x._id == id).FirstOrDefaultAsync();
            user.RefreshToken = token;
            await _collection.ReplaceOneAsync(x => x._id == id, user);
        }
        public async Task UpdateRefreshToken(string id, RefreshToken token)
        {
            var user = await _collection.Find(x => x._id == id).FirstOrDefaultAsync();
            user.RefreshToken = token;
            await _collection.ReplaceOneAsync(x => x._id == id, user);
        }
        public async Task RevokeRefreshToken(string id)
        {
            var user = await _collection.Find(x => x._id == id).FirstOrDefaultAsync();
            user.RefreshToken = null;
            await _collection.ReplaceOneAsync(x => x._id == id, user);
        }
    }
    public class AuthResponse
    {
        public string Username { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
    public class RefreshRequest
    {
        public string id { get; set; } = null!;
        public string Token { get; set; } = null!;
        public RefreshToken RefreshToken { get; set; } = null!;
    }
}
