using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LargeDatasetProject.Models;

public class StatusModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string? UId { get; set; }

    public string? Status { get; set; }

    public int TotalBatches { get; set;}

    public List<Batch> Batches { get; set; } = [];

    public class Batch {
        public string? BId { get; set; }

        public string? BatchStatus { get; set; }

        public int BatchStart { get; set; }

        public int BatchEnd { get; set; }
    }
}