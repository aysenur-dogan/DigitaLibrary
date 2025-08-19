using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DigitaLibrary.Data;      // AppDbContext için
using DigitaLibrary.Models;
using DigitaLibrary.Data;
using Microsoft.EntityFrameworkCore;
// Admin için

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
.AddDefaultUI();

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

// Eski Identity profil URL'lerini Profile/Me sayfasına yönlendir
app.MapGet("/Identity/Account/Manage", ctx => {
    ctx.Response.Redirect("/Profile/Me", permanent: true);
    return Task.CompletedTask;
});
app.MapGet("/Identity/Account/Manage/Index", ctx => {
    ctx.Response.Redirect("/Profile/Me", permanent: true);
    return Task.CompletedTask;
});
app.MapGet("/Identity/Account/Manage/{**_}", ctx => {
    ctx.Response.Redirect("/Profile/Me", permanent: true);
    return Task.CompletedTask;
    

    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddIdentity<Admin, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    var app = builder.Build();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
    app.Run();

});

// ✅ Migrationları otomatik uygula
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate();
//}

app.MapGet("/_whoami", (DigitaLibrary.Data.AppDbContext db) =>
{
    var cnn = db.Database.GetDbConnection();
    return Results.Text($"DB: {cnn.Database}\nServer: {cnn.DataSource}\nConnStr: {cnn.ConnectionString}");
});

app.Run();
