using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.Models;
using FoodTech.ViewModels;
using FoodTech.Services;

var builder = WebApplication.CreateBuilder(args);

// ====== Конфигурация подключения к базе ======
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// ====== Настройка Kestrel ======
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // слушать на всех интерфейсах
});

// ====== DbContext ======
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, x => x.MigrationsAssembly("FoodTech")));

// ====== Identity ======
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// ====== Регистрация сервисов ======
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<UserClaimsService>();
builder.Services.AddScoped<EmailService>();

// ====== Сессии ======
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(300);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ====== MVC / Razor / API ======
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ====== CORS ======
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001",
            "http://127.0.0.1:5000",
            "http://[::1]:5000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// ====== Инициализация базы и ролей ======
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await WaitForSqlServer(context);
        await context.Database.MigrateAsync();
        
        // Инициализация ролей
        await InitializeRoles(services);
        
        // Инициализация остальных данных
        await SeedData.Initialize(services);
        
        Console.WriteLine("✅ База данных и роли успешно инициализированы!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Ошибка инициализации базы данных или ролей");
        throw;
    }
}

// ====== Middleware ======
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ====== Включаем CORS ДО аутентификации ======
app.UseCors("AllowLocalhost");

// ====== Сессии, аутентификация, авторизация ======
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ====== Маршруты ======

// 1. Обычные контроллеры (Home, Equipment и др.)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 2. Админские контроллеры в папке Controllers/Admin
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller}/{action}/{id?}",
    defaults: new { area = "Admin" });

// 3. Маршрут для Requests
app.MapControllerRoute(
    name: "requests",
    pattern: "Requests/{action=MyRequests}/{id?}",
    defaults: new { controller = "Request" });
    
app.MapRazorPages();
app.MapControllers();

app.Run();

// ====== Вспомогательные методы ======

// Проверка подключения к SQL Server
async Task WaitForSqlServer(ApplicationDbContext context, int maxAttempts = 15)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            if (await context.Database.CanConnectAsync())
            {
                Console.WriteLine("✅ SQL Server готов к работе!");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⏳ Ожидание SQL Server... Попытка {attempt}/{maxAttempts}. Ошибка: {ex.Message}");
            if (attempt == maxAttempts) throw;
            await Task.Delay(3000);
        }
    }
    throw new Exception($"Не удалось подключиться к SQL Server после {maxAttempts} попыток");
}

// Инициализация ролей
async Task InitializeRoles(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
    // Создаем базовые роли, если их нет
    string[] roles = { "Admin", "User", "Moderator" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"✅ Создана роль: {role}");
        }
    }
    
    // Опционально: создаем администратора по умолчанию
    var adminEmail = "admin@example.com";
    
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Администратор",
            LastName = "Системы",
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Создан администратор: {adminEmail}");
        }
        else
        {
            Console.WriteLine($"❌ Не удалось создать администратора: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}