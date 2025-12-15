using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks; // Добавьте эту строку

namespace FoodTech.Services
{
    public class UserClaimsService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public UserClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        // Асинхронная версия
        public async Task<(string Name, string Email, string Company, string Phone)> GetUserInfoAsync()
        {
            // Если будете работать с БД, здесь можно добавить асинхронные вызовы
            await Task.CompletedTask; // Пока что заглушка
            
            var user = _httpContextAccessor.HttpContext?.User;
            bool isAuth = user?.Identity?.IsAuthenticated == true;
            
            if (!isAuth) 
                return ("", "", "", "");
                
            return (
                Name: user?.Identity?.Name ?? "",
                Email: user?.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Company: user?.FindFirst("Company")?.Value ?? "",
                Phone: user?.FindFirst(ClaimTypes.MobilePhone)?.Value ?? ""
            );
        }
        
        // Синхронная версия для обратной совместимости
        public (string Name, string Email, string Company, string Phone) GetUserInfo()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            bool isAuth = user?.Identity?.IsAuthenticated == true;
            
            if (!isAuth) 
                return ("", "", "", "");
                
            return (
                Name: user?.Identity?.Name ?? "",
                Email: user?.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Company: user?.FindFirst("Company")?.Value ?? "",
                Phone: user?.FindFirst(ClaimTypes.MobilePhone)?.Value ?? ""
            );
        }
    }
}