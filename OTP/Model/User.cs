using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OTP.Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public bool isDeleted { get; set; }
        public string? Secret { get; set; }
        public RefreshToken? RefreshToken { get; set; }
    }
}
