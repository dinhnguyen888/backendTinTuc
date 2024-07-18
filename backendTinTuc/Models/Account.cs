using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Account
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("Email")]
    public string Email { get; set; }

    [BsonElement("Password")]
    public string Password { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; }

    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("Token")]
    public DateTime Token { get; set; }

}
