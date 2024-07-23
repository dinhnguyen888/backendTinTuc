using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class UserReference
{
    [BsonElement("UserId")]
    public string UserId { get; set; }

    [BsonElement("UserName")]
    public string UserName { get; set; }
}

public class UserCommentDetails
{
    [BsonElement("UserComment")]
    public UserReference UserComment { get; set; }

    [BsonElement("Content")]
    public string Content { get; set; }

    [BsonElement("ToUser")]
    public UserReference? ToUser { get; set; }  // set ToUser trở thành optional (trường hợp User comment vào bài đăng của chủ tus)

    [BsonElement("CreateAt")]
    public DateTime CreateAt { get; set; }
}

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("PageNews")]
    public string PageNews { get; set; }

    [BsonElement("Comments")]
    public List<UserCommentDetails> Comments { get; set; } = new List<UserCommentDetails>();
}
