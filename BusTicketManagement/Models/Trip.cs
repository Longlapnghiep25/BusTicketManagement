namespace BusTicketManagement.Models;
 
public class Trip
{
    public int Id { get; set; }
    public int BusOperatorId { get; set; }
    public int RouteId { get; set; }
    public DateTime DepartureTime { get; set; }
    public double Price { get; set; } // Giá vé cơ bản
    public int TotalSeats { get; set; } = 36; // Số ghế tổng cộng
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 
    // Navigation
    public BusOperator? BusOperator { get; set; }
    public Route? Route { get; set; }
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}