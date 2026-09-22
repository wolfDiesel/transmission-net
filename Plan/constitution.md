# Constitution — непреложные законы TransmissionNET

Этот документ фиксирует принципы, которые нельзя нарушать. Любая спека и любой PR должны им соответствовать.

## 1. Назначение

TransmissionNET — десктоп-клиент transmission-daemon. Платформа: **только Linux**, .NET 8, UI — **Avalonia** (`TransmissonNET.App.Avalonia`).

## 2. Слои и зависимости

```
Domain            → без зависимостей
Application       → Domain + Providers.Abstractions
Infrastructure    → Application (реализует порты)
App.Avalonia      → Application
Desktop           → GLib/GTK P/Invoke-хелперы
Providers.*       → только Providers.Abstractions (+ свои зависимости)
```

- Бизнес-логика — только в Application-хендлерах. **Не** в endpoint-лямбдах, **не** в UI-компонентах.
- Application объявляет порты (`I*`), Infrastructure их реализует.
- Domain не ссылается на ASP.NET, Photino, HttpClient, JSON-сериализаторы.
- API DTO — в тонком слое Contracts; маппинг в/из Domain — в хендлерах.
- Запрещены ссылочные циклы; в Domain запрещён `HttpClient`/`System.Net`.

## 3. Именование

| Тип | Паттерн | Пример |
|-----|---------|--------|
| Use case | `{Verb}{Noun}Handler` | `GetTorrentsHandler` |
| Порт | `I{Name}` | `ITransmissionClient` |
| API-модель | `{Name}Dto` | `AppSettingsDto` |
| Domain | обычные имена | `Torrent`, `DaemonConnection` |

## 4. DI и UI

- Хендлеры — scoped, регистрация в `Application/DependencyInjection.cs`; UI вызывает через `HandlerInvoker` (DI-scope на вызов).
- Новые VM — только constructor injection. Статический service locator `AppServices` **не использовать** в новом коде.
- VM — `internal`, наследуют `ViewModelBase`; `[ObservableProperty]`/`[RelayCommand]`.
- VM не обращается к `Application.Current`.
- Провайдеры не содержат `using Avalonia` и не трогают `Dispatcher.UIThread` (login-окна выделяются наружу — `IProviderUiHost`).

## 5. RPC Informational (Transmission)

- 409 + `X-Transmission-Session-Id` → один retry.
- HTTP Basic auth.
- Имена методов: `rpc-version >= 17` → kebab-case, иначе snake_case.

## 6. Безопасность

- Секреты, пароли, куки — **не** в git, не в логах, не в коммитах.
- Признанные долги (plaintext-пароль, plaintext-куки) фиксируются в спеках с приоритетом, а не замалчиваются.

## 7. Релизный workflow («ЧКП»)

1. Обновить `CHANGELOG.md`.
2. Коммит только продуктовых файлов (никогда `.cursor/`, `.vscode/`, `Plan/`, секреты).
3. Push; сборка AppImage — в CI на `release: published`.

## 8. Spec-Driven Development

- `Plan/specs/` — источник правды о «что»; `plan.md` — «как»; `tasks.md` — трекинг.
- Acceptance-критерии — в формате GIVEN/WHEN/THEN.
- Изменение поведения = сначала правка спеки, потом кода.