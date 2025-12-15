window.PartialManager = {
    registry: {},

    register(name, initFn) {
        this.registry[name] = initFn;
        console.log(`📌 PartialManager: зарегистрирован ${name}`);
    },

    init(name, container) {
        const fn = this.registry[name];
        if (!fn) {
            console.warn(`⚠️ PartialManager: нет инициализатора для ${name}`);
            return;
        }

        try {
            console.log(`🚀 PartialManager: инициализация ${name}`);
            fn(container);
        } catch (err) {
            console.error(`❌ Ошибка инициализации ${name}:`, err);
        }
    },

    reload() {
        const active = localStorage.getItem("activeTab");
        if (active) {
            const tab = document.querySelector(`[data-tab="${active}"]`);
            tab?.click();
        }
    }
};
