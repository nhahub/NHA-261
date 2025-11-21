using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------- DATABASE --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- IDENTITY --------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// -------------------- SESSION SUPPORT (ADD THIS) --------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Cart expires after 30 mins
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Important for GDPR
    options.Cookie.Name = "StockUpCart";
});

// -------------------- REPOSITORIES --------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// -------------------- CONTROLLERS & VIEWS --------------------
builder.Services.AddControllersWithViews();

// -------------------- FIX: IActionContextAccessor --------------------
builder.Services.AddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.IActionContextAccessor, Microsoft.AspNetCore.Mvc.Infrastructure.ActionContextAccessor>();

var app = builder.Build();

// -------------------- MIDDLEWARE --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// -------------------- ADD SESSION MIDDLEWARE (ADD THIS) --------------------
app.UseSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// -------------------- ROUTING --------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// -------------------- SEED DEFAULT ADMIN --------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure roles exist
    string[] roles = { "Admin", "Staff", "Viewer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int>(role));
    }

    // Create default admin if not exists
    string adminEmail = "hazemhamdyhafez@gmail.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            FullName = "Hazem Hamdy",
            UserName = adminEmail,
            Email = adminEmail
        };
        await userManager.CreateAsync(admin, "Hazem123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
    // Create default staff if not exists
    string staffEmail = "moaaz@stockup.com";
    if (await userManager.FindByEmailAsync(staffEmail) == null)
    {
        var staff = new ApplicationUser
        {
            FullName = "Moaaz Magdy",
            UserName = staffEmail,
            Email = staffEmail
        };
        await userManager.CreateAsync(staff, "staff123!");
        await userManager.AddToRoleAsync(staff, "Staff");
    }
}

app.Run();