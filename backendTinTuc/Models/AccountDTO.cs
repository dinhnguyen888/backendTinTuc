using MongoDB.Bson.Serialization.Attributes;

namespace backendTinTuc.Models
{
    public class AccountDTO
    {
        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }
    }
}
