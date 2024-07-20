using backendTinTuc.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly MongoDbContext _context;

    public AccountController(MongoDbContext context)
    {
        _context = context;
    }

    // Lấy thông tin tất cả các account
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")] // Hoặc [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Account>>> GetAllAccounts()
    {
        var accounts = await _context.GetCollection<Account>("Account").Find(_ => true).ToListAsync();
        return Ok(accounts);
    }

    // Lấy thông tin của một account cụ thể
    [HttpGet("{id:length(24)}", Name = "GetAccount")]
    [Authorize(Roles = "Admin, User")]
    public async Task<ActionResult<Account>> GetAccount(string id)
    {
        var account = await _context.GetCollection<Account>("Account").Find(a => a.Id == id).FirstOrDefaultAsync();
        if (account == null)
        {
            return NotFound();
        }
        return Ok(account);
    }

    // Cập nhật thông tin của một account
    [HttpPut("{id:length(24)}")]
    [Authorize(Roles = "Admin, User")]
    public async Task<IActionResult> UpdateAccount(string id, [FromBody] Account updatedAccount)
    {
        var collection = _context.GetCollection<Account>("Account");
        var account = await collection.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (account == null)
        {
            return NotFound();
        }

        // chỉ chủ account hoặc admin mới có thể sửa account 
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.Name)?.Value;

        if (userRole != "Admin" && userId != account.Id)
        {
            return Forbid();
        }

        updatedAccount.Id = account.Id; //đảm bảo id acc không thay đổi
        await collection.ReplaceOneAsync(a => a.Id == id, updatedAccount);
        return NoContent();
    }

    // Xóa một account nào đó
    [HttpDelete("{id:length(24)}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        var collection = _context.GetCollection<Account>("Account");
        var account = await collection.Find(a => a.Id == id).FirstOrDefaultAsync();
        if (account == null)
        {
            return NotFound();
        }

        await collection.DeleteOneAsync(a => a.Id == id);
        return NoContent();
    }
}
