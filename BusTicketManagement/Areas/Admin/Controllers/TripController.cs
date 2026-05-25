using BusTicketManagement.Data;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TripController : Controller
{
    private readonly AppDbContext _db;

    public TripController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/Trip
    public async Task<IActionResult> Index()
    {
        var trips = await _db.Trips
            .Include(t => t.BusOperator)
            .Include(t => t.Route)
            .OrderByDescending(t => t.DepartureTime)
            .ToListAsync();

        return View(trips);
    }

    // GET: Admin/Trip/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.BusOperators = await _db.BusOperators.ToListAsync();
        ViewBag.Routes = await _db.Routes.ToListAsync();
        return View();
    }

    // POST: Admin/Trip/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Trip model)
    {
        if (ModelState.IsValid)
        {
            model.CreatedAt = DateTime.UtcNow;
            
            // Tự động tạo ghế cho chuyến
            _db.Trips.Add(model);
            await _db.SaveChangesAsync();

            // Tạo ghế (A1-A12, B1-B12, C1-C12)
            var seats = new List<Seat>();
            string[] rows = { "A", "B", "C" };
            for (int row = 0; row < 3; row++)
            {
                for (int col = 1; col <= 12; col++)
                {
                    seats.Add(new Seat
                    {
                        TripId = model.Id,
                        SeatNumber = rows[row] + col,
                        Status = "Available",
                        LockedUntil = null
                    });
                }
            }

            _db.Seats.AddRange(seats);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm chuyến xe và ghế thành công!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.BusOperators = await _db.BusOperators.ToListAsync();
        ViewBag.Routes = await _db.Routes.ToListAsync();
        return View(model);
    }

    // GET: Admin/Trip/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var trip = await _db.Trips.FindAsync(id);
        if (trip == null)
            return NotFound();

        ViewBag.BusOperators = await _db.BusOperators.ToListAsync();
        ViewBag.Routes = await _db.Routes.ToListAsync();
        return View(trip);
    }

    // POST: Admin/Trip/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Trip model)
    {
        if (id != model.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _db.Trips.Update(model);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật chuyến xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Trips.Any(t => t.Id == id))
                    return NotFound();
                throw;
            }
        }

        ViewBag.BusOperators = await _db.BusOperators.ToListAsync();
        ViewBag.Routes = await _db.Routes.ToListAsync();
        return View(model);
    }

    // GET: Admin/Trip/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var trip = await _db.Trips
            .Include(t => t.BusOperator)
            .Include(t => t.Route)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null)
            return NotFound();

        return View(trip);
    }

    // POST: Admin/Trip/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var trip = await _db.Trips.FindAsync(id);
        if (trip != null)
        {
            _db.Trips.Remove(trip);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Xóa chuyến xe thành công!";
        }

        return RedirectToAction(nameof(Index));
    }
}

