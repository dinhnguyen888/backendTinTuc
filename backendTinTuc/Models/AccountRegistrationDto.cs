using MongoDB.Bson.Serialization.Attributes;

namespace backendTinTuc.Models
{
    public class AccountRegistrationDto
    {
        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Roles")]
        public string Roles { get; set; }

    }
}
