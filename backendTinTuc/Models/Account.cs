using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.AspNetCore.Http.HttpResults;

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

    [BsonRepresentation(BsonType.String)]
    [BsonElement("Roles")]
    public Roles Roles { get; set; }
}
public enum Roles
{
    User,
    Admin
}
