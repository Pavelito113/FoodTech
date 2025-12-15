using Microsoft.AspNetCore.Mvc;

namespace FoodTech.Controllers;

public class AdminController : Controller
{
    // ====== Dashboard ======
    public IActionResult Index()
    {
        // Безопасная проверка авторизации с проверкой на null
        if (!(User.Identity?.IsAuthenticated ?? false))
            return Challenge();
            
        return View();
    }

    // УДАЛИТЬ все остальные методы (UsersPartial, RequestsPartial и т.д.)
    // Они уже есть в отдельных контроллерах в папке Admin/
}