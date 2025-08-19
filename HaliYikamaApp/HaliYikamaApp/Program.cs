using HaliYikamaApp.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

/* --------- Yerelleþtirme (tr-TR) --------- */
var tr = new CultureInfo("tr-TR");
var supportedCultures = new[] { tr };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(tr),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Bazý senaryolarda model binding için faydalý:
CultureInfo.DefaultThreadCurrentCulture = tr;
CultureInfo.DefaultThreadCurrentUICulture = tr;
/* ----------------------------------------- */

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Varsayýlan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// (Opsiyonel) Orders için açýk rota – /Orders/Edit/5 vb. kesin eþleþir
app.MapControllerRoute(
    name: "orders-id",
    pattern: "Orders/{action=Index}/{id:int?}",
    defaults: new { controller = "Orders" });

app.Run();
