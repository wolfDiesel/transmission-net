# Avalonia UI reference

UI проекта: `src/TransmissonNET.App.Avalonia`.

## Rules

- Вызов Application-хендлеров через `HandlerInvoker` + scoped DI; не дублировать RPC-логику.
- Не ссылаться на `TransmissonNET.App` (Photino).
- Linux desktop: `TransmissonNET.Desktop` (tray, single instance, заголовок окна `TransmissionNET`).
- Тема: `ThemeService` + `UiSettings` appearance (паритет с web).
- i18n: `LocalizationService` + embedded `Localization/{en,ru,de,fr}.json` (экспорт `scripts/export-avalonia-locales.mjs`).
- Ловушка неймспейса: корень проекта — `TransmissonNET.App.Avalonia`; для квалифицированных типов Avalonia использовать `global::Avalonia.*`.

## Layout

- `MainWindow`: sidebar (Torrents / Search / Add / Settings), `ContentControl` + `ViewLocator`, status bar.
- Search: чипы провайдеров + query + результаты; Download → `AddTorrentViewModel.OpenFromMetainfoBase64Async`.
- Список хендлеров: `TransmissonNET.Application/DependencyInjection.cs`.

## Mass rename

- Preview: `Features/MassRename/MassRenameEngine.cs` (порт из web `torrentMassRename`).
- Apply: `ExecuteTorrentRenameBatchHandler`.