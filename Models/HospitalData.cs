using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Aashirwad.Models
{
    [BsonIgnoreExtraElements]
    public class HospitalData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("hospital_name")]
        public string HospitalName { get; set; } = string.Empty;

        [BsonElement("hospital_id")]
        public string HospitalId { get; set; } = string.Empty;

        [BsonElement("address_line1")]
        public string AddressLine1 { get; set; } = string.Empty;

        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("state")]
        public string State { get; set; } = string.Empty;

        [BsonElement("postal_code")]
        public string PostalCode { get; set; } = string.Empty;

        [BsonElement("contact_number")]
        public string ContactNumber { get; set; } = string.Empty;

        [BsonElement("emergency_contact_number")]
        public string EmergencyContactNumber { get; set; } = string.Empty;

        [BsonElement("email_address")]
        public string EmailAddress { get; set; } = string.Empty;

        [BsonElement("website_url")]
        public string WebsiteUrl { get; set; } = string.Empty;

        [BsonElement("number_of_general_beds")]
        public int NumberOfGeneralBeds { get; set; }

        [BsonElement("occupied_general")]
        public int OccupiedGeneral { get; set; }

        [BsonElement("number_of_icu_beds")]
        public int NumberOfIcuBeds { get; set; }

        [BsonElement("occupied_icu")]
        public int OccupiedIcu { get; set; }

        [BsonElement("number_of_ventilators")]
        public int NumberOfVentilators { get; set; }

        [BsonElement("occupied_ventilator")]
        public int OccupiedVentilator { get; set; }

        [BsonElement("emergency_department")]
        public string EmergencyDepartment { get; set; } = string.Empty;

        [BsonElement("specialization")]
        public List<string> Specialization { get; set; } = new();

        [BsonElement("hospital_operating_hours")]
        public string HospitalOperatingHours { get; set; } = string.Empty;

        [BsonElement("visiting_hours")]
        public string VisitingHours { get; set; } = string.Empty;

        [BsonElement("pharmacy_on_site")]
        public string PharmacyOnSite { get; set; } = string.Empty;

        [BsonElement("total_number_of_nurses")]
        public int TotalNumberOfNurses { get; set; }

        [BsonElement("administrative_staff_count")]
        public int AdministrativeStaffCount { get; set; }

        [BsonElement("ambulance_services")]
        public string AmbulanceServices { get; set; } = string.Empty;

        [BsonElement("blood_bank")]
        public string BloodBank { get; set; } = string.Empty;

        [BsonElement("diagnostic_services")]
        public string DiagnosticServices { get; set; } = string.Empty;
    }
}
