using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OTP.Model;
namespace OTP.Service
{
    public class LoginService
    {
        private readonly IMongoCollection<User> _collection;
        public LoginService(IOptionsMonitor<UserDB> UserMonitor)
        {
            var client = new MongoClient(UserMonitor.CurrentValue.ConnectionString);
            var database = client.GetDatabase(UserMonitor.CurrentValue.DatabaseName);
            _collection = database.GetCollection<User>(UserMonitor.CurrentValue.CollectionName);
        }
        public async Task<User?> GetUser(string Username, string Password) => await _collection.Find(x => x.Username == Username && x.Password == Password).FirstOrDefaultAsync(); 
        public async Task AddUser(string Username, string Password) => await _collection.InsertOneAsync(new User { Username = Username, Password = Password, isDeleted=false });
        public async Task DeleteUser(string Username, string Password) => await _collection.UpdateOneAsync(x => x.Username == Username && x.Password == Password, Builders<User>.Update.Set("isDeleted", true));
        public async Task WriteSecret(string Username, string Password, string Secret) =>
            await _collection.UpdateOneAsync(x => x.Username == Username && x.Password == Password, Builders<User>.Update.Set("Secret", Secret));
        public async Task<string?> GetSecret(string Username, string Password)
        {
            var user = await _collection.Find(x => x.Username == Username && x.Password == Password).FirstOrDefaultAsync();
            return user?.Secret;
        }
    }
}
