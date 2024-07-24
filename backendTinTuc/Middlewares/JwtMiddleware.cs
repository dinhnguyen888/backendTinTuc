using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly MongoDbContext _context;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration, MongoDbContext context, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }
    
    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            await AttachAccountToContext(context, token);
        }

        await _next(context);
    }

    private async Task AttachAccountToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

            var collection = _context.GetCollection<Account>("Account");
            var account = await collection.Find(x => x.Id == accountId).FirstOrDefaultAsync();

            context.Items["Account"] = account;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError($"Security token validation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error attaching account to context: {ex.Message}");
        }
    }

}
