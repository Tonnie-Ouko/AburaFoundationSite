using AburaFoundationSite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AburaFoundationSite.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MpesaAuthService>();
builder.Services.AddTransient<MpesaStkPushService>();
builder.Services.AddHttpClient<MpesaStkPushService>();
builder.Services.Configure<MpesaSettings>(
    builder.Configuration.GetSection("Mpesa")
);
builder.Services.AddHttpClient<MpesaAuthService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>();

// services config...
builder.Services.AddRazorPages();
var app = builder.Build();

app.UseAuthentication(); // 👈 Required both necessary for Identity and Rotativa
app.UseAuthorization();  // 👈 Required


// 🔧 Configure Rotativa
Rotativa.AspNetCore.RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

// Middleware setup
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();


// Add services to the container.
