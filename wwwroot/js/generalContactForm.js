// generalContactForm.js
console.log('generalContactForm.js загружен');

document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM загружен, инициализируем форму');
    
    const form = document.getElementById('generalContactForm');
    
    if (!form) {
        console.error('Форма generalContactForm не найдена!');
        return;
    }
    
    console.log('Форма найдена');

    function clearErrors() {
        document.querySelectorAll('.field-error').forEach(e => e.textContent = '');
    }

    function showFieldError(fieldId, message) {
        const errorElement = document.getElementById(fieldId + 'Error');
        if (errorElement) {
            errorElement.textContent = message;
        }
    }

    function isValidEmail(email) {
        if (!email || email.trim() === '') return true;
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    function isValidPhone(phone) {
        if (!phone || phone.trim() === '') return true;
        const digits = phone.replace(/\D/g, '');
        return digits.length >= 5;
    }

    function validateForm() {
        console.log('Валидация формы...');
        clearErrors();
        let isValid = true;

        // Проверка имени
        const name = document.getElementById('Name').value.trim();
        if (!name) {
            showFieldError('name', 'Введите ваше имя');
            isValid = false;
        }

        // Проверка сообщения
        const message = document.getElementById('Message').value.trim();
        if (!message) {
            showFieldError('message', 'Введите ваше сообщение');
            isValid = false;
        }

        // Проверка email и телефона
        const email = document.getElementById('Email').value.trim();
        const phone = document.getElementById('Phone').value.trim();

        // Хотя бы один контакт должен быть указан
        if (!email && !phone) {
            showFieldError('email', 'Укажите email или телефон');
            showFieldError('phone', 'Укажите email или телефон');
            isValid = false;
        }

        // Валидация email
        if (email && !isValidEmail(email)) {
            showFieldError('email', 'Введите корректный email');
            isValid = false;
        }

        // Валидация телефона
        if (phone && !isValidPhone(phone)) {
            showFieldError('phone', 'Введите корректный номер телефона (минимум 5 цифр)');
            isValid = false;
        }

        // Проверка согласия для неавторизованных
        if (window.isAuthenticated === false) {
            const agreeCheckbox = document.getElementById('AgreeTerms');
            if (agreeCheckbox && !agreeCheckbox.checked) {
                showFieldError('terms', 'Необходимо согласие с политикой конфиденциальности');
                isValid = false;
            }
        }

        console.log('Валидация завершена, результат:', isValid);
        return isValid;
    }

    // Обработчик отправки формы
    form.addEventListener('submit', function (event) {
        console.log('Отправка формы...');
        
        if (!validateForm()) {
            console.log('Валидация не пройдена, отправка отменена');
            event.preventDefault();
            return;
        }

        // Показываем индикатор загрузки
        const submitBtn = document.getElementById('submitBtn');
        const spinner = submitBtn.querySelector('.spinner-border');
        const text = submitBtn.querySelector('.submit-text');

        submitBtn.disabled = true;
        spinner.classList.remove('d-none');
        text.textContent = 'Отправка...';

        // Форма отправится стандартным способом
        console.log('Валидация пройдена, отправка разрешена');
    });
});