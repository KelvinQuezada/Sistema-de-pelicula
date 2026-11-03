using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SistemaDePeliculaSakila.Data;
using SistemaDePeliculaSakila.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(opciones =>
    opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<FabricaConexion>();
builder.Services.AddScoped<PeliculaRepository>();
builder.Services.AddScoped<InventarioRepository>();
builder.Services.AddScoped<AlquilerRepository>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<ReporteRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Cultura fija para que los decimales se interpreten siempre con punto.
var cultura = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Crea los usuarios de demostración la primera vez (con la contraseña protegida).
using (var scope = app.Services.CreateScope())
{
    try
    {
        scope.ServiceProvider.GetRequiredService<UsuarioRepository>().CrearUsuariosIniciales();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudieron crear los usuarios iniciales. ¿Ejecutó el script 01_modificaciones_sakila.sql?");
    }
}

app.Run();
