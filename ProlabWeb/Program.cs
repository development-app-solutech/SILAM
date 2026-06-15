using FastReport.Data;
using FastReport.Utils;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Data;
using ProlabWeb.Db;
using ProlabWeb.Extensions;
using ProlabWeb.Seeders;
using Serilog;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Charger Serilog depuis appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("IdentityConnection") ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
builder.Services.AddDbContext<ProlabIdentityDbContext>(options => options.UseNpgsql(connectionString));
var prolabConnectionString = builder.Configuration.GetConnectionString("ProlabConnection") ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
builder.Services.AddDbContext<ProlabwebContext>(options => options.UseNpgsql(prolabConnectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ProlabIdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ProlabIdentityDbContext>();

// Add global authorization filter
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Configure form options to handle large forms
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueCountLimit = 10000; // Increase from default 1024
    options.KeyLengthLimit = 4096; // Increase from default 2048
    options.ValueLengthLimit = 1024 * 1024 * 4; // 4MB per field
    options.MultipartBodyLengthLimit = 1024 * 1024 * 100; // 100MB total
});

// Configure MVC options for large model binding
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
{
    options.MaxModelBindingCollectionSize = 10000; // Increase from default 1024
});
builder.Services.AddRazorPages();

builder.Services.AddAutoMapper(typeof(Program));

//builder.Services.AddFastReport();

FastReport.Utils.RegisteredObjects.AddConnection(typeof(PostgresDataConnection));

var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var app = builder.Build();

//Create or migrate the database and seed initial data
await app.CreateOrMigrateDbContextAsync<ProlabIdentityDbContext>(async (context, services) =>
{
    var userManager = services.GetRequiredService<UserManager<ProlabIdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var adminEmail = builder.Configuration["AdminEmail"] ?? "admin@prolab.com";
    await IdentityDbSeeder.SeedRolesAndAdminAccountAsync(userManager, roleManager, adminEmail);
});

// Configure the HTTP request pipeline.
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
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

//app.UseFastReport();

app.Run();

// Ensure to flush and close loggers on shutdown
Log.CloseAndFlush();
