using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace backendTinTuc.Models
{
    public class UserCommentDetails
    {
        [BsonElement("FromUserId")]
        public string FromUserId { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("ToUserId")]
        public string? ToUserId { get; set; }

        [BsonElement("CreateAt")]
        public DateTime CreateAt { get; set; }
    }

    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Comments")]
        public List<UserCommentDetails>? Comments { get; set; } = new List<UserCommentDetails>();
    }
}
