namespace BusTicketManagement.Models;

public class BusOperator
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Hotline { get; set; } = "";
    public string Policy { get; set; } = ""; // Chính sách của nhà xe
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}

