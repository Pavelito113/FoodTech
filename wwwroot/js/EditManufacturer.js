// 📘 EditManufacturer.js — исправленная версия с правильной инициализацией Bootstrap
console.log("📘 EditManufacturer.js загружен");

// Конфигурация маршрутов
const ROUTES = {
    base: "/admin/manufacturers",
    create: "/admin/manufacturers/create",
    edit: (id) => `/admin/manufacturers/edit/${id}`,
    delete: (id) => `/admin/manufacturers/delete/${id}`,
    deleteConfirm: "/admin/manufacturers/delete",
    save: "/admin/manufacturers/save",
    users: (id) => `/admin/manufacturers/users/${id}`,
    assignUser: "/admin/manufacturers/assign-user",
    unassignUser: "/admin/manufacturers/unassign-user"
};

// Глобальные переменные для управления модальными окнами
let currentModalInstance = null;

// Регистрация в PartialManager
if (window.PartialManager) {
    PartialManager.register("EditManufacturer", initEditManufacturer);
} else {
    console.warn("⚠️ PartialManager не найден, используем прямую инициализацию");
    document.addEventListener('DOMContentLoaded', () => {
        const container = document.getElementById('editManufacturersContainer');
        if (container) initEditManufacturer(container);
    });
}

// ================================================================
// 🚀 ИНИЦИАЛИЗАЦИЯ
// ================================================================
function initEditManufacturer(container) {
    console.log("🚀 Инициализация EditManufacturer...");

    // Кнопка добавления в заголовке
    setupButton(container, "#addManufacturerBtn", () => openManufacturerModal());
    
    // Кнопки в пустом состоянии
    setupButton(container, "#addManufacturerBtnEmpty", () => openManufacturerModal());
    setupButton(container, "#addManufacturerBtnEmptyMobile", () => openManufacturerModal());

    // Кнопки редактирования
    container.querySelectorAll(".edit-manufacturer-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            const id = this.dataset.id;
            console.log(`✏️ Редактирование производителя ID: ${id}`);
            openManufacturerModal(id);
        });
    });

    // Кнопки удаления
    container.querySelectorAll(".delete-manufacturer-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            const id = this.dataset.id;
            const name = this.dataset.name;
            console.log(`🗑 Удаление производителя: ${name} (ID: ${id})`);
            openDeleteModal(id, name);
        });
    });

    // Кнопки управления пользователями
    container.querySelectorAll(".manage-users-btn").forEach(btn => {
        btn.addEventListener("click", async function () {
            const manufacturerId = this.getAttribute('data-id');
            console.log(`👥 Управление пользователями для производителя ID: ${manufacturerId}`);
            await openUserAssignmentModal(manufacturerId);
        });
    });

    console.log("✅ EditManufacturer инициализирован");
}

// ================================================================
// 🔧 ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
// ================================================================
function setupButton(container, selector, handler) {
    const btn = container.querySelector(selector);
    if (btn) btn.addEventListener("click", handler);
}

function removeOldModal(modalId) {
    const oldModal = document.getElementById(modalId);
    if (oldModal) {
        // Используем правильный способ получения экземпляра модального окна
        const instance = bootstrap.Modal.getInstance(oldModal);
        if (instance) {
            instance.hide();
            instance.dispose(); // Важно: освобождаем ресурсы
        }
        oldModal.remove();
    }
    currentModalInstance = null;
}

// Функция для инициализации динамически добавленных модальных окон
function initializeBootstrapModal(modalElement) {
    if (!modalElement) return null;
    
    // Создаем экземпляр модального окна
    const modal = new bootstrap.Modal(modalElement, {
        backdrop: true,
        focus: true,
        keyboard: true
    });
    
    // Сохраняем ссылку на экземпляр
    currentModalInstance = modal;
    
    // Добавляем обработчик закрытия для очистки
    modalElement.addEventListener('hidden.bs.modal', () => {
        setTimeout(() => {
            if (modalElement.parentNode) {
                modalElement.remove();
            }
            currentModalInstance = null;
        }, 300);
    });
    
    return modal;
}

// ================================================================
// 🔍 ОТКРЫТИЕ ФОРМЫ СОЗДАНИЯ/РЕДАКТИРОВАНИЯ
// ================================================================
async function openManufacturerModal(id = null) {
    try {
        const url = id ? ROUTES.edit(id) : ROUTES.create;
        console.log(`📤 Загрузка формы по URL: ${url}`);

        // Удаляем старую модалку, если она есть
        removeOldModal("manufacturerModal");

        const response = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            console.error(`❌ HTTP ${response.status} для ${url}`);
            throw new Error(`HTTP ${response.status}`);
        }

        const html = await response.text();

        // Создаем модальное окно с правильной структурой
        const modalHtml = `
            <div class="modal fade" id="manufacturerModal" tabindex="-1" aria-labelledby="manufacturerModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content" id="manufacturerModalContent">
                        ${html}
                    </div>
                </div>
            </div>
        `;

        // Вставляем модальное окно в DOM
        document.body.insertAdjacentHTML("beforeend", modalHtml);

        // Находим элемент модального окна
        const modalEl = document.getElementById("manufacturerModal");
        if (!modalEl) {
            throw new Error("Модальное окно не создано");
        }

        // Инициализируем Bootstrap модальное окно
        const modal = initializeBootstrapModal(modalEl);
        if (!modal) {
            throw new Error("Не удалось инициализировать модальное окно");
        }

        // Инициализируем форму внутри модального окна
        const form = modalEl.querySelector("form");
        if (form) {
            form.addEventListener("submit", async (e) => {
                e.preventDefault();
                await saveManufacturer(form, modal);
            });
        }

        // Показываем модальное окно
        modal.show();

    } catch (err) {
        console.error("❌ Ошибка загрузки формы:", err);
        showToast("Ошибка загрузки формы", "danger");
        
        // Показываем дополнительную информацию об ошибке
        if (err.message.includes("404")) {
            console.error("Маршрут не найден. Проверьте контроллер.");
        }
    }
}

// ================================================================
// 👥 ОТКРЫТИЕ МОДАЛКИ УПРАВЛЕНИЯ ПОЛЬЗОВАТЕЛЯМИ
// ================================================================
// Замените старую функцию на новую:
async function openUserAssignmentModal(manufacturerId) {
    try {
        const url = ROUTES.users(manufacturerId);
        console.log(`👥 Загрузка пользователей по URL: ${url}`);

        removeOldModal("userAssignmentModal");

        const response = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            console.error(`❌ HTTP ${response.status} для ${url}`);
            throw new Error('Ошибка загрузки пользователей');
        }

        const html = await response.text();
        
        document.body.insertAdjacentHTML('beforeend', html);
        
        // Ждем немного, чтобы DOM обновился, затем инициализируем
        setTimeout(() => {
            const modalEl = document.getElementById('userAssignmentModal');
            if (modalEl) {
                const modal = new bootstrap.Modal(modalEl);
                modal.show();
                setupUserAssignmentFunctionality(modalEl);
                
                modalEl.addEventListener('hidden.bs.modal', () => {
                    setTimeout(() => modalEl.remove(), 300);
                });
            }
        }, 100);

    } catch (error) {
        console.error('❌ Ошибка загрузки пользователей:', error);
        showToast('Не удалось загрузить список пользователей', 'danger');
    }
}

// ================================================================
// 💾 СОХРАНЕНИЕ ПРОИЗВОДИТЕЛЯ
// ================================================================
async function saveManufacturer(form, modalInstance) {
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn?.innerHTML;

    try {
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Сохранение...';
        }

        const formData = new FormData(form);
        
        console.log(`💾 Сохранение по URL: ${ROUTES.save}`);

        const response = await fetch(ROUTES.save, {
            method: "POST",
            body: formData,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        const html = await response.text();

        // Если есть ошибки валидации
        if (html.includes("field-validation-error") || html.includes("text-danger")) {
            // Обновляем содержимое модального окна
            const modalContent = document.getElementById("manufacturerModalContent");
            if (modalContent) {
                modalContent.innerHTML = html;
                
                // Переинициализируем форму
                const newForm = modalContent.querySelector("form");
                if (newForm) {
                    newForm.addEventListener("submit", async (e) => {
                        e.preventDefault();
                        await saveManufacturer(newForm, modalInstance);
                    });
                }
            }
            return;
        }

        // Закрываем модальное окно
        if (modalInstance && modalInstance.hide) {
            modalInstance.hide();
        }
        
        showToast("Изменения сохранены", "success");
        await reloadManufacturersList();

    } catch (err) {
        console.error("❌ Ошибка сохранения:", err);
        showToast("Ошибка сохранения", "danger");
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    }
}

// ================================================================
// 🗑 ОТКРЫТИЕ МОДАЛКИ УДАЛЕНИЯ
// ================================================================
async function openDeleteModal(id, name) {
    try {
        const url = ROUTES.delete(id);
        console.log(`🗑 Загрузка диалога удаления по URL: ${url}`);

        // Удаляем старую модалку
        removeOldModal("manufacturerModal");

        const response = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const html = await response.text();

        // Создаем модальное окно
        const modalHtml = `
            <div class="modal fade" id="manufacturerModal" tabindex="-1" aria-labelledby="deleteModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content" id="manufacturerModalContent">
                        ${html}
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML("beforeend", modalHtml);

        const modalEl = document.getElementById("manufacturerModal");
        const modal = initializeBootstrapModal(modalEl);

        // Инициализируем форму удаления
        const deleteForm = modalEl.querySelector("#deleteForm");
        if (deleteForm) {
            deleteForm.addEventListener("submit", async (e) => {
                e.preventDefault();
                await confirmDelete(deleteForm, modal);
            });
        }

        // Показываем модальное окно
        if (modal) {
            modal.show();
        }

    } catch (err) {
        console.error("❌ Ошибка загрузки диалога:", err);
        showToast("Ошибка загрузки диалога", "danger");
    }
}

// ================================================================
// ✔ ПОДТВЕРЖДЕНИЕ УДАЛЕНИЯ
// ================================================================
async function confirmDelete(form, modalInstance) {
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn?.innerHTML;

    try {
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Удаление...';
        }

        const formData = new FormData(form);
        
        console.log("🔍 Отправка запроса на удаление...");

        const response = await fetch(ROUTES.deleteConfirm, {
            method: "POST",
            body: formData,
            credentials: "same-origin",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        const result = await response.json();

        if (result?.success) {
            // Закрываем модальное окно
            if (modalInstance && modalInstance.hide) {
                modalInstance.hide();
            }
            
            showToast("Производитель удалён", "success");
            await reloadManufacturersList();
        } else {
            showToast(result?.message || "Ошибка удаления", "danger");
        }

    } catch (err) {
        console.error("❌ Ошибка удаления:", err);
        showToast("Ошибка удаления", "danger");
    } finally {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    }
}

// ================================================================
// 🔄 ПЕРЕЗАГРУЗКА СПИСКА ПРОИЗВОДИТЕЛЕЙ
// ================================================================
async function reloadManufacturersList() {
    try {
        const container = document.getElementById("editManufacturersContainer");
        if (!container) {
            console.error("❌ Контейнер не найден");
            return;
        }

        console.log(`🔄 Перезагрузка списка по URL: ${ROUTES.base}`);

        const response = await fetch(ROUTES.base, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const html = await response.text();

        // Создаем временный контейнер
        const tempDiv = document.createElement("div");
        tempDiv.innerHTML = html;

        const newContainer = tempDiv.querySelector("#editManufacturersContainer");
        
        if (!newContainer) {
            console.error("❌ Новый контейнер не найден");
            return;
        }

        // Заменяем старый контейнер
        container.replaceWith(newContainer);

        // Повторно инициализируем
        initEditManufacturer(newContainer);

        console.log("✅ Список производителей обновлён");

    } catch (err) {
        console.error("❌ Ошибка обновления списка:", err);
        showToast("Ошибка обновления списка", "danger");
    }
}

// ================================================================
// 🎯 ЭКСПОРТ ФУНКЦИЙ ДЛЯ ГЛОБАЛЬНОГО ДОСТУПА
// ================================================================
// Экспортируем функции для доступа из встроенного скрипта
window.openManufacturerModal = openManufacturerModal;
window.openDeleteModal = openDeleteModal;
window.openUserAssignmentModal = openUserAssignmentModal;
window.reloadManufacturersList = reloadManufacturersList;
// ================================================================
// 👥 ГЛОБАЛЬНЫЙ ОБРАБОТЧИК ДЛЯ МОДАЛКИ УПРАВЛЕНИЯ ПОЛЬЗОВАТЕЛЯМИ
// ================================================================

// Инициализация модалки управления пользователями после загрузки
function initUserAssignmentModal() {
    const modalEl = document.getElementById('userAssignmentModal');
    if (!modalEl) return;
    
    console.log('👥 Инициализация модалки управления пользователями...');
    
    // Инициализируем Bootstrap модальное окно
    const modal = new bootstrap.Modal(modalEl, {
        backdrop: true,
        focus: true,
        keyboard: true
    });
    
    // Показываем модальное окно
    modal.show();
    
    // Инициализируем функционал
    setupUserAssignmentFunctionality(modalEl);
    
    // Обработчик закрытия
    modalEl.addEventListener('hidden.bs.modal', () => {
        setTimeout(() => {
            if (modalEl.parentNode) {
                modalEl.remove();
            }
        }, 300);
    });
}

// Настройка всего функционала модалки
function setupUserAssignmentFunctionality(modalEl) {
    // Элементы DOM
    const userSelect = modalEl.querySelector('.user-select');
    const assignBtn = modalEl.querySelector('.assign-user-btn');
    const userSearch = modalEl.querySelector('.user-search-input');
    const clearSearchBtn = modalEl.querySelector('.clear-search-btn');
    const userCountSpan = modalEl.querySelector('.user-count');
    const expandBtns = modalEl.querySelectorAll('.expand-btn');
    const unassignBtns = modalEl.querySelectorAll('.unassign-user-btn');
    
    // Токен для запросов
    const antiForgeryToken = modalEl.querySelector('#userAssignmentAntiForgeryToken')?.value || 
                            document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    // Оригинальные опции для поиска
    const originalOptions = userSelect ? Array.from(userSelect.options) : [];
    
    // Инициализация
    updateUserCount();
    
    // ========== ПОИСК ПОЛЬЗОВАТЕЛЕЙ ==========
    if (userSearch && userSelect) {
        userSearch.addEventListener('input', function(e) {
            const searchTerm = e.target.value.toLowerCase().trim();
            filterUserOptions(searchTerm);
        });
        
        clearSearchBtn?.addEventListener('click', function() {
            userSearch.value = '';
            filterUserOptions('');
            userSearch.focus();
        });
        
        userSearch.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                userSearch.value = '';
                filterUserOptions('');
            }
        });
    }
    
    // ========== ФИЛЬТРАЦИЯ ОПЦИЙ ==========
    function filterUserOptions(searchTerm) {
        if (!userSelect || !originalOptions.length) return;
        
        userSelect.innerHTML = '<option value="" disabled selected>Выберите пользователя...</option>';
        let visibleCount = 0;
        
        originalOptions.forEach(option => {
            if (option.value === '') return;
            
            const searchData = option.getAttribute('data-search') || '';
            if (searchData.includes(searchTerm)) {
                userSelect.appendChild(option.cloneNode(true));
                visibleCount++;
            }
        });
        
        updateUserCount(visibleCount);
        userSelect.value = '';
        if (assignBtn) assignBtn.disabled = true;
    }
    
    // ========== ОБНОВЛЕНИЕ СЧЕТЧИКА ==========
    function updateUserCount(count) {
        if (!userCountSpan) return;
        
        if (count !== undefined) {
            userCountSpan.textContent = count;
        } else if (userSelect) {
            const visibleOptions = Array.from(userSelect.options).filter(opt => opt.value !== '');
            userCountSpan.textContent = visibleOptions.length;
        }
    }
    
    // ========== КНОПКИ РАЗВЕРТЫВАНИЯ ==========
    expandBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const target = this.getAttribute('data-target');
            const collapseEl = modalEl.querySelector(target);
            if (!collapseEl) return;
            
            const icon = this.querySelector('i');
            const isExpanded = !collapseEl.classList.contains('show');
            
            // Переключаем иконку
            if (isExpanded) {
                icon.classList.remove('bi-chevron-down');
                icon.classList.add('bi-chevron-up');
            } else {
                icon.classList.remove('bi-chevron-up');
                icon.classList.add('bi-chevron-down');
            }
            
            // Bootstrap сам управляет collapse
        });
    });
    
    // ========== ВКЛЮЧЕНИЕ/ОТКЛЮЧЕНИЕ КНОПКИ ПРИВЯЗКИ ==========
    if (userSelect && assignBtn) {
        userSelect.addEventListener('change', function() {
            assignBtn.disabled = !this.value || this.value === '';
        });
    }
    
    // ========== ПРИВЯЗКА ПОЛЬЗОВАТЕЛЯ ==========
    if (assignBtn) {
        assignBtn.addEventListener('click', async function() {
            const userId = userSelect?.value;
            const manufacturerId = this.getAttribute('data-manufacturer-id');
            
            if (!userId) return;
            
            const btn = this;
            const originalText = btn.innerHTML;
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Привязка...';
            
            try {
                const response = await fetch('/admin/manufacturers/assign-user', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': antiForgeryToken || '',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({
                        ManufacturerId: parseInt(manufacturerId),
                        UserId: userId
                    })
                });
                
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }
                
                const result = await response.json();
                
                if (result.success) {
                    showToast('Пользователь успешно привязан', 'success');
                    
                    // Закрываем модалку
                    const modalInstance = bootstrap.Modal.getInstance(modalEl);
                    if (modalInstance) modalInstance.hide();
                    
                    // Обновляем список производителей
                    setTimeout(() => {
                        reloadManufacturersList();
                    }, 500);
                } else {
                    showToast('Ошибка: ' + (result.message || 'Неизвестная ошибка'), 'error');
                    btn.disabled = false;
                    btn.innerHTML = originalText;
                }
            } catch (error) {
                console.error('❌ Ошибка привязки:', error);
                showToast('Ошибка привязки пользователя', 'error');
                btn.disabled = false;
                btn.innerHTML = originalText;
            }
        });
    }
    
    // ========== ОТВЯЗКА ПОЛЬЗОВАТЕЛЯ ==========
    unassignBtns.forEach(btn => {
        btn.addEventListener('click', async function() {
            const userId = this.getAttribute('data-user-id');
            const manufacturerId = this.getAttribute('data-manufacturer-id');
            const userName = this.getAttribute('data-user-name') || 'пользователя';
            
            if (!confirm(`Вы уверены, что хотите отвязать ${userName} от производителя?\n\nПользователь потеряет доступ к оборудованию этого производителя.`)) {
                return;
            }
            
            const originalHTML = this.innerHTML;
            this.disabled = true;
            this.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
            
            try {
                const response = await fetch('/admin/manufacturers/unassign-user', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': antiForgeryToken || '',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify({
                        ManufacturerId: parseInt(manufacturerId),
                        UserId: userId
                    })
                });
                
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }
                
                const result = await response.json();
                
                if (result.success) {
                    showToast('Пользователь успешно отвязан', 'success');
                    
                    // Перезагружаем модалку
                    await refreshUserAssignmentModal(manufacturerId, modalEl);
                } else {
                    showToast('Ошибка: ' + (result.message || 'Неизвестная ошибка'), 'error');
                    this.disabled = false;
                    this.innerHTML = originalHTML;
                }
            } catch (error) {
                console.error('❌ Ошибка отвязки:', error);
                showToast('Ошибка отвязки пользователя', 'error');
                this.disabled = false;
                this.innerHTML = originalHTML;
            }
        });
    });
}

// ========== ОБНОВЛЕНИЕ МОДАЛКИ БЕЗ ПЕРЕЗАГРУЗКИ ==========
async function refreshUserAssignmentModal(manufacturerId, currentModalEl) {
    try {
        const response = await fetch(`/admin/manufacturers/users/${manufacturerId}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        
        const html = await response.text();
        
        // Создаем временный контейнер
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;
        
        // Находим новое модальное окно
        const newModalContent = tempDiv.querySelector('#userAssignmentModal .modal-content');
        if (newModalContent && currentModalEl) {
            // Заменяем содержимое текущей модалки
            const modalContent = currentModalEl.querySelector('.modal-content');
            if (modalContent) {
                modalContent.innerHTML = newModalContent.innerHTML;
            }
            
            // Переинициализируем функционал
            setTimeout(() => {
                setupUserAssignmentFunctionality(currentModalEl);
            }, 50);
        }
        
    } catch (error) {
        console.error('❌ Ошибка обновления модалки:', error);
        // Если не удалось обновить, закрываем модалку
        const modalInstance = bootstrap.Modal.getInstance(currentModalEl);
        if (modalInstance) modalInstance.hide();
        showToast('Обновлено', 'success');
    }
}

// ================================================================
// 🎯 ОБНОВЛЕННАЯ ФУНКЦИЯ ОТКРЫТИЯ МОДАЛКИ УПРАВЛЕНИЯ ПОЛЬЗОВАТЕЛЯМИ
// ================================================================
async function openUserAssignmentModal(manufacturerId) {
    try {
        const url = ROUTES.users(manufacturerId);
        console.log(`👥 Загрузка пользователей по URL: ${url}`);

        // Удаляем старую модалку
        removeOldModal("userAssignmentModal");

        const response = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            console.error(`❌ HTTP ${response.status} для ${url}`);
            throw new Error('Ошибка загрузки пользователей');
        }

        const html = await response.text();
        
        // Вставляем HTML
        document.body.insertAdjacentHTML('beforeend', html);
        
        // Инициализируем модалку через глобальную функцию
        setTimeout(() => {
            initUserAssignmentModal();
        }, 50);

    } catch (error) {
        console.error('❌ Ошибка загрузки пользователей:', error);
        showToast('Не удалось загрузить список пользователей', 'danger');
    }
}

// Экспортируем глобальные функции
window.initUserAssignmentModal = initUserAssignmentModal;
window.setupUserAssignmentFunctionality = setupUserAssignmentFunctionality;