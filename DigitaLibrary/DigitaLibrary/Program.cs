using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DigitaLibrary.Data;      // AppDbContext için
using DigitaLibrary.Models;    // Admin için

var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity ayarları
builder.Services.AddIdentity<Admin, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // Default Identity UI (Razor Class Library)

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Varsayılan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
// Eski Identity profil URL'lerini tek sayfaya yönlendir
app.MapGet("/Identity/Account/Manage", ctx => {
    ctx.Response.Redirect("/Profile/Me", permanent: true);
    return Task.CompletedTask;
});
app.MapGet("/Identity/Account/Manage/Index", ctx => {
    ctx.Response.Redirect("/Profile/Me", permanent: true);
    return Task.CompletedTask;
});
// Alt sayfalar (Email, Password vs.) dahil
app.MapGet("/Identity/Account/Manage/{**_}", ctx => {
    ctx.Response.Redirect("/Profile/Me", permanent: true);
    return Task.CompletedTask;
});

// ✅ Identity'nin kendi profil sayfasını (/Identity/Account/Manage/Index) Profile/Me sayfasına yönlendir
app.MapGet("/Identity/Account/Manage/Index", context =>
{
    context.Response.Redirect("/Profile/Me");
    return Task.CompletedTask;
});

app.Run();
