using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusTicketManagement.Models;
using Microsoft.IdentityModel.Tokens;
 
namespace BusTicketManagement.Helpers;
 
public class JwtHelper
{
    private readonly IConfiguration _config;
 
    public JwtHelper(IConfiguration config) => _config = config;
 
        public string GenerateToken(User user)
        {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
 
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, string.IsNullOrEmpty(user.Role) ? "Customer" : user.Role)
        };
 
        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
 
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}