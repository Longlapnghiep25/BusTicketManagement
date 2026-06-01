using System.Text;
using BusTicketManagement.Data;
using BusTicketManagement.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
 
var builder = WebApplication.CreateBuilder(args);
 
// ── Database ────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection"))));
 
// ── Auth: Cookie (Admin Web) + JWT Bearer (API) ─────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
 
builder.Services.AddAuthentication(opt =>
{
    // Default for MVC pages = Cookie
    opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(opt =>
{
    opt.LoginPath  = "/Admin/Account/Login";
    opt.AccessDeniedPath = "/Admin/Account/AccessDenied";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
 
// API controllers use JWT, MVC uses Cookie — handled via [Authorize] on each Area
builder.Services.AddAuthorization();
 
// ── MVC + API ───────────────────────────────────────────
builder.Services.AddControllersWithViews();
 
// ── Helpers / Services ──────────────────────────────────
builder.Services.AddScoped<JwtHelper>();
 
// ── Swagger (dev only) ──────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 
var app = builder.Build();
 
// ── Middleware ──────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
 
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Seed default admin in Development if none exists
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Now safe to check for existing admin
        if (!db.Users.Any(u => u.Role == "Admin"))
        {
            var admin = new BusTicketManagement.Models.User
            {
                Email = "admin@localhost",
                FullName = "Administrator",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                Rank = "Gold",
                Points = 0
            };
            db.Users.Add(admin);
            db.SaveChanges();
        }
    }
}

// ── Routes ──────────────────────────────────────────────
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Make Admin Dashboard the default landing area (redirect root to Admin/Dashboard)
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Admin}/{controller=Dashboard}/{action=Index}/{id?}");
 
app.Run();