using BusTicketManagement.Data;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrderController : Controller
{
    private readonly AppDbContext _db;

    public OrderController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/Order
    public async Task<IActionResult> Index(string searchTerm = "")
    {
        IQueryable<Order> query = _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(o =>
                o.Id.ToString().Contains(searchTerm) ||
                o.User.Email.Contains(searchTerm) ||
                o.User.PhoneNumber.Contains(searchTerm)
            );
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        ViewBag.SearchTerm = searchTerm;
        return View(orders);
    }

    // GET: Admin/Order/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Promotion)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Trip)
                    .ThenInclude(t => t.Route)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Trip)
                    .ThenInclude(t => t.BusOperator)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return View(order);
    }

    // POST: Admin/Order/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        if (order.Status == "Cancelled")
        {
            TempData["Error"] = "Đơn hàng này đã bị hủy trước đó.";
        }
        else
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                order.Status = "Cancelled";

                // Unlock seats or cancel associated tickets
                foreach (var od in order.OrderDetails)
                {
                    var seat = await _db.Seats.FirstOrDefaultAsync(s => 
                        s.TripId == od.TripId && s.SeatNumber == od.SeatNumber);
                    
                    if (seat != null && (seat.Status == "Locked" || seat.Status == "Sold"))
                    {
                        seat.Status = "Available";
                        seat.LockedUntil = null;
                        seat.LockedByUserId = null;
                    }

                    var ticket = await _db.Tickets.FirstOrDefaultAsync(t => 
                        t.TripId == od.TripId && t.SeatNumber == od.SeatNumber && t.UserId == order.UserId);
                    
                    if (ticket != null)
                    {
                        ticket.Status = "Cancelled";
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                TempData["Success"] = "Hủy đơn hàng thành công.";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Có lỗi xảy ra khi hủy đơn hàng.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
