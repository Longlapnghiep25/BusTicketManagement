using BusTicketManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/Dashboard
    public async Task<IActionResult> Index()
    {
        // AD-01: Thẻ thống kê tổng quát
        var totalRevenue = await _db.Orders
            .Where(o => o.Status == "Paid")
            .SumAsync(o => o.FinalAmount);

        var totalTicketsSold = await _db.Tickets.CountAsync();

        var newCustomers = await _db.Users
            .Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .CountAsync();

        var totalBusOperators = await _db.BusOperators.CountAsync();

        // AD-02: Xu hướng đặt vé trong 30 ngày gần nhất (dữ liệu cho biểu đồ)
        // Fix: EF Core cannot translate .ToString("yyyy-MM-dd") inside a query.
        // We fetch the raw data and format it in memory.
        var rawTrendData = await _db.Orders
            .Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                DateValue = g.Key,
                Count = g.Count(),
                Revenue = g.Where(o => o.Status == "Paid").Sum(o => o.FinalAmount)
            })
            .OrderBy(x => x.DateValue)
            .ToListAsync();

        var bookingTrend = rawTrendData.Select(x => new
        {
            Date = x.DateValue.ToString("yyyy-MM-dd"),
            x.Count,
            x.Revenue
        }).ToList();

        // AD-03: Top 10 đơn hàng mới nhất chờ xử lý
        var pendingOrders = await _db.Orders
            .Where(o => o.Status == "Pending")
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Trip)
                    .ThenInclude(t => t.Route)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .ToListAsync();

        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.TotalTicketsSold = totalTicketsSold;
        ViewBag.NewCustomers = newCustomers;
        ViewBag.TotalBusOperators = totalBusOperators;
        ViewBag.BookingTrend = bookingTrend;

        return View(pendingOrders);
    }
}
