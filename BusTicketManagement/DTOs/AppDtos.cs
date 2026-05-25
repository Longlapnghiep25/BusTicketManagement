namespace BusTicketManagement.DTOs;
 
// ── Auth ──────────────────────────────────────────────
public record RegisterRequest(string Email, string Password, string FullName, string? PhoneNumber = null);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string FullName, string Email, int Points, string Rank);
 
// ── Trip Search ───────────────────────────────────────
public record TripSearchRequest(string From, string To, DateTime Date);
 
public record TripDto(
    int Id,
    string From,
    string To,
    DateTime DepartureTime,
    double Price,
    string BusCompany,
    int AvailableSeats
);
 
// ── Seats ─────────────────────────────────────────────
public record SeatDto(int Id, string SeatNumber, string Status);
 
public record LockSeatRequest(int TripId, string SeatNumber);
 
// ── Tickets ───────────────────────────────────────────
public record CreateTicketRequest(int TripId, string SeatNumber);

// ── Orders ───────────────────────────────────────────
public record CreateOrderRequest(int TripId, string[] Seats, string? PromotionCode = null, bool UsePoints = false);
public record ConfirmOrderRequest(int OrderId);
