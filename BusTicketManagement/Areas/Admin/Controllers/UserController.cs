using BusTicketManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/User
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View(users);
    }

    // GET: Admin/User/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound();

        // Lấy lịch sử mua vé của khách
        var tickets = await _db.Tickets
            .Where(t => t.UserId == id)
            .Include(t => t.Trip)
                .ThenInclude(tr => tr.Route)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.Tickets = tickets;

        return View(user);
    }
}

