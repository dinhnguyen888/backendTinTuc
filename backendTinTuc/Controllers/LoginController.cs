using backendTinTuc.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace backendTinTuc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public LoginController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountDTO accountDto)
        {
            var collection = _context.GetCollection<Account>("Accounts");
            var filter = Builders<Account>.Filter.Eq(a => a.Email, accountDto.Email) &
                         Builders<Account>.Filter.Eq(a => a.Password, accountDto.Password);

            var account = await collection.Find(filter).FirstOrDefaultAsync();

            if (account == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Nếu cần tạo một token JWT, bạn có thể làm ở đây
            // var token = GenerateJwtToken(account);

            return Ok(account);
        }

        // private string GenerateJwtToken(Account account)
        // {
        //     // Implement JWT token generation logic here
        // }
    }
}
