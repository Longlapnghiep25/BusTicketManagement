using BusTicketManagement.Data;
using BusTicketManagement.DTOs;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
 
namespace BusTicketManagement.Areas.Api.Controllers;
 
[Area("Api")]
[Route("api/seats")]
[ApiController]
[Authorize] // Requires valid JWT
public class SeatController : ControllerBase
{
    private readonly AppDbContext _db;
    public SeatController(AppDbContext db) => _db = db;
 
    // POST api/seats/lock
    // Khóa ghế trong 10 phút — trả 409 nếu đã có người giữ
    [HttpPost("lock")]
    public async Task<IActionResult> Lock([FromBody] LockSeatRequest req)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
 
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "User not identified." });

            var userId = int.Parse(userIdClaim);

            var seat = await _db.Seats
                .Where(s => s.TripId == req.TripId && s.SeatNumber == req.SeatNumber)
                .FirstOrDefaultAsync();
 
            if (seat is null)
                return NotFound(new { message = "Ghế không tồn tại." });
 
            // Release expired lock
            if (seat.Status == "Locked" && seat.LockedUntil < DateTime.UtcNow)
            {
                seat.Status       = "Available";
                seat.LockedUntil  = null;
                seat.LockedByUserId = null;
            }

            if (seat.Status == "Locked")
            {
                return Conflict(new { message = "Ghế đã được giữ hoặc đã bán." }); // HTTP 409
            }

            // Lock seat for this user
            seat.Status        = "Locked";
            seat.LockedUntil   = DateTime.UtcNow.AddMinutes(10);
            seat.LockedByUserId = userId;
 
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
 
            return Ok(new { message = "Khóa ghế thành công. Vui lòng thanh toán trong 10 phút." });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
 
    // POST api/seats/confirm
    // Tạo vé và chuyển ghế -> Sold (trong 1 transaction)
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] CreateTicketRequest req)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
 
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "User not identified." });

            var userId = int.Parse(userIdClaim);

            var seat = await _db.Seats
                .Where(s => s.TripId == req.TripId && s.SeatNumber == req.SeatNumber)
                .FirstOrDefaultAsync();
 
            if (seat is null)
                return NotFound(new { message = "Ghế không tồn tại." });
 
            if (seat.Status != "Locked")
                return BadRequest(new { message = "Ghế chưa được khóa hoặc đã hết hạn giữ chỗ." });

            // Ensure this user is the locker
            if (seat.LockedByUserId == null || seat.LockedByUserId != userId)
                return Forbid();

            // Tạo vé
            var ticket = new Ticket
            {
                UserId     = userId,
                TripId     = req.TripId,
                SeatNumber = req.SeatNumber,
                CreatedAt  = DateTime.UtcNow
            };
            _db.Tickets.Add(ticket);
 
            // Cập nhật trạng thái ghế -> Sold
            seat.Status      = "Sold";
            seat.LockedUntil = null;
            seat.LockedByUserId = null;
 
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
 
            return Ok(new { message = "Đặt vé thành công!", ticketId = ticket.Id });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}