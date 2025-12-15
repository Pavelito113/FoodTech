// 📘 EditRequests.js — управление запросами клиентов (исправленная версия)
console.log("📘 EditRequests.js загружен");

(function () {
    if (!window.PartialManager) {
        console.error("❌ PartialManager не найден!");
        return;
    }

    PartialManager.register("RequestManagement", initRequestManagement);

    // ================================================================
    // 🏭 Кэш производителей и выбранные компании
    // ================================================================
    let manufacturersCache = null;
    let selectedManufacturers = new Map(); // key = companyId, value = companyName

    // ================================================================
    // 🚀 Инициализация вкладки "Запросы" (делегирование событий)
    // ================================================================
    function initRequestManagement(container) {
        console.log("🚀 Инициализация RequestManagement (delegated)");

        container.addEventListener("change", function (e) {
            if (e.target.classList.contains("select-status")) {
                if (e.target._previousValue === undefined) {
                    e.target._previousValue = e.target.value;
                }
                handleStatusChange(e);
            }
        });

        container.addEventListener("click", function (e) {
            const detailsBtn = e.target.closest(".btn-details");
            if (detailsBtn) {
                openRequestDetails(detailsBtn.dataset.id);
                return;
            }

            const deleteBtn = e.target.closest(".btn-delete");
            if (deleteBtn) {
                deleteRequest(deleteBtn.dataset.id);
            }
        });

        console.log("✅ RequestManagement инициализирован");
    }

    // ================================================================
    // 🔍 Открытие деталей запроса
    // ================================================================
    async function openRequestDetails(id) {
        try {
            const response = await fetch(`/admin/requests/details/${id}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            if (!response.ok) {
                showToast("Ошибка загрузки запроса", "danger");
                return;
            }

            const html = await response.text();

            const oldModal = document.getElementById("requestDetailsModal");
            if (oldModal) {
                bootstrap.Modal.getInstance(oldModal)?.hide();
                oldModal.remove();
            }

            document.body.insertAdjacentHTML("beforeend", html);

            const modalEl = document.getElementById("requestDetailsModal");
            const modal = new bootstrap.Modal(modalEl);

            await initDetailsModalHandlers(id, modal);
            modal.show();

        } catch (err) {
            console.error(err);
            showToast("Ошибка загрузки данных", "danger");
        }
    }

    // ================================================================
    // 🎯 Инициализация обработчиков в модалке
    // ================================================================
    async function initDetailsModalHandlers(requestId, modalInstance) {
        const modalEl = document.getElementById("requestDetailsModal");
        if (!modalEl) return;

        selectedManufacturers.clear();
        updateSelectedPreview();

        modalEl.querySelector("#toggleManufacturersBtn")
            ?.addEventListener("click", toggleManufacturersList);

        await loadManufacturers();

        modalEl.querySelector("#forwardManufacturerBtn")
            ?.addEventListener("click", () => forwardToManufacturers(requestId, modalInstance));

        modalEl.querySelector("#replyToClientForm")
            ?.addEventListener("submit", e => handleReplyToClient(e, requestId, modalInstance));

        modalEl.querySelector("#markProcessedBtn")
            ?.addEventListener("click", () => markAsProcessed(requestId, modalInstance));

        modalEl.addEventListener("hidden.bs.modal", () => {
            setTimeout(() => modalEl.remove(), 300);
        });
    }

    // ================================================================
    // 🔘 Переключение списка производителей
    // ================================================================
    function toggleManufacturersList() {
        const container = document.getElementById("manufacturersContainer");
        const btn = document.getElementById("toggleManufacturersBtn");

        const isHidden = container.style.display === "none";
        container.style.display = isHidden ? "block" : "none";
        btn.innerHTML = isHidden
            ? '<i class="bi bi-chevron-up"></i> Скрыть список'
            : '<i class="bi bi-chevron-down"></i> Показать список';
    }

    // ================================================================
    // 🏭 Загрузка производителей
    // ================================================================
    async function loadManufacturers() {
        const container = document.getElementById("manufacturersContainer");
        if (!container) return;

        try {
            const response = await fetch("/admin/requests/manufacturers/list", {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });

            manufacturersCache = await response.json();
            updateManufacturersUI(manufacturersCache);

        } catch {
            container.innerHTML = `<div class="alert alert-warning">Ошибка загрузки производителей</div>`;
        }
    }

    // ================================================================
    // 📋 UI производителей
    // ================================================================
    function updateManufacturersUI(manufacturers) {
        const container = document.getElementById("manufacturersContainer");
        if (!container) return;

        if (!manufacturers?.length) {
            container.innerHTML = `<em class="text-muted">Нет производителей</em>`;
            return;
        }

        container.innerHTML = `
            <div class="d-flex flex-wrap gap-2">
                ${manufacturers.map(c => `
                    <label class="form-check form-check-inline">
                        <input class="form-check-input manufacturer-checkbox"
                               type="checkbox"
                               value="${c.companyId}"
                               data-name="${c.companyName}">
                        <span class="small">${c.companyName}</span>
                    </label>
                `).join("")}
            </div>
        `;

        container.querySelectorAll(".manufacturer-checkbox")
            .forEach(cb => cb.addEventListener("change", handleManufacturerSelection));
    }

    // ================================================================
    // 🎯 Выбор производителей
    // ================================================================
    function handleManufacturerSelection(e) {
        const id = e.target.value;
        const name = e.target.dataset.name;

        e.target.checked
            ? selectedManufacturers.set(id, name)
            : selectedManufacturers.delete(id);

        updateSelectedPreview();
    }

    function updateSelectedPreview() {
        document.getElementById("selectedCount").textContent = selectedManufacturers.size;
        document.getElementById("selectedPreview").textContent =
            selectedManufacturers.size
                ? Array.from(selectedManufacturers.values()).join(", ")
                : "Не выбрано";
    }

    // ================================================================
    // 📤 Пересылка
    // ================================================================
    async function forwardToManufacturers(requestId, modalInstance) {
        if (!selectedManufacturers.size) {
            showToast("Выберите производителя", "warning");
            return;
        }

        const note = document.getElementById("forwardNote")?.value || "";

        await Promise.all(
            Array.from(selectedManufacturers.entries()).map(([id, name]) =>
                fetch("/admin/requests/forward", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        ContactRequestId: +requestId,
                        ManufacturerId: +id,
                        ManufacturerName: name,
                        Note: note
                    })
                })
            )
        );

        await updateRequestStatus(requestId, "InProgress", modalInstance);
    }

    // ================================================================
    // 🔄 Статусы и удаление
    // ================================================================
    async function handleStatusChange(e) {
        const select = e.target;
        const id = select.dataset.id;

        try {
            await updateRequestStatus(id, select.value);
        } catch {
            select.value = select._previousValue;
        }
    }

    async function updateRequestStatus(id, status, modal) {
        const res = await fetch("/admin/requests/update-status", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ ContactRequestId: +id, Status: status })
        });

        if (res.ok) {
            document.querySelectorAll(`.select-status[data-id="${id}"]`)
                .forEach(s => s.value = status);
            modal?.hide();
        }
    }

    async function deleteRequest(id) {
        if (!confirm("Удалить запрос?")) return;
        await fetch("/admin/requests/delete", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Id: +id })
        });
        reloadRequestList();
    }

    async function reloadRequestList() {
        const res = await fetch("/admin/requests", { headers: { "X-Requested-With": "XMLHttpRequest" } });
        const html = await res.text();
        document.getElementById("requestsContainer").innerHTML = html;
        initRequestManagement(document.getElementById("requestsContainer"));
    }

})();
