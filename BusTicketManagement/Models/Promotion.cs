namespace BusTicketManagement.Models;

public class Promotion
{
    public int Id { get; set; }
    public string Code { get; set; } = ""; // Mã voucher (VD: SUMMER2024)
    public string Description { get; set; } = "";
    public double DiscountAmount { get; set; } = 0; // Số tiền giảm (VND)
    public double DiscountPercent { get; set; } = 0; // Hoặc % giảm (%)
    public double MinOrderAmount { get; set; } = 0; // Đơn hàng tối thiểu
    public int UsageLimit { get; set; } = -1; // -1 = vô hạn
    public int UsedCount { get; set; } = 0;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

