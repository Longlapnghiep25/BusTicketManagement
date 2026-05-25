namespace BusTicketManagement.Models;

public class Route
{
    public int Id { get; set; }
    public string From { get; set; } = ""; // Điểm đi
    public string To { get; set; } = "";   // Điểm đến
    public double BaseDistance { get; set; } = 0; // Khoảng cách (km) - tùy chọn
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}

