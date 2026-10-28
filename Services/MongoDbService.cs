using MongoDB.Driver;
using Aashirwad.Models;

namespace Aashirwad.Services
{
    /// <summary>
    /// MongoDB database service - equivalent to Python's modules/db.py
    /// Handles all MongoDB collection references for the Smart Hospital Management System
    /// </summary>
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDB");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'MongoDB' is not configured.");
            }

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("HMS");

            // Test connection asynchronously (equivalent to Python's ping without blocking startup)
            Task.Run(async () =>
            {
                try
                {
                    await client.GetDatabase("admin").RunCommandAsync<MongoDB.Bson.BsonDocument>(
                        new MongoDB.Bson.BsonDocument("ping", 1));
                    Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MongoDB connection note: {ex.Message}");
                }
            });
        }

        // All collections — equivalent to Python module-level variables in db.py
        public IMongoCollection<Patient> Patients =>
            _database.GetCollection<Patient>("patients");

        public IMongoCollection<Doctor> Doctors =>
            _database.GetCollection<Doctor>("doctors");

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("users");

        public IMongoCollection<Admin> Admins =>
            _database.GetCollection<Admin>("admin");

        public IMongoCollection<Appointment> Appointments =>
            _database.GetCollection<Appointment>("appointment");

        public IMongoCollection<Contact> Contacts =>
            _database.GetCollection<Contact>("contact");

        public IMongoCollection<SuperAdmin> SuperAdmins =>
            _database.GetCollection<SuperAdmin>("Superadmin");

        public IMongoCollection<HospitalData> HospitalData =>
            _database.GetCollection<HospitalData>("hospital_data");

        public IMongoCollection<DischargedPatient> HospitalDischarge =>
            _database.GetCollection<DischargedPatient>("discharged");

        public IMongoCollection<InventoryOrder> Inventory =>
            _database.GetCollection<InventoryOrder>("inventory");

        public IMongoCollection<MongoDB.Bson.BsonDocument> Stock =>
            _database.GetCollection<MongoDB.Bson.BsonDocument>("stock");

        public IMongoCollection<Feedback> Feedback =>
            _database.GetCollection<Feedback>("feedback");

        public IMongoCollection<MongoDB.Bson.BsonDocument> AdminFeedback =>
            _database.GetCollection<MongoDB.Bson.BsonDocument>("admin_feedback");
    }
}
