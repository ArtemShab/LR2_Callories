# AI Calorie Counter (LR2)

ASP.NET Core MVC застосунок: користувач завантажує фото страви, нейромережа визначає назву, калорійність і БЖУ.

## Вимоги

- .NET SDK 10

## Швидкий старт

```powershell
dotnet run --project LR2_Callories
```

Якщо API-ключ не заданий, автоматично вмикається заглушка `MockFoodAnalysisService` — сайт повністю працює для демонстрації без інтернету та витрат.

## Підключення Gemini

1. Отримати ключ у Google AI Studio: <https://aistudio.google.com/apikey>
2. Зберегти ключ локально (поза репозиторієм):

```powershell
cd LR2_Callories
dotnet user-secrets set "Ai:ApiKey" "ВАШ_КЛЮЧ"
```

3. Запустити застосунок звичайною командою `dotnet run`.

Ключ зберігається у `%APPDATA%\Microsoft\UserSecrets\lr2-callories-ai\secrets.json`
(Linux/macOS: `~/.microsoft/usersecrets/lr2-callories-ai/secrets.json`) і **не потрапляє в git**.

Альтернатива — змінні середовища (зручно для CI):

```powershell
$env:Ai__ApiKey = "ВАШ_КЛЮЧ"
dotnet run --project LR2_Callories
```

## Налаштування

Усі параметри лежать у секції `Ai` файлу `appsettings.json` і можуть перекриватися
змінними середовища (`Ai__BaseUrl`, `Ai__Model`, `Ai__TimeoutSeconds`).

| Параметр | За замовчуванням | Опис |
|---|---|---|
| `BaseUrl` | `https://generativelanguage.googleapis.com/v1beta/openai/` | OpenAI-сумісний endpoint |
| `ApiKey` | `""` | порожній ключ → працює заглушка |
| `Model` | `gemini-3.5-flash` | назва моделі |
| `TimeoutSeconds` | `120` | безкоштовний тір Gemini відповідає повільно |

Підходить будь-який провайдер з OpenAI-сумісним API: достатньо змінити `BaseUrl` і `Model`.
Для швидших відповідей на безкоштовному тірі можна спробувати `gemini-3.5-flash-lite`.

## Безпека

- `ApiKey` у `appsettings.json` завжди порожній — секрети в репозиторій не комітяться.
- `.gitignore` додатково ігнорує `appsettings.*.local.json`, `secrets.json`, `.env`, `.env.*`.
- Якщо ключ випадково потрапив у коміт або публічний чат — його треба відкликати в AI Studio і згенерувати новий.

## Структура

- `Services/IFoodAnalysisService.cs` — інтерфейс аналізу фото.
- `Services/OpenAiFoodAnalysisService.cs` — клієнт до OpenAI-сумісного API (Gemini).
- `Services/MockFoodAnalysisService.cs` — заглушка для роботи без ключа.
- `Services/ImageValidator.cs` — перевірка типу та розміру файлу.
- `Controllers/HomeController.cs` — обробка форми `AnalyzeImage`.
