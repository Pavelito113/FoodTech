// 📘 EditRequests.js — управление запросами клиентов
console.log("📘 EditRequests.js загружен");

(function () {
    if (!window.PartialManager) {
        console.error("❌ PartialManager не найден!");
        return;
    }

    PartialManager.register("RequestManagement", initRequestManagement);

    // ================================================================
    // 🚀 Инициализация вкладки "Запросы"
    // ================================================================
    function initRequestManagement(container) {
        console.log("🚀 Инициализация RequestManagement...");

        // Сохраняем предыдущие значения для отката при ошибке
        container.querySelectorAll(".select-status").forEach(select => {
            select._previousValue = select.value;
            select.addEventListener("change", handleStatusChange);
        });

        // Просмотр деталей запроса
        container.querySelectorAll(".btn-details").forEach(btn => {
            btn.addEventListener("click", () => openRequestDetails(btn.dataset.id));
        });

        // Удаление запроса
        container.querySelectorAll(".btn-delete").forEach(btn => {
            btn.addEventListener("click", () => deleteRequest(btn.dataset.id));
        });

        console.log("✅ RequestManagement инициализирован");
    }

    // ================================================================
    // 🔄 Обновление статуса запроса
    // ================================================================
    async function handleStatusChange(e) {
        const select = e.target;
        const id = select.dataset.id;
        const status = select.value;

        try {
            const response = await fetch("/admin/requests/update-status", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: JSON.stringify({
                    ContactRequestId: parseInt(id),
                    Status: status
                })
            });

            const result = await response.json();

            if (result.success) {
                showToast("Статус обновлен", "success");
                select._previousValue = status; // Обновляем предыдущее значение
                
                // Обновляем визуально строку в таблице
                updateStatusInTable(id, status);
                
                // Обновляем список если статус стал "Done"
                if (status === "Done") {
                    reloadRequestList();
                }
            } else {
                showToast(result.message || "Ошибка обновления статуса", "danger");
                // Восстанавливаем предыдущее значение
                select.value = select._previousValue;
            }
        } catch (err) {
            console.error("❌ Ошибка обновления статуса:", err);
            showToast("Ошибка соединения", "danger");
            select.value = select._previousValue;
        }
    }

    // ================================================================
    // 🔍 Открытие деталей запроса (модалка)
    // ================================================================
    async function openRequestDetails(id) {
        console.log(`📥 Загрузка деталей запроса #${id}`);

        try {
            const response = await fetch(`/admin/requests/details/${id}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            if (!response.ok) {
                if (response.status === 404) {
                    showToast("Запрос не найден", "warning");
                } else {
                    throw new Error(`Ошибка сервера: ${response.status}`);
                }
                return;
            }

            const html = await response.text();
            
            // Удаляем старую модалку если есть
            const oldModal = document.getElementById("requestDetailsModal");
            if (oldModal) {
                const modalInstance = bootstrap.Modal.getInstance(oldModal);
                if (modalInstance) modalInstance.hide();
                oldModal.remove();
            }

            // Добавляем новую модалку в DOM
            document.body.insertAdjacentHTML("beforeend", html);

            // Инициализируем модалку
            const modalEl = document.getElementById("requestDetailsModal");
            const modal = new bootstrap.Modal(modalEl);
            
            // Назначаем обработчики для новой модалки
            initDetailsModalHandlers(id, modal);

            modal.show();

        } catch (err) {
            console.error("❌ Ошибка загрузки деталей:", err);
            showToast("Ошибка загрузки данных", "danger");
        }
    }

    // ================================================================
    // 🎯 Инициализация обработчиков в модалке деталей
    // ================================================================
    function initDetailsModalHandlers(requestId, modalInstance) {
        const modalEl = document.getElementById("requestDetailsModal");
        if (!modalEl) return;

        // 1. Пересылка производителю
        const forwardBtn = modalEl.querySelector("#forwardManufacturerBtn");
        if (forwardBtn) {
            forwardBtn.addEventListener("click", () => forwardToManufacturer(requestId, modalInstance));
        }

        // 2. Ответ клиенту
        const replyForm = modalEl.querySelector("#replyToClientForm");
        if (replyForm) {
            replyForm.addEventListener("submit", (e) => handleReplyToClient(e, requestId, modalInstance));
        }

        // 3. Пометить как обработанное
        const markProcessedBtn = modalEl.querySelector("#markProcessedBtn");
        if (markProcessedBtn) {
            markProcessedBtn.addEventListener("click", () => markAsProcessed(requestId, modalInstance));
        }

        // 4. Удаление модалки при закрытии
        modalEl.addEventListener("hidden.bs.modal", () => {
            setTimeout(() => {
                if (modalEl.parentNode) {
                    modalEl.remove();
                }
            }, 300);
        });
    }

    // ================================================================
    // 📤 Пересылка заявки на завод/производителю
    // ================================================================
    async function forwardToManufacturer(requestId, modalInstance) {
        const note = document.getElementById("forwardNote")?.value || "";
        const forwardBtn = document.getElementById("forwardManufacturerBtn");
        
        if (!confirm("Переслать этот запрос производителю?")) return;

        // Блокируем кнопку на время запроса
        if (forwardBtn) {
            const originalText = forwardBtn.innerHTML;
            forwardBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Отправка...';
            forwardBtn.disabled = true;
        }

        try {
            const response = await fetch("/admin/requests/forward", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: JSON.stringify({
                    ContactRequestId: parseInt(requestId),
                    Note: note
                })
            });

            const result = await response.json();

            if (result.success) {
                showToast("Запрос переслан производителю", "success");
                
                // Закрываем модалку
                if (modalInstance) {
                    modalInstance.hide();
                }
                
                // Обновляем список запросов (локальное обновление статуса)
                updateStatusInTable(requestId, "InProgress");
                
            } else {
                showToast(result.message || "Ошибка при пересылке", "danger");
            }
        } catch (err) {
            console.error("❌ Ошибка пересылки:", err);
            showToast("Ошибка соединения", "danger");
        } finally {
            // Восстанавливаем кнопку
            if (forwardBtn) {
                forwardBtn.innerHTML = '<i class="bi bi-send"></i> Переслать производителю';
                forwardBtn.disabled = false;
            }
        }
    }

    // ================================================================
    // ✉ Ответ клиенту
    // ================================================================
    async function handleReplyToClient(e, requestId, modalInstance) {
        e.preventDefault();
        
        const form = e.target;
        const formData = new FormData(form);
        const data = Object.fromEntries(formData.entries());
        const submitBtn = form.querySelector('button[type="submit"]');

        if (!data.Subject || !data.Body) {
            showToast("Заполните тему и текст сообщения", "warning");
            return;
        }

        // Блокируем кнопку отправки
        if (submitBtn) {
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Отправка...';
            submitBtn.disabled = true;
        }

        try {
            const response = await fetch("/admin/requests/reply", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: JSON.stringify({
                    ContactRequestId: parseInt(requestId),
                    Subject: data.Subject,
                    Body: data.Body
                })
            });

            const result = await response.json();

            if (result.success) {
                showToast("Ответ отправлен клиенту", "success");
                form.reset();
                
                // Помечаем как обработанное
                await updateRequestStatus(requestId, "Done", modalInstance);
                
            } else {
                showToast(result.message || "Ошибка отправки", "danger");
            }
        } catch (err) {
            console.error("❌ Ошибка отправки ответа:", err);
            showToast("Ошибка соединения", "danger");
        } finally {
            // Восстанавливаем кнопку
            if (submitBtn) {
                submitBtn.innerHTML = '<i class="bi bi-reply"></i> Отправить клиенту';
                submitBtn.disabled = false;
            }
        }
    }

    // ================================================================
    // ✅ Пометить как обработанное
    // ================================================================
    async function markAsProcessed(requestId, modalInstance) {
        const markBtn = document.getElementById("markProcessedBtn");
        
        // Блокируем кнопку
        if (markBtn) {
            const originalText = markBtn.innerHTML;
            markBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Обновление...';
            markBtn.disabled = true;
        }

        try {
            await updateRequestStatus(requestId, "Done", modalInstance);
        } finally {
            // Восстанавливаем кнопку
            if (markBtn) {
                markBtn.innerHTML = '<i class="bi bi-check2-circle"></i> Пометить как обработанное';
                markBtn.disabled = false;
            }
        }
    }

    // ================================================================
    // 🔄 Обновление статуса запроса (вспомогательная функция)
    // ================================================================
    async function updateRequestStatus(requestId, status, modalInstance) {
        try {
            const response = await fetch("/admin/requests/update-status", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: JSON.stringify({
                    ContactRequestId: parseInt(requestId),
                    Status: status
                })
            });

            const result = await response.json();

            if (result.success) {
                const statusText = status === "Done" ? "обработанный" : "в работе";
                showToast(`Запрос помечен как ${statusText}`, "success");
                
                // Закрываем модалку
                if (modalInstance) {
                    modalInstance.hide();
                }
                
                // Обновляем в таблице
                updateStatusInTable(requestId, status);
                
                // Полностью перезагружаем список если нужно
                if (status === "Done") {
                    setTimeout(() => reloadRequestList(), 500);
                }
                
            } else {
                showToast(result.message || "Ошибка обновления статуса", "danger");
            }
        } catch (err) {
            console.error("❌ Ошибка обновления статуса:", err);
            showToast("Ошибка соединения", "danger");
        }
    }

    // ================================================================
    // ❌ Удаление запроса
    // ================================================================
    async function deleteRequest(id) {
        if (!confirm("Удалить этот запрос?")) return;

        const deleteBtn = document.querySelector(`.btn-delete[data-id="${id}"]`);
        if (deleteBtn) {
            deleteBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
            deleteBtn.disabled = true;
        }

        try {
            const response = await fetch("/admin/requests/delete", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: JSON.stringify({ Id: parseInt(id) })
            });

            const result = await response.json();

            if (result.success) {
                showToast("Запрос удален", "success");
                reloadRequestList();
            } else {
                showToast(result.message || "Ошибка удаления", "danger");
                if (deleteBtn) {
                    deleteBtn.innerHTML = '<i class="bi bi-trash"></i>';
                    deleteBtn.disabled = false;
                }
            }
        } catch (err) {
            console.error("❌ Ошибка удаления:", err);
            showToast("Ошибка соединения", "danger");
            if (deleteBtn) {
                deleteBtn.innerHTML = '<i class="bi bi-trash"></i>';
                deleteBtn.disabled = false;
            }
        }
    }

    // ================================================================
    // 🔄 Обновление статуса в таблице (локально)
    // ================================================================
    function updateStatusInTable(requestId, status) {
        // Обновляем select в таблице
        const statusSelect = document.querySelector(`.select-status[data-id="${requestId}"]`);
        if (statusSelect) {
            statusSelect.value = status;
            statusSelect._previousValue = status;
            
            // Обновляем строку визуально
            const row = statusSelect.closest('tr');
            if (row) {
                // Можно добавить классы для визуального выделения
                row.classList.remove('table-primary', 'table-info', 'table-success');
                switch(status) {
                    case 'New':
                        row.classList.add('table-primary');
                        break;
                    case 'InProgress':
                        row.classList.add('table-info');
                        break;
                    case 'Done':
                        row.classList.add('table-success');
                        // Скрываем кнопку "Пометить как обработанное" если есть в модалке
                        const markBtn = document.getElementById("markProcessedBtn");
                        if (markBtn) {
                            markBtn.style.display = 'none';
                        }
                        break;
                }
            }
        }
    }

    // ================================================================
    // 🔄 Перезагрузка списка запросов
    // ================================================================
    async function reloadRequestList() {
        try {
            const response = await fetch("/admin/requests", {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            if (!response.ok) throw new Error("Ошибка загрузки");

            const html = await response.text();
            
            // Находим контейнер с запросами
            const container = document.querySelector(".container-fluid.mt-3");
            if (container) {
                // Заменяем содержимое
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = html;
                const newContent = tempDiv.querySelector(".container-fluid.mt-3");
                
                if (newContent) {
                    container.innerHTML = newContent.innerHTML;
                    
                    // Инициализируем заново
                    initRequestManagement(container);
                    
                    showToast("Список обновлен", "info");
                }
            }
        } catch (err) {
            console.error("❌ Ошибка перезагрузки списка:", err);
            showToast("Ошибка обновления списка", "warning");
        }
    }

})();