// 📘 EditUsers.js — логика вкладки "Пользователи"
console.log("📘 EditUsers.js загружен");

if (!window.PartialManager) {
    console.error("❌ PartialManager не найден!");
} else {
    PartialManager.register("EditUsers", initEditUsersModule);
}

// Глобальные переменные для модалок
let editUserModal = null;
let addUserModal = null;
let tempPasswordModal = null;

// ====================================================================
// 🚀 ГЛАВНАЯ ТОЧКА ВХОДА ДЛЯ PartialManager
// ====================================================================
function initEditUsersModule(container) {
    console.log("🚀 Init: EditUsers");

    // Инициализируем модалки при первом запуске
    initUserModals();
    
    bindAddUser(container);
    bindEditUser(container);
    bindUpdateRole(container);
    bindFreezeUser(container);
    bindDeleteUser(container);

    console.log("✅ EditUsers готов");
}

// ====================================================================
// 🏗 ИНИЦИАЛИЗАЦИЯ МОДАЛОК ПОЛЬЗОВАТЕЛЕЙ
// ====================================================================
function initUserModals() {
    // Инициализация модалки редактирования
    const editModalEl = document.getElementById('editUserModal');
    if (editModalEl && !editUserModal) {
        editUserModal = new bootstrap.Modal(editModalEl);
    }

    // Инициализация модалки добавления
    const addModalEl = document.getElementById('addUserModal');
    if (addModalEl && !addUserModal) {
        addUserModal = new bootstrap.Modal(addModalEl);
    }

    // Инициализация модалки временного пароля
    const passModalEl = document.getElementById('tempPasswordModal');
    if (passModalEl && !tempPasswordModal) {
        tempPasswordModal = new bootstrap.Modal(passModalEl);
        
        // Настройка копирования
        document.getElementById('copyEmailBtn')?.addEventListener('click', () => {
            copyToClipboard('tempPasswordEmail');
        });
        
        document.getElementById('copyPasswordBtn')?.addEventListener('click', () => {
            copyToClipboard('tempPasswordValue');
        });
    }
}

function copyToClipboard(elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    element.select();
    element.setSelectionRange(0, 99999); // Для мобильных устройств
    
    try {
        document.execCommand('copy');
        showToast('Скопировано в буфер обмена', 'success');
    } catch (err) {
        console.error('Ошибка копирования:', err);
        showToast('Не удалось скопировать', 'danger');
    }
}
// ====================================================================
// ➕ ДОБАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯ (как раньше работало)
// ====================================================================
function bindAddUser(root) {
    const form = document.getElementById("addUserForm");
    const submitBtn = document.getElementById("addUserSubmitBtn");

    if (!form || !submitBtn) return;

    form.onsubmit = async e => {
        e.preventDefault();

        const original = submitBtn.innerHTML;
        submitBtn.innerHTML = spinner();
        submitBtn.disabled = true;

        try {
            const payload = Object.fromEntries(new FormData(form).entries());

            const res = await fetch("/admin/users/add", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const json = await res.json(); // Просто парсим без safeJson!

            if (!json.success) {
                showToast(json.message, "danger");
                return;
            }

            // Показываем пароль
            if (json.tempPassword) {
                document.getElementById('tempPasswordEmail').value = payload.Email;
                document.getElementById('tempPasswordValue').value = json.tempPassword;
                const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('tempPasswordModal'));
                modal.show();
            }

            showToast("Пользователь создан", "success");

            // Закрываем модалку добавления
            const addModal = bootstrap.Modal.getInstance(document.getElementById('addUserModal'));
            if (addModal) addModal.hide();
            
            form.reset();

            await reloadUsersPartial();
        } catch (error) {
            console.error("Ошибка при добавлении пользователя:", error);
            showToast("Ошибка при создании пользователя", "danger");
        } finally {
            submitBtn.innerHTML = original;
            submitBtn.disabled = false;
        }
    };
}

// ====================================================================
// ✏ РЕДАКТИРОВАНИЕ ПОЛЬЗОВАТЕЛЯ (просто как раньше)
// ====================================================================
function bindEditUser(root) {
    const form = document.getElementById("editUserForm");
    const submitBtn = document.getElementById("editUserSubmitBtn");

    if (!form || !submitBtn) return;

    // Клик по кнопке редактирования
    root.addEventListener('click', function (ev) {
        const btn = ev.target.closest('.edit-user');
        if (!btn) return;

        ev.preventDefault();

        const tr = btn.closest('tr');
        if (!tr) return;

        // Берем данные из таблицы
        const cells = [...tr.children];
        form.querySelector("[name='Id']").value = tr.dataset.userId || "";
        form.querySelector("[name='Name']").value = (cells[0] && cells[0].textContent || "").trim();
        form.querySelector("[name='Email']").value = (cells[1] && cells[1].textContent || "").trim();
        form.querySelector("[name='Company']").value = (cells[2] && cells[2].textContent || "").trim();
        form.querySelector("[name='Phone']").value = (cells[3] && cells[3].textContent || "").trim();

        // Показываем модалку
        const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('editUserModal'));
        modal.show();
    });

    // Отправка формы
    form.onsubmit = async e => {
        e.preventDefault();

        const original = submitBtn.innerHTML;
        submitBtn.innerHTML = spinner();
        submitBtn.disabled = true;

        try {
            const data = Object.fromEntries(new FormData(form));
            const res = await fetch("/admin/users/edit", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data)
            });

            const json = await res.json(); // Просто парсим!

            if (!json.success) {
                showToast(json.message, "danger");
                return;
            }

            showToast("Изменения сохранены", "success");
            
            // Закрываем модалку
            const modal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            if (modal) modal.hide();
            
            await reloadUsersPartial();
        } catch (error) {
            console.error("Ошибка при редактировании:", error);
            showToast("Ошибка при сохранении", "danger");
        } finally {
            submitBtn.innerHTML = original;
            submitBtn.disabled = false;
        }
    };
}

// ====================================================================
// 🎭 ОБНОВЛЕНИЕ РОЛИ
// ====================================================================
function bindUpdateRole(root) {
    root.querySelectorAll(".user-role").forEach(select => {
        select.onchange = async () => {
            const payload = { userId: select.dataset.id, role: select.value };

            const res = await fetch("/admin/users/update-role", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const json = await safeJson(res);
            showToast(json.message, json.success ? "success" : "danger");
        };
    });
}

// ====================================================================
// ❄ FREEZE
// ====================================================================
function bindFreezeUser(root) {
    root.querySelectorAll(".freeze-user").forEach(btn => {
        btn.onclick = async () => {
            const tr = btn.closest("tr");
            const id = tr.dataset.userId;
            const isFrozen = tr.classList.contains("table-secondary");
            
            // Извлекаем имя без иконки
            let userName = "";
            if (tr.children[0]) {
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = tr.children[0].innerHTML;
                const icon = tempDiv.querySelector('i.bi-snow');
                if (icon) icon.remove();
                userName = tempDiv.textContent.trim();
            }

            if (!confirm(isFrozen
                ? `Разморозить пользователя "${userName}"?`
                : `Заморозить пользователя "${userName}"?`)) return;

            const res = await fetch("/admin/users/freeze", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(id)
            });

            const json = await safeJson(res);
            if (!json.success) {
                showToast(json.message, "danger");
                return;
            }

            updateFreezeUI(tr, btn, json.frozen);
            showToast(json.frozen ? "Пользователь заморожен" : "Пользователь активирован", "success");
        };
    });
}

function updateFreezeUI(tr, btn, isFrozen) {
    tr.classList.toggle("table-secondary", isFrozen);
    tr.classList.toggle("text-muted", isFrozen);

    const statusCell = tr.children[5];
    statusCell.innerHTML = isFrozen
        ? '<span class="badge bg-warning text-dark"><i class="bi bi-snow"></i> Заморожен</span>'
        : '<span class="badge bg-success"><i class="bi bi-check-circle"></i> Активен</span>';

    btn.classList.toggle("btn-warning", isFrozen);
    btn.classList.toggle("btn-outline-warning", !isFrozen);
}

// ====================================================================
// 🗑 DELETE
// ====================================================================
function bindDeleteUser(root) {
    root.querySelectorAll(".delete-user").forEach(btn => {
        btn.onclick = async () => {
            const id = btn.closest("tr")?.dataset.userId;
            if (!id) return;

            if (!confirm("Удалить пользователя?")) return;

            const res = await fetch("/admin/users/delete", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(id)
            });

            const json = await safeJson(res);

            if (json.success) {
                showToast("Пользователь удалён", "success");
                await reloadUsersPartial();
            } else {
                showToast(json.message, "danger");
            }
        };
    });
}

// ====================================================================
// 🔄 Перезагрузка partial
// ====================================================================
async function reloadUsersPartial() {
    const container = document.querySelector("#editUsersContainer");
    if (!container) return;

    const res = await fetch("/admin/users", {
        headers: { "X-Requested-With": "XMLHttpRequest" }
    });

    if (!res.ok) {
        console.error("❌ Ошибка загрузки пользователей:", res.status);
        showToast("Ошибка загрузки данных", "danger");
        return;
    }

    container.outerHTML = await res.text();

    // После перезагрузки инициализируем модуль снова
    const newContainer = document.querySelector("#editUsersContainer");
    if (newContainer) {
        initEditUsersModule(newContainer);
    }
}

// ====================================================================
// 🔧 UTILS
// ====================================================================
function spinner() {
    return `<span class="spinner-border spinner-border-sm"></span>`;
}

// Устаревшая функция для обратной совместимости
function showPasswordModal(tempPassword, email) {
    console.warn("⚠️ Используйте tempPasswordModal вместо showPasswordModal");
    if (tempPasswordModal) {
        document.getElementById('tempPasswordEmail').value = email;
        document.getElementById('tempPasswordValue').value = tempPassword;
        tempPasswordModal.show();
    }
}