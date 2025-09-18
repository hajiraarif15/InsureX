using InSureX.Data;
using InSureX.Models;
using InSureX.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services 

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity with Roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add MVC Controllers with Views
builder.Services.AddControllersWithViews();

// Add custom services
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuditService>();

builder.WebHost.UseUrls("http://localhost:5147");

var app = builder.Build();

//  Middleware 

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// SEED ROLES & DEFAULT USERS 

async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var config = serviceProvider.GetRequiredService<IConfiguration>();

    // Ensure required roles exist
    string[] roles = { "Employee", "CPD", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            logger.LogInformation($"✅ Role '{role}' created.");
        }
    }

    // Admin User
    var adminEmail = config["AdminUser:Email"];
    var adminPassword = config["AdminUser:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("✅ Default Admin user created & assigned 'Admin' role.");
            }
            else
            {
                foreach (var error in result.Errors)
                    logger.LogError($"❌ Admin creation error: {error.Description}");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("ℹ️ Admin user already exists, role assigned.");
            }
            else
            {
                logger.LogInformation("ℹ️ Admin user already exists with correct role.");
            }
        }
    }

    // CPD User 
    var cpdEmail = config["CPDUser:Email"];
    var cpdPassword = config["CPDUser:Password"];

    if (!string.IsNullOrWhiteSpace(cpdEmail) && !string.IsNullOrWhiteSpace(cpdPassword))
    {
        var cpdUser = await userManager.FindByEmailAsync(cpdEmail);
        if (cpdUser == null)
        {
            cpdUser = new ApplicationUser
            {
                UserName = cpdEmail,
                Email = cpdEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(cpdUser, cpdPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(cpdUser, "CPD");
                logger.LogInformation("✅ Default CPD user created & assigned 'CPD' role.");
            }
            else
            {
                foreach (var error in result.Errors)
                    logger.LogError($"❌ CPD creation error: {error.Description}");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(cpdUser, "CPD"))
            {
                await userManager.AddToRoleAsync(cpdUser, "CPD");
                logger.LogInformation("ℹ️ CPD user already exists, role assigned.");
            }
            else
            {
                logger.LogInformation("ℹ️ CPD user already exists with correct role.");
            }
        }
    }
}

// Run seeding
using (var scope = app.Services.CreateScope())
{
    await SeedRolesAndUsersAsync(scope.ServiceProvider);
}

// Routing 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

await app.RunAsync();
