using MongoDB.Bson.Serialization.Attributes;

namespace backendTinTuc.Models
{
    public class News
    {

        [BsonElement("Id")]
        public string Id { get; set; }


        [BsonElement("Source")]
        public Source Source { get; set; }


        [BsonElement("Author")]
        public string Author { get; set; }


        [BsonElement("Title")]
        public string Title { get; set; }


        [BsonElement("Description")]
        public string Description { get; set; }


        [BsonElement("Url")]
        public string Url { get; set; }


        [BsonElement("UrlToImage")]
        public string UrlToImage { get; set; }


        [BsonElement("PublishedAt")]
        public DateTime PublishedAt { get; set; }


        [BsonElement("Content")]
        public string Content { get; set; }
    }

    public class Source
    {

        [BsonElement("Id")]
        public string Id { get; set; }


        [BsonElement("Name")]
        public string Name { get; set; }
    }
}
