using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moonmax.Data;
using Moonmax.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1) Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2) DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3) Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(4);
    });

// 4) MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 5) Build the app
var app = builder.Build();

// 6) Seed default admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ensure database exists
    context.Database.EnsureCreated();

    // Seed admin if none exists
    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            FirstName = "Default",
            LastName = "Admin",
            Email = "admin@moonmax.com",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!"); // default password

        context.Users.Add(admin);
        context.SaveChanges();
    }
}

// 7) Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 8) Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();

app.Run();
