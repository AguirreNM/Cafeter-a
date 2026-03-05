using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PryCafeteria.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Registrar filtro global para badge de stock bajo en sidebar
    options.Filters.AddService<PryCafeteria.Filters.StockBajoFilter>();
});

// Registrar el filtro como servicio scoped (necesita DbContext)
builder.Services.AddScoped<PryCafeteria.Filters.StockBajoFilter>();

// HU-Productos: servicio de gestión de imágenes (validación + redimensionado)
builder.Services.AddScoped<PryCafeteria.Services.IImagenService,
                           PryCafeteria.Services.ImagenService>();

// HU-Dashboard: caché en memoria para métricas del panel
builder.Services.AddMemoryCache();

// HU11-HU13: sesión para carrito de compras
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<BdcafeteriaContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BDCAFETERIAConn"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;

        // HU02: bloqueo tras 5 intentos fallidos
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<BdcafeteriaContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // Rutas personalizadas — sin esto Identity redirige a /Account/Login (404)
    options.LoginPath = "/Cuentas/Login";
    options.LogoutPath = "/Cuentas/Logout";
    options.AccessDeniedPath = "/Cuentas/Login";
});

// NOTA: AddAuthentication ya está registrado por AddIdentity arriba.
// No se llama de nuevo para evitar sobrescribir la configuración de Identity.
builder.Services.AddAuthorization();

var cultureInfo = new System.Globalization.CultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Aplicar migraciones pendientes automáticamente al iniciar
    var db = scope.ServiceProvider.GetRequiredService<BdcafeteriaContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Crear roles
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("Cliente"))
        await roleManager.CreateAsync(new IdentityRole("Cliente"));

    // Crear usuario admin
    var adminEmail = "admin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Nombre = "Administrador",
            Apellido = "Sistema",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "admin123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}

// Páginas de error personalizadas (404, etc.)
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();          // debe ir antes de Auth para que el carrito esté disponible
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
