using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
namespace OTP.Model
{
    public class NoCardRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [Required]
        public string uID { get; set; }
        [Required]
        public string verify_code { get; set; }
        [Required]
        public int denomination { get; set; }
        [Required]
        public string username { get; set; }
        [Required]
        public string account { get; set; }
        [Required]
        public DateTime request_time { get; set; }
        [Required]
        public bool Finished { get; set; }
    }
}
