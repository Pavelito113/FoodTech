using Microsoft.AspNetCore.Mvc;
using FoodTech.Services;
using FoodTech.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Linq;

namespace FoodTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly EquipmentService _service;

        public HomeController(EquipmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] EquipmentFilterVm filter)
        {
            var vm = await _service.GetEquipmentIndexVmAsync(filter);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Privacy() => View();

        [HttpGet]
        public IActionResult About() => View();

        // ===============================
        // ОБЩАЯ ФОРМА ОБРАТНОЙ СВЯЗИ (GET)
        // ===============================
        [HttpGet]
        public IActionResult CreateContactRequest()
        {
            var model = new ContactRequestVm
            {
                EquipmentId = null,
                EquipmentName = "Общий вопрос",
                Status = "New"
            };
            
            return View(model);
        }

        // ===============================
        // ОБЩАЯ ФОРМА ОБРАТНОЙ СВЯЗИ (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContactRequest(ContactRequestVm model)
        {
            // Фиксированные значения для общей формы
            model.EquipmentId = null;
            model.EquipmentName = "Общий вопрос";
            model.Status = "New";
            
            // Валидация модели
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            // Дополнительная бизнес-логика валидации
            if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("", "Укажите email или телефон для связи.");
                return View(model);
            }
            
            if (!string.IsNullOrWhiteSpace(model.Email) && !IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Некорректный email адрес.");
                return View(model);
            }
            
            if (!string.IsNullOrWhiteSpace(model.Phone) && !IsValidPhone(model.Phone))
            {
                ModelState.AddModelError("Phone", "Некорректный номер телефона.");
                return View(model);
            }

            try
            {
                // Сохраняем источник запроса
                model.SourceUrl = GetReferrerUrl();
                
                // Используем метод для общей формы
                await _service.CreateGeneralContactRequestAsync(model, model.SourceUrl);

                TempData["Success"] = "Ваше сообщение успешно отправлено. Мы свяжемся с вами в ближайшее время.";
                
                // Очищаем форму после успешной отправки
                return RedirectToAction(nameof(CreateContactRequest));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при отправке сообщения: {ex.Message}";
                return View(model);
            }
        }

        // ===============================
        // AJAX-ФОРМА ДЛЯ ОБОРУДОВАНИЯ (модалки)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCreate(ContactRequestVm model)
        {
            try
            {
                // Валидация для модалки
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return Json(new { 
                        success = false, 
                        message = string.Join(" ", errors) 
                    });
                }
                
                // Проверяем, что есть контактные данные
                if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Phone))
                {
                    return Json(new { 
                        success = false, 
                        message = "Укажите email или телефон для связи." 
                    });
                }
                
                if (!string.IsNullOrWhiteSpace(model.Email) && !IsValidEmail(model.Email))
                {
                    return Json(new { 
                        success = false, 
                        message = "Некорректный email адрес." 
                    });
                }
                
                if (!string.IsNullOrWhiteSpace(model.Phone) && !IsValidPhone(model.Phone))
                {
                    return Json(new { 
                        success = false, 
                        message = "Некорректный номер телефона." 
                    });
                }

                // Сохраняем источник запроса
                model.SourceUrl = GetReferrerUrl();
                model.Status = "New";
                
                // Используем метод для оборудования
                var contact = await _service.CreateContactRequestAsync(model, model.SourceUrl);

                return Json(new
                {
                    success = true,
                    message = "Ваш запрос успешно отправлен",
                    equipmentName = contact.EquipmentName
                });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("EquipmentId не может быть null"))
            {
                return Json(new
                {
                    success = false,
                    message = "Ошибка: не указано оборудование"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Ошибка при отправке: {ex.Message}"
                });
            }
        }

        // ===============================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ===============================
        private string GetReferrerUrl()
        {
            return HttpContext.Request.Headers["Referer"].ToString() ?? 
                   HttpContext.Request.Headers["Origin"].ToString() ??
                   $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;
                
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length >= 5;
        }
    }
}