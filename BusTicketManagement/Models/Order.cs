namespace BusTicketManagement.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public double TotalAmount { get; set; }
    public double DiscountAmount { get; set; } = 0;
    public double FinalAmount { get; set; }
    // "Pending" | "Paid" | "Cancelled" | "Completed"
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public int? PromotionId { get; set; }
    
    // Navigation
    public User? User { get; set; }
    public Promotion? Promotion { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

