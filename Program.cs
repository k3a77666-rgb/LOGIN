using LOGIN.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔥 CONFIGURAR PROTECCIÓN DE DATOS
builder.Services.AddDataProtection()
    .SetApplicationName("LOGIN")
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/keys"));

// Agregar controladores MVC y API
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// 🔥 AGREGAR CACHÉ EN MEMORIA
builder.Services.AddMemoryCache();

// Configurar PostgreSQL (Supabase)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllers();

app.Run();