using MongoDB.Bson.Serialization.Attributes;
namespace Aashirwad.Models;

[BsonIgnoreExtraElements]
    public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
