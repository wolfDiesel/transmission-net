# Photino + Kestrel host reference — LEGACY

> Устарело: переведено на Avalonia (`specs/001-avalonia-ui/`).

## Startup pattern

1. `WebApplication.CreateBuilder()` — регистрация Application/Infrastructure сервисов.
2. `app.UseStaticFiles()` + `MapFallbackToFile("index.html")` для SPA.
3. Маппинг `/api/*` Minimal API.
4. Bind Kestrel на `http://127.0.0.1:0` (эфемерный порт); читать назначенный порт.
5. `new PhotinoWindow().SetTitle("TransmissionNET").Load($"http://127.0.0.1:{port}")`.
6. Закрытие окна → `app.StopAsync()` и выход.

Same origin: React вызывает `/api/...` относительными URL — без CORS.

## Linux deps (Fedora)

```bash
sudo dnf install webkit2gtk4.1-devel
```

При падении Photino — понятное stderr-сообщение с именем пакета.

## wwwroot

- Dev: `npm run dev` + Vite proxy к API.
- Release: MSBuild target `BuildWeb` — `npm ci && npm run build` в `web/transmission-ui`, копирование `dist/**` в `TransmissonNET.App/wwwroot`.

## Window size

`AppSettings.UiSettings.WindowWidth/Height` из `ISettingsStore` (defaults 1280×800).

## Do not

- Открывать `file://` URL в Photino для SPA.
- Поднимать второй публичный listener — только localhost.