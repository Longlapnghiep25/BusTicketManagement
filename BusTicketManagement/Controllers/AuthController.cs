using System.Security.Claims;
using BusTicketManagement.Data;
using BusTicketManagement.DTOs;
using BusTicketManagement.Helpers;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
 
namespace BusTicketManagement.Controllers;
 
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;
 
    public AuthController(AppDbContext db, JwtHelper jwt)
    {
        _db  = db;
        _jwt = jwt;
    }
 
    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Email đã được sử dụng." });
 
        var user = new User
        {
            Email    = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName,
            PhoneNumber = req.PhoneNumber,
            Points   = 0,
            Rank     = "Bronze",
            Role     = "Customer",
            CreatedAt = DateTime.UtcNow
        };
 
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
 
        return Ok(new { message = "Đăng ký thành công." });
    }
 
    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
 
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });
 
        var token = _jwt.GenerateToken(user);
 
        return Ok(new LoginResponse(token, user.FullName, user.Email, user.Points, user.Rank));
    }

    // GET api/auth/profile
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var user = await _db.Users
            .Select(u => new {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Points,
                u.Rank,
                u.CreatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        return Ok(user);
    }
}
