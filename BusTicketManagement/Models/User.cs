namespace BusTicketManagement.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Password { get; set; } = ""; // Stored as BCrypt hash
        public string FullName { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public int Points { get; set; } = 0;
        // "Bronze" | "Silver" | "Gold"
        public string Rank { get; set; } = "Bronze";
            // Role: "Customer" | "Admin"
            public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}