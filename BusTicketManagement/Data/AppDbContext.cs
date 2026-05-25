using Microsoft.EntityFrameworkCore;
using BusTicketManagement.Models;
using Route = BusTicketManagement.Models.Route;

namespace BusTicketManagement.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
 
    public DbSet<User>         Users         { get; set; }
    public DbSet<BusOperator>  BusOperators  { get; set; }
    public DbSet<Route>        Routes        { get; set; }
    public DbSet<Trip>         Trips         { get; set; }
    public DbSet<Seat>         Seats         { get; set; }
    public DbSet<Ticket>       Tickets       { get; set; }
    public DbSet<Promotion>    Promotions    { get; set; }
    public DbSet<Order>        Orders        { get; set; }
    public DbSet<OrderDetail>  OrderDetails  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Trip Relationships ──────────────────────────
        modelBuilder.Entity<Trip>()
            .HasOne(t => t.BusOperator)
            .WithMany(bo => bo.Trips)
            .HasForeignKey(t => t.BusOperatorId);

        modelBuilder.Entity<Trip>()
            .HasOne(t => t.Route)
            .WithMany(r => r.Trips)
            .HasForeignKey(t => t.RouteId);

        // ── Seat Relationships ──────────────────────────
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Trip)
            .WithMany(t => t.Seats)
            .HasForeignKey(s => s.TripId);

        // ── Ticket Relationships ────────────────────────
        modelBuilder.Entity<Ticket>()
            .HasOne(tk => tk.User)
            .WithMany()
            .HasForeignKey(tk => tk.UserId);

        modelBuilder.Entity<Ticket>()
            .HasOne(tk => tk.Trip)
            .WithMany(t => t.Tickets)
            .HasForeignKey(tk => tk.TripId);

        // ── Order Relationships ─────────────────────────
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Promotion)
            .WithMany()
            .HasForeignKey(o => o.PromotionId)
            .IsRequired(false);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderDetails)
            .WithOne(od => od.Order)
            .HasForeignKey(od => od.OrderId);

        // ── OrderDetail Relationships ───────────────────
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Trip)
            .WithMany()
            .HasForeignKey(od => od.TripId);
    }
}