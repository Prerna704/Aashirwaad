using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aashirwad.Models
{
    [BsonIgnoreExtraElements]
    public class Patient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("dob")]
        public string Dob { get; set; } = string.Empty;

        [BsonElement("gender")]
        public string Gender { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("aadhaar")]
        public string Aadhaar { get; set; } = string.Empty;

        [BsonElement("bed_type")]
        public string BedType { get; set; } = string.Empty;

        [BsonElement("bed no")]
        public string BedNo { get; set; } = string.Empty;

        [BsonElement("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;
    }
}
