using System.Security.Claims;
using BusTicketManagement.Data;
using BusTicketManagement.DTOs;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    // POST api/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(new { message = "User not identified." });

        var userId = int.Parse(userIdClaim);

        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == req.TripId);
        if (trip is null)
            return NotFound(new { message = "Chuyến xe không tồn tại." });

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var seats = new List<Seat>();
            foreach (var seatNumber in req.Seats)
            {
                var seat = await _db.Seats
                    .Where(s => s.TripId == req.TripId && s.SeatNumber == seatNumber)
                    .FirstOrDefaultAsync();

                if (seat is null)
                    return NotFound(new { message = $"Ghế {seatNumber} không tồn tại." });

                // Check and release expired lock
                if (seat.Status == "Locked" && seat.LockedUntil < DateTime.UtcNow)
                {
                    seat.Status = "Available";
                    seat.LockedUntil = null;
                    seat.LockedByUserId = null;
                }

                if (seat.Status == "Sold")
                    return Conflict(new { message = $"Ghế {seatNumber} đã được bán." });

                if (seat.Status == "Locked" && seat.LockedByUserId != userId)
                    return Conflict(new { message = $"Ghế {seatNumber} đang được giữ bởi người dùng khác." });

                // Lock the seat
                seat.Status = "Locked";
                seat.LockedUntil = DateTime.UtcNow.AddMinutes(10);
                seat.LockedByUserId = userId;

                seats.Add(seat);
            }

            // Calculate amounts
            var total = trip.Price * req.Seats.Length;
            double discount = 0;
            Promotion? promo = null;
            if (!string.IsNullOrEmpty(req.PromotionCode))
            {
                promo = await _db.Promotions.FirstOrDefaultAsync(p => p.Code == req.PromotionCode && p.IsActive);
                if (promo != null && promo.StartDate <= DateTime.UtcNow && promo.EndDate >= DateTime.UtcNow && (promo.UsageLimit == -1 || promo.UsedCount < promo.UsageLimit) && total >= promo.MinOrderAmount)
                {
                    discount = promo.DiscountAmount;
                    if (promo.DiscountPercent > 0)
                        discount = Math.Max(discount, total * (promo.DiscountPercent / 100.0));
                }
            }

            var finalAmount = Math.Max(0, total - discount);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            var pointsUsed = 0;
            if (req.UsePoints)
            {
                pointsUsed = Math.Min(user.Points, (int)Math.Floor(finalAmount));
                finalAmount -= pointsUsed;
                user.Points -= pointsUsed;
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = total,
                DiscountAmount = discount + pointsUsed,
                FinalAmount = finalAmount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                PromotionId = promo?.Id
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var seat in seats)
            {
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    TripId = seat.TripId,
                    SeatNumber = seat.SeatNumber,
                    Price = trip.Price
                });
            }

            if (promo != null) promo.UsedCount += 1;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { orderId = order.Id, finalAmount = order.FinalAmount, status = order.Status });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal Server Error");
        }
    }

    // POST api/orders/{orderId}/confirm
    [HttpPost("{orderId}/confirm")]
    public async Task<IActionResult> ConfirmPayment(int orderId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        var userId = int.Parse(userIdClaim);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null || order.Status != "Pending")
                return BadRequest(new { message = "Đơn hàng không hợp lệ hoặc đã được xử lý." });

            foreach (var od in order.OrderDetails)
            {
                var seat = await _db.Seats.FirstOrDefaultAsync(s => s.TripId == od.TripId && s.SeatNumber == od.SeatNumber);
                if (seat == null || seat.Status != "Locked" || seat.LockedByUserId != userId || (seat.LockedUntil.HasValue && seat.LockedUntil < DateTime.UtcNow))
                {
                    return BadRequest(new { message = $"Ghế {od.SeatNumber} không còn khả dụng." });
                }

                seat.Status = "Sold";
                seat.LockedUntil = null;
                seat.LockedByUserId = null;

                _db.Tickets.Add(new Ticket
                {
                    UserId = userId,
                    TripId = od.TripId,
                    SeatNumber = od.SeatNumber,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.Status = "Paid";
            order.PaidAt = DateTime.UtcNow;

            // Tích điểm: 100k = 1 điểm
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.Points += (int)(order.FinalAmount / 100000);
                // Update rank
                if (user.Points > 1000) user.Rank = "Gold";
                else if (user.Points > 500) user.Rank = "Silver";
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Thanh toán thành công." });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal Server Error");
        }
    }
}
