document.addEventListener('DOMContentLoaded', function () {
    const elements = {
        modal: document.getElementById('contactModal'),
        form: document.getElementById('contactForm'),
        submitBtn: document.getElementById('submitContactForm'),
        spinner: document.querySelector('#submitContactForm .spinner-border'),
        submitText: document.querySelector('#submitContactForm .submit-text'),
        successAlert: document.getElementById('successAlert'),
        errorAlert: document.getElementById('errorAlert'),
        equipField: document.getElementById('formEquipmentId'),
        equipNameField: document.getElementById('formEquipmentName'),
        equipNameLabel: document.getElementById('selectedEquipmentName')
    };

    // Проверка необходимых элементов
    if (!elements.modal || !elements.form) return;
    
    const modal = new bootstrap.Modal(elements.modal);
    const isAuth = window.isAuthenticated === true;
    const requestUrl = window.requestCreateUrl;

    // Валидация
    function validateField(field) {
        if (field.type === 'checkbox') {
            const isValid = field.checked;
            field.classList.toggle('is-invalid', !isValid);
            field.classList.toggle('is-valid', isValid);
            return isValid;
        }
        
        const isValid = field.value.trim().length > 0;
        field.classList.toggle('is-invalid', !isValid);
        field.classList.toggle('is-valid', isValid);
        return isValid;
    }

    function validateForm() {
        const requiredFields = elements.form.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!validateField(field)) {
                isValid = false;
            }
        });

        // Дополнительная проверка оборудования
        if (!elements.equipField?.value) {
            isValid = false;
            showError('Оборудование не выбрано');
        }

        return isValid;
    }

    // Управление состоянием UI
    function setLoadingState(loading) {
        if (elements.submitBtn) {
            elements.submitBtn.disabled = loading;
        }
        if (elements.spinner) {
            elements.spinner.classList.toggle('d-none', !loading);
        }
        if (elements.submitText) {
            elements.submitText.textContent = loading ? 'Отправка...' : 'Отправить запрос';
        }
    }

    function resetForm() {
        elements.form.reset();
        elements.form.querySelectorAll('.is-valid, .is-invalid')
            .forEach(el => el.classList.remove('is-valid', 'is-invalid'));
        
        hideAlerts();
    }

    function hideAlerts() {
        if (elements.successAlert) elements.successAlert.classList.add('d-none');
        if (elements.errorAlert) elements.errorAlert.classList.add('d-none');
    }

    function showAlert(alertElement, messageElementId, message) {
        hideAlerts();
        
        if (alertElement) {
            const messageEl = alertElement.querySelector(messageElementId);
            if (messageEl) messageEl.textContent = message;
            alertElement.classList.remove('d-none');
        }
    }

    function showSuccess(message) {
        showAlert(elements.successAlert, '#successMessage', message);
    }

    function showError(message) {
        showAlert(elements.errorAlert, '#errorMessage', message);
    }

    // Обработчики событий
    document.addEventListener('click', function (e) {
        const targetBtn = e.target.closest('.contact-btn, [data-bs-target="#contactModal"]');
        if (!targetBtn) return;

        e.preventDefault();
        
        const equipId = targetBtn.dataset.equipmentId || '';
        const equipName = targetBtn.dataset.equipmentName || 'не выбрано';

        // Установка данных оборудования
        if (elements.equipField) elements.equipField.value = equipId;
        if (elements.equipNameField) elements.equipNameField.value = equipName;  
        if (elements.equipNameLabel) elements.equipNameLabel.textContent = equipName;

        resetForm();
        modal.show();
    });

    // Валидация при потере фокуса
    elements.form.querySelectorAll('input, textarea, select').forEach(input => {
        input.addEventListener('blur', () => validateField(input));
    });

    // Отправка формы
    elements.form.addEventListener('submit', async function (e) {
        e.preventDefault();
        
        if (!validateForm()) {
            showError('Пожалуйста, заполните все обязательные поля');
            return;
        }

        setLoadingState(true);
        
        try {
            const formData = new FormData(elements.form);
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const response = await fetch(requestUrl, {
                method: 'POST',
                body: formData,
                headers: token ? { 'RequestVerificationToken': token } : {}
            });

            const result = await response.json();

            if (result.success) {
                showSuccess(result.message);
                window.showToast?.(result.message, 'success');
                setTimeout(() => { 
                    modal.hide(); 
                    resetForm(); 
                }, 2000);
            } else {
                throw new Error(result.message || 'Ошибка при отправке формы');
            }
            
        } catch (error) {
            console.error('Form submission error:', error);
            const message = error.message || 'Ошибка соединения с сервером';
            showError(message);
            window.showToast?.(message, 'danger');
        } finally {
            setLoadingState(false);
        }
    });

    // Закрытие модального окна
    elements.modal.addEventListener('hidden.bs.modal', function () {
        resetForm();
    });
});