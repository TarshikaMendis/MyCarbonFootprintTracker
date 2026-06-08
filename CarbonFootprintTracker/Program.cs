using CarbonFootprintTracker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
// Connection string
var connectionString = 
    builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string" +
    " 'DefaultConnection' not found.");
//  Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); 
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Identity configuration (FIXED)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{ options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
// MVC + Razor Pages (Identity needs both)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var app = builder.Build();
// HTTP pipeline
if (app.Environment.IsDevelopment())
{ 
    app.UseMigrationsEndPoint();
}
else
{ 
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
// IMPORTANT ORDER
app.UseAuthentication();
app.UseAuthorization();
// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Identity Razor Pages
app.MapRazorPages();
app.Run();