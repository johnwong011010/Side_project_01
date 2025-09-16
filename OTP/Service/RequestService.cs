using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
namespace OTP.Service
{
    public class RequestService
    {
        private readonly IMongoCollection<Model.NoCardRequest> _collection;
        public RequestService(IOptionsMonitor<Model.RequestDB> monitor)
        {
            var client = new MongoClient(monitor.CurrentValue.ConnectionString);
            var database = client.GetDatabase(monitor.CurrentValue.DatabaseName);
            _collection = database.GetCollection<Model.NoCardRequest>(monitor.CurrentValue.CollectionName);
        }
        public async Task<List<Model.NoCardRequest>> GetAllRequests() => await _collection.Find(_ => true).ToListAsync();
        public async Task<List<Model.NoCardRequest>> GetRequestByUser(string username) => await _collection.Find(x => x.username == username).ToListAsync();
        public async Task<List<Model.NoCardRequest>> GetRequestByAccount(string account) => await _collection.Find(x => x.account == account).ToListAsync();
        public async Task AddRequest(Model.NoCardRequest request) => await _collection.InsertOneAsync(request);
        public async Task ChangeRequestStatus(string id,string code,bool status)
        {
            var request  = await _collection.Find(x => x.Id == id && x.verify_code == code).FirstOrDefaultAsync();
            request.Finished = status;
            await _collection.ReplaceOneAsync(x => x.Id == id && x.verify_code == code, request);
        }
    }
}
