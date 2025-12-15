const fs = require('fs');
const path = require('path');

// Функция для копирования файлов
function copyFile(source, target) {
    if (fs.existsSync(source)) {
        fs.mkdirSync(path.dirname(target), { recursive: true });
        fs.copyFileSync(source, target);
        console.log(`✓ Скопирован: ${target}`);
    } else {
        console.log(`✗ Файл не найден: ${source}`);
    }
}

// Функция для копирования всей папки
function copyFolder(source, target) {
    if (fs.existsSync(source)) {
        fs.mkdirSync(target, { recursive: true });
        const files = fs.readdirSync(source);
        
        files.forEach(file => {
            const sourcePath = path.join(source, file);
            const targetPath = path.join(target, file);
            
            if (fs.statSync(sourcePath).isDirectory()) {
                copyFolder(sourcePath, targetPath);
            } else {
                copyFile(sourcePath, targetPath);
            }
        });
    }
}

console.log('📦 Копирование Bootstrap и Bootstrap Icons...');

// Копируем Bootstrap CSS
copyFile('node_modules/bootstrap/dist/css/bootstrap.min.css', 'wwwroot/css/bootstrap.min.css');
copyFile('node_modules/bootstrap/dist/css/bootstrap.min.css.map', 'wwwroot/css/bootstrap.min.css.map');
copyFile('node_modules/bootstrap/dist/css/bootstrap.rtl.min.css', 'wwwroot/css/bootstrap.rtl.min.css');
copyFile('node_modules/bootstrap/dist/css/bootstrap.rtl.min.css.map', 'wwwroot/css/bootstrap.rtl.min.css.map');

// Копируем Bootstrap JS
copyFile('node_modules/bootstrap/dist/js/bootstrap.bundle.min.js', 'wwwroot/js/bootstrap.bundle.min.js');
copyFile('node_modules/bootstrap/dist/js/bootstrap.bundle.min.js.map', 'wwwroot/js/bootstrap.bundle.min.js.map');
copyFile('node_modules/bootstrap/dist/js/bootstrap.min.js', 'wwwroot/js/bootstrap.min.js');
copyFile('node_modules/bootstrap/dist/js/bootstrap.min.js.map', 'wwwroot/js/bootstrap.min.js.map');

// Копируем Bootstrap Icons
copyFile('node_modules/bootstrap-icons/font/bootstrap-icons.css', 'wwwroot/css/bootstrap-icons.css');
copyFolder('node_modules/bootstrap-icons/font/fonts', 'wwwroot/fonts');

console.log('✅ Все файлы успешно скопированы в wwwroot!');