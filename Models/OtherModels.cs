using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aashirwad.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("number")]
        public string Number { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements]
    public class Admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;

        [BsonElement("hospital_mail")]
        public string HospitalMail { get; set; } = string.Empty;

        [BsonElement("hospital_password")]
        public string HospitalPassword { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements]
    public class SuperAdmin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements]
    public class Contact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("number")]
        public string Number { get; set; } = string.Empty;

        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("nearest_hospital")]
        public string NearestHospital { get; set; } = string.Empty;

        [BsonElement("subject")]
        public string Subject { get; set; } = string.Empty;

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements]
    public class Feedback
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("nearest_hospital")]
        public string NearestHospital { get; set; } = string.Empty;

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements]
    public class InventoryOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("medicine_name")]
        public string MedicineName { get; set; } = string.Empty;

        [BsonElement("medicine_composition")]
        public string MedicineComposition { get; set; } = string.Empty;

        [BsonElement("medicine_quantity")]
        public int MedicineQuantity { get; set; }

        [BsonElement("order_comment")]
        public string OrderComment { get; set; } = string.Empty;

        [BsonElement("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;
    }

    [BsonIgnoreExtraElements]
    public class DischargedPatient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Patient ID")]
        public string PatientId { get; set; } = string.Empty;

        [BsonElement("Patient Name")]
        public string PatientName { get; set; } = string.Empty;

        [BsonElement("Admission Date")]
        public string AdmissionDate { get; set; } = string.Empty;

        [BsonElement("Discharge Date")]
        public string DischargeDate { get; set; } = string.Empty;

        [BsonElement("Diagnosis")]
        public string Diagnosis { get; set; } = string.Empty;

        [BsonElement("Treatment")]
        public string Treatment { get; set; } = string.Empty;

        [BsonElement("Doctor Name")]
        public string DoctorName { get; set; } = string.Empty;

        [BsonElement("Discharge Summary")]
        public string DischargeSummary { get; set; } = string.Empty;

        [BsonElement("Follow-Up Instructions")]
        public string FollowUpInstructions { get; set; } = string.Empty;

        [BsonElement("Medications")]
        public string Medications { get; set; } = string.Empty;

        [BsonElement("Contact Info")]
        public string ContactInfo { get; set; } = string.Empty;

        [BsonElement("Gender")]
        public string Gender { get; set; } = string.Empty;

        [BsonElement("Address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("Bed Type")]
        public string BedType { get; set; } = string.Empty;
    }
}
