using BusTicketManagement.Data;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TicketController : Controller
{
    private readonly AppDbContext _db;

    public TicketController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/Ticket
    public async Task<IActionResult> Index(string searchTerm = "")
    {
        IQueryable<Ticket> query = _db.Tickets
            .Include(t => t.User)
            .Include(t => t.Trip)
                .ThenInclude(tr => tr.Route)
            .Include(t => t.Trip)
                .ThenInclude(tr => tr.BusOperator);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(t =>
                t.Id.ToString().Contains(searchTerm) ||
                t.User.Email.Contains(searchTerm) ||
                t.User.PhoneNumber.Contains(searchTerm) ||
                t.SeatNumber.Contains(searchTerm)
            );
        }

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.SearchTerm = searchTerm;

        return View(tickets);
    }

    // GET: Admin/Ticket/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _db.Tickets
            .Include(t => t.User)
            .Include(t => t.Trip)
                .ThenInclude(tr => tr.Route)
            .Include(t => t.Trip)
                .ThenInclude(tr => tr.BusOperator)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
            return NotFound();

        return View(ticket);
    }

    // POST: Admin/Ticket/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);

        if (ticket == null)
            return NotFound();

        if (ticket.Status == "Cancelled")
        {
            TempData["Error"] = "Vé này đã bị hủy rồi!";
        }
        else
        {
            ticket.Status = "Cancelled";

            var seat = await _db.Seats.FirstOrDefaultAsync(s =>
                s.TripId == ticket.TripId &&
                s.SeatNumber == ticket.SeatNumber);

            if (seat != null)
            {
                seat.Status = "Available";
                seat.LockedByUserId = null;
                seat.LockedUntil = null;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Hủy vé thành công!";
        }

        return RedirectToAction(nameof(Index));
    }
}
