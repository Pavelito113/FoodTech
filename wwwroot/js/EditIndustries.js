// 📘 EditIndustries.js — вкладка "Отрасли"
console.log("📘 EditIndustries.js загружен (актуальная версия)");

(function () {

    if (!window.PartialManager) {
        console.error("❌ PartialManager не найден!");
        return;
    }

    // Регистрация вкладки
    PartialManager.register("EditIndustries", initEditIndustries);

    // ================================================================
    // 🚀 ИНИЦИАЛИЗАЦИЯ
    // ================================================================
    function initEditIndustries(container) {
        console.log("🚀 Инициализация EditIndustries");

        const root = container.querySelector("#editIndustries");
        if (!root) {
            console.warn("❌ EditIndustries: корневой элемент не найден");
            return;
        }

        const tbody = root.querySelector("tbody");
        const btnAdd = root.querySelector("#btnAddIndustry");

        // Все клики — через делегирование
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
                    await saveIndustry(row);
                    break;

                case btn.classList.contains("btn-del"):
                    await deleteIndustry(row);
                    break;

                case btn === btnAdd:
                    createNewRow(tbody);
                    break;
            }
        });

        console.log("✅ EditIndustries инициализирован");
    }

    // ================================================================
    // ✏️ ПЕРЕКЛЮЧЕНИЕ РЕЖИМА РЕДАКТИРОВАНИЯ
    // ================================================================
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

    // ================================================================
    // 💾 СОХРАНЕНИЕ
    // ================================================================
    async function saveIndustry(row) {
        const id = row.dataset.id;
        const name = row.querySelector(".edit-input").value.trim();

        if (!name) {
            showToast("Введите название отрасли", "warning");
            return;
        }

        const btn = row.querySelector(".btn-save");
        btn.disabled = true;
        btn.innerHTML = `<i class="bi bi-hourglass-split"></i>`;

        const url = id === "new"
            ? "/admin/industries/add"
            : "/admin/industries/update";

        const formData = new URLSearchParams();
        if (id !== "new") formData.append("id", id);
        formData.append("name", name);

        try {
            const res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: formData.toString()
            });

            if (!res.ok) throw new Error("Сервер вернул ошибку");

            const result = await res.json();
            if (!result.success) throw new Error(result.message);

            // Новая запись — обновляем ID
            if (id === "new") {
                row.dataset.id = result.id;
                row.querySelector(".id-cell").textContent = result.id;
            }

            row.querySelector(".view-name").textContent = name;
            toggleEdit(row, false);

            showToast(id === "new" ? "Отрасль добавлена" : "Изменения сохранены");
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

    // ================================================================
    // 🗑 УДАЛЕНИЕ
    // ================================================================
    async function deleteIndustry(row) {
        const id = row.dataset.id;
        const name = row.querySelector(".view-name").textContent;

        if (!confirm(`Удалить отрасль "${name}"?`)) return;

        // Если новая строка — просто удалить
        if (id === "new") {
            row.remove();
            return;
        }

        try {
            const res = await fetch("/admin/industries/delete", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `id=${id}`
            });

            const result = await res.json();
            if (!result.success) throw new Error(result.message);

            row.remove();
            showToast("Отрасль удалена", "success");
        }
        catch (err) {
            console.error(err);
            showToast("Ошибка удаления: " + err.message, "danger");
        }
    }

    // ================================================================
    // ➕ ДОБАВЛЕНИЕ НОВОЙ В СЕРЕДИНЕ ТАБЛИЦЫ
    // ================================================================
    function createNewRow(tbody) {
        const tr = document.createElement("tr");
        tr.dataset.id = "new";

        tr.innerHTML = `
        <td class="id-cell text-muted">—</td>
        <td>
            <span class="view-name d-none"></span>
            <input type="text" class="form-control form-control-sm edit-input" placeholder="Новая отрасль" />
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
