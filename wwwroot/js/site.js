// -------------------------------------------
// 🌐 Глобальные утилиты для всего сайта
// -------------------------------------------

document.addEventListener("DOMContentLoaded", () => {
    console.log("🌍 site.js initialized");
});

// ------------------------------------------------------
// 🟢 Универсальный Bootstrap Toast
// ------------------------------------------------------
window.showToast = (message, type = "success") => {
    let container = document.getElementById("toastContainer");

    if (!container) {
        container = document.createElement("div");
        container.id = "toastContainer";
        container.className = "toast-container position-fixed top-0 end-0 p-3";
        document.body.appendChild(container);
    }

    const id = "toast-" + Date.now();

    container.insertAdjacentHTML(
        "beforeend",
        `
        <div id="${id}" class="toast align-items-center text-bg-${type} border-0 show" role="alert">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto"
                        data-bs-dismiss="toast"></button>
            </div>
        </div>
    `
    );

    const toastEl = document.getElementById(id);
    const bsToast = bootstrap.Toast.getOrCreateInstance(toastEl);

    bsToast.show();

    setTimeout(() => {
        toastEl.remove();
    }, 5000);
};

// ------------------------------------------------------
// 🟡 Безопасный JSON парсер
// ------------------------------------------------------
window.safeJson = (str, fallback = null) => {
    try {
        return JSON.parse(str);
    } catch {
        return fallback;
    }
};

// ------------------------------------------------------
// 🔵 Debounce — задержка вызовов функции
// ------------------------------------------------------
window.debounce = (fn, delay = 300) => {
    let timer = null;

    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => fn(...args), delay);
    };
};

// ------------------------------------------------------
// 🟣 Универсальный GET запрос с JSON
// ------------------------------------------------------
window.fetchJson = async (url, options = {}) => {
    try {
        const resp = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" },
            ...options
        });

        if (!resp.ok) {
            throw new Error(`Server returned ${resp.status}`);
        }

        return await resp.json();
    } catch (err) {
        console.error("❌ fetchJson error:", err);
        showToast("Ошибка загрузки данных", "danger");
        throw err;
    }
};

// ------------------------------------------------------
// 🔴 POST формы (включая модалки)
// ------------------------------------------------------
window.postForm = async (url, formEl) => {
    try {
        const formData = new FormData(formEl);

        const resp = await fetch(url, {
            method: "POST",
            body: formData,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        const text = await resp.text();

        if (!resp.ok) {
            console.error("❌ Ошибка от сервера:", text);
            showToast("Ошибка сохранения", "danger");
            return { ok: false, html: text };
        }

        return { ok: true, html: text };
    } catch (err) {
        console.error("❌ postForm error:", err);
        showToast("Ошибка отправки формы", "danger");
        return { ok: false, html: "" };
    }
};

// ------------------------------------------------------
// 🟠 Получить валидатор ASP.NET MVC из partial
// (Для корректной работы form validation внутри вкладок)
// ------------------------------------------------------
window.reparseFormValidation = () => {
    if (window.jQuery && window.jQuery.validator) {
        $("form").removeData("validator");
        $("form").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse("form");
    }
};
