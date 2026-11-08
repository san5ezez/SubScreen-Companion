# 🖥️ SubScreen Companion

<p align="center">
  <img src="src/SubScreenCompanion/Assets/icon.ico" width="80" alt="SubScreen Companion Logo" />
</p>

<p align="center">
  <b>Идеальное приложение-компаньон для экрана вашего корпуса ПК (JONSBO D31/D41 MESH SC и любых дополнительных суб-дисплеев)</b><br>
  <i>The ultimate companion app for PC case sub-displays (JONSBO D31/D41 Mesh SC, Barrowch, Turing & auxiliary HDMI/Type-C mini-monitors)</i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-0078D6?logo=windows&logoColor=white" alt="Windows 10/11" />
  <img src="https://img.shields.io/badge/License-MIT-green" alt="MIT License" />
  <img src="https://img.shields.io/badge/AIDA64-Not%20Required-success" alt="No AIDA64 Required" />
</p>

---

## 🇷🇺 Описание на русском

Многие современные корпуса ПК (например, **Jonsbo D31 / D41 Mesh SC**, модели от Barrowch и кастомные мониторы) оснащаются встроенным ЖК-дисплеем (8", 8.8", 12.6" и др.), который подключается к видеокарте как дополнительный монитор.

Однако пользователи сталкиваются с неприятными проблемами:
1. **Мышь "улетает" в корпус:** курсор случайно соскакивает на экран корпуса, теряется там или случайно нажимает на элементы.
2. **Нужно покупать и настраивать AIDA64:** шаблоны сложно верстать, лицензия стоит денег.
3. **Разная ориентация:** экран может быть установлен горизонтально (Landscape, например 1920×480) или вертикально (Portrait, 480×1920).
4. **Сворачивание окон:** при играх или работе окна перекрывают сенсорную панель.

**SubScreen Companion** решает все эти проблемы «из коробки»!

### ✨ Главные возможности

- 🔒 **Mouse Barrier Engine (Защита от потери мыши):** 
  - Низкоуровневый перехватчик блокирует переход курсора на экран корпуса. Курсор упирается в границу основного экрана, как будто экран внутри корпуса изолирован!
  - **Горячая клавиша `Ctrl + Alt + M`** или переключатель в трее для мгновенной временной разблокировки мыши, если нужно кликнуть.
- 🔄 **Автоопределение ориентации (Landscape / Portrait):**
  - Приложение автоматически считывает соотношение сторон экрана и адаптирует макет (горизонтальный или вертикальный) без искажений благодаря векторному автомасштабированию `Viewbox`.
- 📌 **Always-on-Top & Безрамочный режим:**
  - Окно занимает весь экран корпуса без рамок, заголовков и панели задач, не перекрывается другими окнами.
- 📊 **Мониторинг оборудования без AIDA64 (LibreHardwareMonitor):**
  - **CPU:** загрузка %, температура ядер/пакета, частота MHz, энергопотребление W.
  - **GPU:** загрузка %, температура, использование видеопамяти VRAM (GB и %).
  - **RAM:** использовано/всего GB, шкала нагрузки %.
  - **Сеть:** текущая скорость отдачи и загрузки (KB/s / MB/s).
  - **Часы и дата:** стильные цифровые часы и календарь.
- 🎨 **5 готовых стилей/пресетов:**
  1. `Cyberpunk Neon` (яркий неоновый циан и фуксия)
  2. `Stealth Dark` (строгий минимализм в графитовых тонах)
  3. `ROG / Gaming Red` (черно-красный агрессивный гейминг)
  4. `Snow / Clean White` (светлая серебристая тема для белых корпусов Jonsbo D31 White)
  5. `Amber Sci-Fi` (теплый янтарный ретро-терминал)
- ⚙️ **Удобное меню в системном трее и отдельное окно настроек:**
  - Окно настроек открывается **на вашем основном мониторе**, где вы управляете мышью.
  - Мгновенное превью тем, выбор любого подключенного экрана, настройка автозапуска.
- 🚀 **Автозапуск с Windows:**
  - Приложение стартует сразу после включения ПК в фоне и выводит мониторинг на экран корпуса.

---

### 🎮 Горячие клавиши

| Хоткей | Действие |
| :--- | :--- |
| `Ctrl + Alt + M` | Временно разблокировать / заблокировать мышь на экране корпуса |
| Двойной клик по иконке в трее | Открыть окно настроек на основном мониторе |

---

### 🛠️ Быстрый старт

1. **Скачайте** готовый релиз из раздела [Releases](../../releases) (файл `SubScreenCompanion.exe`) или соберите из исходников.
2. Подключите экран корпуса к HDMI/Type-C видеокарты и настройте его в параметрах Windows:
   - *Параметры Windows -> Система -> Дисплей -> Расширить эти экраны*.
   - Выберите ориентацию (Альбомная или Книжная) и разрешение (например, 1920×480 или 1280×800).
3. **Запустите `SubScreenCompanion.exe`** (желательно от имени Администратора для чтения температурных датчиков CPU/GPU).
4. Кликните правой кнопкой мыши по иконке в трее возле часов -> выберите **⚙️ Настройки...** для выбора экрана и понравившегося пресета.

---

## 🇬🇧 English Description

**SubScreen Companion** is a dedicated, ultra-lightweight Windows desktop dashboard for secondary PC case mini-displays (e.g. JONSBO D31 / D41 Mesh SC, Barrowch, Turing Smart Screen, or any auxiliary LCD connected via HDMI/DisplayPort/Type-C).

### Features:
- 🔒 **Mouse Barrier Engine:** Prevents your cursor from accidentally slipping into the PC case display.
- 🔄 **Auto-Orientation:** Automatically switches between Landscape and Portrait layouts depending on the display dimensions.
- 📌 **Always on Top:** Borderless fullscreen overlay on the selected screen that never minimizes.
- 📊 **Built-in Hardware Monitor:** Reads CPU/GPU load, temps, VRAM, RAM, and network speeds via `LibreHardwareMonitorLib` — no expensive AIDA64 license needed!
- 🎨 **5 Beautiful Built-in Presets:** Cyberpunk Neon, Stealth Dark, Gaming Red, Clean White, and Amber Sci-Fi.
- ⚙️ **System Tray & Settings UI:** Dedicated configuration tool opening conveniently on your primary monitor.
- 🚀 **Autostart with Windows:** Auto-launches silently on boot.

---

### 📦 Сборка из исходников (Build from Source)

Требуется: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
# Клонирование репозитория
git clone https://github.com/your-username/subscreen-companion.git
cd subscreen-companion

# Сборка проекта
dotnet build -c Release

# Публикация единого исполняемого файла (Self-contained, не требует установки .NET)
dotnet publish src/SubScreenCompanion/SubScreenCompanion.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

Исполняемый файл появится в папке `./publish/SubScreenCompanion.exe`.

---

### 📄 Лицензия

Распространяется под свободной лицензией [MIT](LICENSE).

