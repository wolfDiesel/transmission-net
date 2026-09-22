# Architecture reference

## Dependency diagram

```
App.Avalonia ──uses──▶ Application ◄──implements── Infrastructure
                          │ uses
                          ▼
                        Domain
```

Providers: `providers/*.dll` ссылаются только на `Providers.Abstractions`.
Desktop (`TransmissonNET.Desktop`) — GLib/GTK P/Invoke-хелперы, общий для Photino и Avalonia.

## Project map

| Проект | Роль |
|--------|------|
| `TransmissonNET.Domain` | сущности, records, enum — без фреймворков |
| `TransmissonNET.Application` | use cases (`*Handler`), порты (`I*`) |
| `TransmissonNET.Infrastructure` | `ITransmissionClient`, `ISettingsStore`, provider loader |
| `TransmissonNET.Providers.Abstractions` | контракт `ITorrentProvider` (плагины) |
| `TransmissonNET.App.Avalonia` | Avalonia UI |
| `TransmissonNET.Desktop` | tray/single-instance/.torrent CLI |
| `providers/*.dll` | плагины поиска (RuTracker, LostFilm, Kinozal) |

## Forbidden

- Логика `Torrent` внутри UI-компонентов (только форматирование/отображение).
- Прямой вызов `TransmissionRpcClient` из точек входа UI — всегда через handler.
- Хранение RPC session-id в состоянии UI.

## Naming

| Kind | Pattern | Example |
|------|---------|---------|
| Use case | `{Verb}{Noun}Handler` | `GetTorrentsHandler` |
| Port | `I{Name}` | `ITransmissionClient` |
| API model | `{Name}Dto` | `AppSettingsDto` |
| Domain | plain names | `Torrent`, `DaemonConnection` |

## PR checklist

- [ ] Нет ссылочных циклов
- [ ] Нет `HttpClient`/`System.Net` в Domain
- [ ] Один endpoint = один handler
- [ ] Новая RPC/фича = порт + handler + инфраструктура + тест
- [ ] Тесты зелёные (`dotnet test`)

## Extension pattern

Новая фича в sidebar: Application handler + (при необходимости) порт/инфраструктуру + VM/View. Не раздувать `MainWindowViewModel` — выделять под-VM.