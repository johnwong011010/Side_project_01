using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
namespace OTP.Service
{
    public class MachineService
    {
        private readonly IMongoCollection<Model.Machine> _collection;
        public MachineService(IOptionsMonitor<Model.MachineDB> MachineMonitor)
        {
            var client = new MongoClient(MachineMonitor.CurrentValue.ConnectionString);
            var database = client.GetDatabase(MachineMonitor.CurrentValue.DatabaseName);
            _collection = database.GetCollection<Model.Machine>(MachineMonitor.CurrentValue.CollectionName);
        }
        public async Task GetAllMachinea() => await _collection.Find(_ => true).ToListAsync();
    }
}
