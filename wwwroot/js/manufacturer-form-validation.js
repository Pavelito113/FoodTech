// scripts/manufacturer-form-validation.js
$(document).ready(function() {
    // Находим форму создания/редактирования производителя
    var $form = $('form[asp-action="Create"], form[asp-action="Edit"]');
    
    if ($form.length === 0) {
        $form = $('form').filter(function() {
            return $(this).attr('action')?.includes('/ManufacturerUser/');
        });
    }
    
    if ($form.length === 0) return;
    
    // Функция для валидации URL
    function validateWebsite(url) {
        if (!url || url.trim() === '') return true; // Пустое поле - валидно
        
        // Регулярное выражение совпадает с серверным
        var pattern = /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/i;
        return pattern.test(url);
    }
    
    // Функция для валидации email
    function validateEmail(email) {
        if (!email || email.trim() === '') return true; // Пустое поле - валидно
        
        var pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return pattern.test(email);
    }
    
    // Функция для валидации телефона
    function validatePhone(phone) {
        if (!phone || phone.trim() === '') return true; // Пустое поле - валидно
        
        // Базовая проверка телефона (можно адаптировать под нужный формат)
        var pattern = /^[\d\s\-\+\(\)]{10,20}$/;
        return pattern.test(phone.replace(/\s/g, ''));
    }
    
    // Функция для отображения/скрытия ошибок
    function showError($field, message) {
        var $errorSpan = $field.next('.text-danger');
        if ($errorSpan.length === 0) {
            $errorSpan = $('<span class="text-danger small"></span>');
            $field.after($errorSpan);
        }
        $errorSpan.text(message);
        $field.addClass('is-invalid');
        return false;
    }
    
    function clearError($field) {
        $field.removeClass('is-invalid');
        var $errorSpan = $field.next('.text-danger');
        if ($errorSpan.length > 0) {
            $errorSpan.remove();
        }
        return true;
    }
    
    // Валидация при вводе
    $form.on('input', 'input[name="Website"]', function() {
        var $field = $(this);
        var value = $field.val().trim();
        
        if (value === '') {
            clearError($field);
        } else if (!validateWebsite(value)) {
            showError($field, 'Некорректный формат URL (пример: example.com или https://example.com)');
        } else {
            clearError($field);
        }
    });
    
    $form.on('input', 'input[name="ContactEmail"]', function() {
        var $field = $(this);
        var value = $field.val().trim();
        
        if (value === '') {
            clearError($field);
        } else if (!validateEmail(value)) {
            showError($field, 'Некорректный email адрес');
        } else {
            clearError($field);
        }
    });
    
    $form.on('input', 'input[name="ContactPhone"]', function() {
        var $field = $(this);
        var value = $field.val().trim();
        
        if (value === '') {
            clearError($field);
        } else if (!validatePhone(value)) {
            showError($field, 'Некорректный номер телефона');
        } else {
            clearError($field);
        }
    });
    
    // Валидация длины полей при вводе
    $form.on('input', 'input[name="Name"]', function() {
        var $field = $(this);
        var value = $field.val();
        
        if (value.length > 100) {
            showError($field, 'Максимальная длина 100 символов');
        } else {
            clearError($field);
        }
    });
    
    $form.on('input', 'input[name="Country"]', function() {
        var $field = $(this);
        var value = $field.val();
        
        if (value.length > 50) {
            showError($field, 'Максимальная длина 50 символов');
        } else {
            clearError($field);
        }
    });
    
    $form.on('input', 'textarea[name="Notes"]', function() {
        var $field = $(this);
        var value = $field.val();
        var $counter = $field.next('.char-counter');
        
        if ($counter.length === 0) {
            $counter = $('<div class="form-text char-counter text-end"></div>');
            $field.after($counter);
        }
        
        $counter.text(value.length + '/500');
        
        if (value.length > 500) {
            showError($field, 'Максимальная длина 500 символов');
            $counter.addClass('text-danger');
        } else {
            clearError($field);
            $counter.removeClass('text-danger');
        }
    });
    
    // Автоматическое форматирование URL
    $form.on('blur', 'input[name="Website"]', function() {
        var $field = $(this);
        var value = $field.val().trim();
        
        if (value && !value.startsWith('http://') && !value.startsWith('https://')) {
            // Автоматически добавляем https:// если его нет
            if (validateWebsite('https://' + value)) {
                $field.val('https://' + value);
                clearError($field);
            }
        }
    });
    
    // Валидация всей формы перед отправкой
    $form.on('submit', function(e) {
        var isValid = true;
        var $submitButton = $(this).find('button[type="submit"]');
        
        // Блокируем кнопку отправки
        $submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Отправка...');
        
        // Проверяем обязательное поле Name
        var $nameField = $form.find('input[name="Name"]');
        if (!$nameField.val() || $nameField.val().trim() === '') {
            showError($nameField, 'Название обязательно');
            isValid = false;
        } else if ($nameField.val().length > 100) {
            showError($nameField, 'Максимальная длина 100 символов');
            isValid = false;
        }
        
        // Проверяем Website
        var $websiteField = $form.find('input[name="Website"]');
        var websiteValue = $websiteField.val()?.trim();
        if (websiteValue && !validateWebsite(websiteValue)) {
            showError($websiteField, 'Некорректный формат URL');
            isValid = false;
        }
        
        // Проверяем ContactEmail если есть
        var $emailField = $form.find('input[name="ContactEmail"]');
        var emailValue = $emailField.val()?.trim();
        if (emailValue && !validateEmail(emailValue)) {
            showError($emailField, 'Некорректный email адрес');
            isValid = false;
        }
        
        // Проверяем ContactPhone если есть
        var $phoneField = $form.find('input[name="ContactPhone"]');
        var phoneValue = $phoneField.val()?.trim();
        if (phoneValue && !validatePhone(phoneValue)) {
            showError($phoneField, 'Некорректный номер телефона');
            isValid = false;
        }
        
        // Проверяем Country
        var $countryField = $form.find('input[name="Country"]');
        if ($countryField.val() && $countryField.val().length > 50) {
            showError($countryField, 'Максимальная длина 50 символов');
            isValid = false;
        }
        
        // Проверяем Notes
        var $notesField = $form.find('textarea[name="Notes"]');
        if ($notesField.val() && $notesField.val().length > 500) {
            showError($notesField, 'Максимальная длина 500 символов');
            isValid = false;
        }
        
        if (!isValid) {
            e.preventDefault();
            e.stopPropagation();
            
            // Прокручиваем к первой ошибке
            $('html, body').animate({
                scrollTop: $form.find('.is-invalid').first().offset().top - 100
            }, 500);
            
            // Разблокируем кнопку
            setTimeout(function() {
                $submitButton.prop('disabled', false).text('Создать производителя');
            }, 2000);
            
            return false;
        }
        
        // Если все валидно, разрешаем отправку
        return true;
    });
    
    // Автоматическая очистка ошибок при начале ввода
    $form.on('focus', 'input, textarea', function() {
        var $field = $(this);
        setTimeout(function() {
            clearError($field);
        }, 100);
    });
    
    // Инициализация счетчика символов для Notes
    var $notesField = $form.find('textarea[name="Notes"]');
    if ($notesField.length > 0) {
        var initialValue = $notesField.val() || '';
        var $counter = $('<div class="form-text char-counter text-end"></div>');
        $notesField.after($counter);
        $counter.text(initialValue.length + '/500');
    }
});