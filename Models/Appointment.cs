using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aashirwad.Models
{
    [BsonIgnoreExtraElements]
    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("number")]
        public string Number { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("appointment_date")]
        public string AppointmentDate { get; set; } = string.Empty;

        [BsonElement("time_slot")]
        public string TimeSlot { get; set; } = string.Empty;

        [BsonElement("speciality")]
        public string Speciality { get; set; } = string.Empty;

        [BsonElement("disease_description")]
        public string DiseaseDescription { get; set; } = string.Empty;

        [BsonElement("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;

        [BsonElement("queue_number")]
        public int QueueNumber { get; set; }

        [BsonElement("appointed_doc")]
        public string AppointedDoc { get; set; } = string.Empty;

        [BsonElement("status")]
        public string Status { get; set; } = "Pending";
    }
}
