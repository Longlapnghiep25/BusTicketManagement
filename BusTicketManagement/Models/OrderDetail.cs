namespace BusTicketManagement.Models;

public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int TripId { get; set; }
    public string SeatNumber { get; set; } = "";
    public double Price { get; set; }

    // Navigation
    public Order? Order { get; set; }
    public Trip? Trip { get; set; }
}
