using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aashirwad.Models
{
    [BsonIgnoreExtraElements]
    public class Doctor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("specialization")]
        public string Specialization { get; set; } = string.Empty;

        [BsonElement("qualification")]
        public string Qualification { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("aadhar")]
        public string Aadhar { get; set; } = string.Empty;

        [BsonElement("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;
    }
}
