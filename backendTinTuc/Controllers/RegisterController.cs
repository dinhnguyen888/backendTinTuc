using backendTinTuc.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class RegisterController : Controller
{
    private readonly AccountRepository _accountRepository;

    public RegisterController(AccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AccountRegistrationDto accountDto)
    {
        if (accountDto == null || string.IsNullOrEmpty(accountDto.Email) || string.IsNullOrEmpty(accountDto.Password))
        {
            return BadRequest("Invalid account data.");
        }

        var existingAccount = await _accountRepository.GetByIdAsync(accountDto.Email);
        if (existingAccount == null)
        {
            return BadRequest("Account already exits.");
        }

        var newAccount = new Account
        {
            Email = accountDto.Email,
            Password = accountDto.Password,
            Name = accountDto.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _accountRepository.CreateAsync(newAccount);
        return Ok(new { Message = "Account registered successfully." });
    }
}
