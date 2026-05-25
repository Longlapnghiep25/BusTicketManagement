using BusTicketManagement.Data;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BusOperatorController : Controller
{
    private readonly AppDbContext _db;

    public BusOperatorController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/BusOperator
    public async Task<IActionResult> Index()
    {
        var operators = await _db.BusOperators.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(operators);
    }

    // GET: Admin/BusOperator/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/BusOperator/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BusOperator model)
    {
        if (ModelState.IsValid)
        {
            model.CreatedAt = DateTime.UtcNow;
            _db.BusOperators.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm nhà xe thành công!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: Admin/BusOperator/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var busOperator = await _db.BusOperators.FindAsync(id);
        if (busOperator == null)
            return NotFound();

        return View(busOperator);
    }

    // POST: Admin/BusOperator/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BusOperator model)
    {
        if (id != model.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _db.BusOperators.Update(model);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật nhà xe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.BusOperators.Any(b => b.Id == id))
                    return NotFound();
                throw;
            }
        }

        return View(model);
    }

    // GET: Admin/BusOperator/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var busOperator = await _db.BusOperators.FindAsync(id);
        if (busOperator == null)
            return NotFound();

        return View(busOperator);
    }

    // POST: Admin/BusOperator/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var busOperator = await _db.BusOperators.FindAsync(id);
        if (busOperator != null)
        {
            _db.BusOperators.Remove(busOperator);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Xóa nhà xe thành công!";
        }

        return RedirectToAction(nameof(Index));
    }
}

