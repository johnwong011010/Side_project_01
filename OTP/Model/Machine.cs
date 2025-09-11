using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OTP.Model
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string machineCode { get; set; }
        public string machineName { get; set; }
        public string location { get; set; }
        public List<int> denominations { get; set; }
    }
}
