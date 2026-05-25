using BusTicketManagement.Data;
using BusTicketManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PromotionController : Controller
{
    private readonly AppDbContext _db;

    public PromotionController(AppDbContext db)
    {
        _db = db;
    }

    // GET: Admin/Promotion
    public async Task<IActionResult> Index()
    {
        var promotions = await _db.Promotions.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(promotions);
    }

    // GET: Admin/Promotion/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Promotion/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Promotion model)
    {
        if (ModelState.IsValid)
        {
            model.CreatedAt = DateTime.UtcNow;
            _db.Promotions.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm mã khuyến mãi thành công!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // GET: Admin/Promotion/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var promotion = await _db.Promotions.FindAsync(id);
        if (promotion == null)
            return NotFound();

        return View(promotion);
    }

    // POST: Admin/Promotion/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Promotion model)
    {
        if (id != model.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _db.Promotions.Update(model);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật mã khuyến mãi thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Promotions.Any(p => p.Id == id))
                    return NotFound();
                throw;
            }
        }

        return View(model);
    }

    // GET: Admin/Promotion/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var promotion = await _db.Promotions.FindAsync(id);
        if (promotion == null)
            return NotFound();

        return View(promotion);
    }

    // POST: Admin/Promotion/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var promotion = await _db.Promotions.FindAsync(id);
        if (promotion != null)
        {
            _db.Promotions.Remove(promotion);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Xóa mã khuyến mãi thành công!";
        }

        return RedirectToAction(nameof(Index));
    }
}

