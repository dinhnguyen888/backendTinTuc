using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backendTinTuc.Models
{
    public class News
    {

        [BsonElement("Id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }


        public string Title { get; set; }
        public string LinkDetail { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Type { get; set; } // Thêm trường Type để lưu kiểu
    }
}
