# Transmission RPC reference

Spec: https://github.com/transmission/transmission/blob/main/docs/rpc-spec.md

## Request shape

- `POST {baseUrl}/transmission/rpc` (default `http://127.0.0.1:9091/transmission/rpc`)
- Body: `{ "method": "<name>", "arguments": { ... } }`
- Headers: `Content-Type: application/json`, optional `Authorization: Basic ...`, `X-Transmission-Session-Id` после первого контакта.

## 409 session bootstrap

```
response = POST(request)
if response.StatusCode == 409:
    sessionId = response.Headers["X-Transmission-Session-Id"]
    request.Headers["X-Transmission-Session-Id"] = sessionId
    response = POST(request)  // единственный retry
return response
```

Session-id хранить на инстансе клиента (singleton/scoped на время жизни приложения).

## Method naming

После `session-get` читаем `rpc-version`:

| rpc-version | Style | Примеры |
|-------------|-------|---------|
| >= 17 | kebab-case | `session-get`, `torrent-get` |
| < 17 | snake_case | `session_get`, `torrent_get` |

## MVP methods

- `session-get` — проверка соединения, определение `rpc-version`.
- `torrent-get` — список с полями ниже.

## torrent-get fields

| Field | Type | UI use |
|-------|------|--------|
| id | number | ключ |
| name | string | заголовок |
| status | number | бейдж статуса |
| percentDone | number | прогресс % |
| rateDownload | number | скорость скачивания |
| rateUpload | number | скорость раздачи |
| eta | number | ETA секунд (-1 = неизвестно) |
| totalSize | number | размер |
| error | number | код ошибки (0 = нет) |
| errorString | string | текст ошибки |

```json
{
  "method": "torrent-get",
  "arguments": {
    "fields": ["id", "name", "status", "percentDone", "rateDownload", "rateUpload", "eta", "totalSize", "error", "errorString"]
  }
}
```

## status (common values)

| Value | Meaning |
|-------|---------|
| 0 | stopped |
| 1 | check wait |
| 2 | checking |
| 3 | download wait |
| 4 | downloading |
| 5 | seed wait |
| 6 | seeding |

Маппинг в `TorrentStatus` enum (Domain).

## session-get (connection test)

Минимальные поля: `version`, `rpc-version`. Успех = RPC доступен и auth верен.

## Errors

Не-success HTTP и `result != "success"` оборачивать в `TransmissionRpcException` с сообщением, которое API-слой маппит в 502.