using System.ComponentModel.DataAnnotations;

namespace BusTicketManagement.Models;
 
public class Seat
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string SeatNumber { get; set; } = "";
    // "Available" | "Locked" | "Sold"
    public string Status { get; set; } = "Available";
    public DateTime? LockedUntil { get; set; }
    public int? LockedByUserId { get; set; }
 
    // Navigation
    public Trip? Trip { get; set; }
}