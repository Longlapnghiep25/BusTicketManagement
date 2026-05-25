using BusTicketManagement.Data;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Route = BusTicketManagement.Models.Route;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RouteController : Controller
{
    private readonly AppDbContext _db;

    public RouteController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/Route
    public async Task<IActionResult> Index()
    {
        var routes = await _db.Routes.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return View(routes);
    }

    // GET: Admin/Route/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Route/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Route model)
    {
        if (ModelState.IsValid)
        {
            model.CreatedAt = DateTime.UtcNow;
            _db.Routes.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm tuyến đường thành công!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: Admin/Route/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null)
            return NotFound();

        return View(route);
    }

    // POST: Admin/Route/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Route model)
    {
        if (id != model.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _db.Routes.Update(model);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật tuyến đường thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Routes.Any(r => r.Id == id))
                    return NotFound();
                throw;
            }
        }

        return View(model);
    }

    // GET: Admin/Route/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null)
            return NotFound();

        return View(route);
    }

    // POST: Admin/Route/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route != null)
        {
            _db.Routes.Remove(route);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Xóa tuyến đường thành công!";
        }

        return RedirectToAction(nameof(Index));
    }
}

