using BusTicketManagement.Data;
using BusTicketManagement.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusTicketManagement.Controllers;

[Route("api/trips")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TripsController(AppDbContext db) => _db = db;

    // GET api/trips/search?from=...&to=...&date=yyyy-MM-dd
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string from, [FromQuery] string to, [FromQuery] DateTime date)
    {
        var trips = await _db.Trips
            .Include(t => t.Route)
            .Include(t => t.BusOperator)
            .Include(t => t.Seats)
            .Where(t => t.Route != null && 
                        t.Route.From.Contains(from) && 
                        t.Route.To.Contains(to) && 
                        t.DepartureTime.Date == date.Date)
            .ToListAsync();

        var result = trips.Select(t => new TripDto(
            t.Id,
            t.Route?.From ?? "",
            t.Route?.To ?? "",
            t.DepartureTime,
            t.Price,
            t.BusOperator?.Name ?? "",
            t.Seats.Count(s => s.Status == "Available" || (s.Status == "Locked" && s.LockedUntil < DateTime.UtcNow))
        ));

        return Ok(result);
    }

    // GET api/trips/{tripId}/seats
    [HttpGet("{tripId}/seats")]
    public async Task<IActionResult> GetSeats(int tripId)
    {
        var seats = await _db.Seats
            .Where(s => s.TripId == tripId)
            .Select(s => new SeatDto(
                s.Id, 
                s.SeatNumber, 
                (s.Status == "Locked" && s.LockedUntil < DateTime.UtcNow) ? "Available" : s.Status
            ))
            .ToListAsync();

        return Ok(seats);
    }
}
