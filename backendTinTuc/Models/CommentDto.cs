using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public class CommentDTO
{
    public string NewsId { get; set; }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CommentId { get; set; }
    public string FromUserId { get; set; }
    public string? ToUserId { get; set; }
    public string Content { get; set; }
}
