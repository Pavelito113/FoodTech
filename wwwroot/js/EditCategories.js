// 📘 EditCategories.js — вкладка "Категории оборудования"
console.log("📘 EditCategories.js загружен (актуальная версия)");

(function () {

    if (!window.PartialManager) {
        console.error("❌ PartialManager не найден!");
        return;
    }

    PartialManager.register("EditCategories", initEditCategories);

    // ====================================================================
    // 🚀 ИНИЦИАЛИЗАЦИЯ
    // ====================================================================
    function initEditCategories(container) {
        console.log("🚀 Инициализация EditCategories");

        const root = container.querySelector("#editCategories");
        if (!root) {
            console.warn("❌ EditCategories: корневой элемент не найден");
            return;
        }

        const tbody = root.querySelector("tbody");
        const btnAdd = root.querySelector("#btnAddCategory");

        // Делегирование кликов
        root.addEventListener("click", async (e) => {
            const btn = e.target.closest("button");
            if (!btn) return;

            const row = btn.closest("tr");

            switch (true) {
                case btn.classList.contains("btn-edit"):
                    toggleEdit(row, true);
                    break;

                case btn.classList.contains("btn-cancel"):
                    toggleEdit(row, false);
                    break;

                case btn.classList.contains("btn-save"):
                    await saveCategory(row);
                    break;

                case btn.classList.contains("btn-del"):
                    await deleteCategory(row);
                    break;

                case btn === btnAdd:
                    createNewRow(tbody);
                    break;
            }
        });

        console.log("✅ EditCategories инициализирован");
    }

    // ====================================================================
    // ✏️ ПЕРЕКЛЮЧЕНИЕ РЕЖИМА РЕДАКТИРОВАНИЯ
    // ====================================================================
    function toggleEdit(row, isEdit) {
        const viewName = row.querySelector(".view-name");
        const editInput = row.querySelector(".edit-input");

        const viewButtons = row.querySelector(".view-buttons");
        const editButtons = row.querySelector(".edit-buttons");

        if (isEdit) {
            editInput.value = viewName.textContent.trim();
            viewName.classList.add("d-none");
            viewButtons.classList.add("d-none");
            editInput.classList.remove("d-none");
            editButtons.classList.remove("d-none");
            editInput.focus();
            editInput.select();
        } else {
            editInput.classList.add("d-none");
            editButtons.classList.add("d-none");
            viewName.classList.remove("d-none");
            viewButtons.classList.remove("d-none");
        }
    }

    // ====================================================================
    // 💾 СОХРАНЕНИЕ КАТЕГОРИИ
    // ====================================================================
    async function saveCategory(row) {
        const id = row.dataset.id;
        const name = row.querySelector(".edit-input").value.trim();

        if (!name) {
            showToast("Введите название категории", "warning");
            return;
        }

        const btn = row.querySelector(".btn-save");
        btn.disabled = true;
        btn.innerHTML = `<i class="bi bi-hourglass-split"></i>`;

        const url = id === "new"
            ? "/admin/categories/add"
            : "/admin/categories/update";

        const formData = new URLSearchParams();
        if (id !== "new") formData.append("id", id);
        formData.append("name", name);

        try {
            const res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: formData.toString()
            });

            if (!res.ok) throw new Error("Ошибка связи с сервером");

            const result = await res.json();
            if (!result.success) throw new Error(result.message);

            // Новая запись → выставить реальный ID
            if (id === "new") {
                row.dataset.id = result.id;
                row.querySelector(".id-cell").textContent = result.id;
            }

            row.querySelector(".view-name").textContent = name;
            toggleEdit(row, false);

            showToast(id === "new" ? "Категория добавлена" : "Категория обновлена");
        }
        catch (err) {
            console.error(err);
            showToast("Ошибка: " + err.message, "danger");
        }
        finally {
            btn.disabled = false;
            btn.innerHTML = `<i class="bi bi-check"></i>`;
        }
    }

    // ====================================================================
    // 🗑 УДАЛЕНИЕ
    // ====================================================================
    async function deleteCategory(row) {
        const id = row.dataset.id;
        const name = row.querySelector(".view-name")?.textContent ?? "";

        if (!confirm(`Удалить категорию "${name}"?`)) return;

        if (id === "new") {
            row.remove();
            return;
        }

        try {
            const res = await fetch("/admin/categories/delete", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `id=${id}`
            });

            const result = await res.json();
            if (!result.success) throw new Error(result.message);

            row.remove();
            showToast("Категория удалена", "success");
        }
        catch (err) {
            console.error(err);
            showToast("Ошибка удаления: " + err.message, "danger");
        }
    }

    // ====================================================================
    // ➕ ДОБАВЛЕНИЕ НОВОЙ СТРОКИ
    // ====================================================================
    function createNewRow(tbody) {
        const tr = document.createElement("tr");
        tr.dataset.id = "new";

        tr.innerHTML = `
        <td class="id-cell text-muted">—</td>
        <td>
            <span class="view-name d-none"></span>
            <input type="text" class="form-control form-control-sm edit-input" placeholder="Новая категория">
        </td>
        <td class="text-center">
            <div class="btn-group view-buttons d-none">
                <button class="btn btn-outline-primary btn-sm btn-edit"><i class="bi bi-pencil"></i></button>
                <button class="btn btn-outline-danger btn-sm btn-del"><i class="bi bi-trash"></i></button>
            </div>
            <div class="btn-group edit-buttons">
                <button class="btn btn-success btn-sm btn-save"><i class="bi bi-check"></i></button>
                <button class="btn btn-secondary btn-sm btn-cancel"><i class="bi bi-x"></i></button>
            </div>
        </td>
        `;

        tbody.prepend(tr);

        tr.querySelector(".edit-input").focus();
    }

})();
