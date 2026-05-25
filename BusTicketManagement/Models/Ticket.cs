namespace BusTicketManagement.Models;
 
public class Ticket
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TripId { get; set; }
    public string SeatNumber { get; set; } = "";
    // "Active" | "Cancelled" | "Used"
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 
    // Navigation
    public User? User { get; set; }
    public Trip? Trip { get; set; }
}