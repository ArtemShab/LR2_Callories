namespace LR2_Callories.Services
{
    // налаштування нейромережі, читаються з appsettings
    public class AiOptions
    {
        // за замовчуванням gemini через openai-сумісний endpoint
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai/";

        // ключ не зберігається в репозиторії, задається через user-secrets або змінні середовища
        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gemini-3.5-flash";

        // безкоштовний тір може відповідати повільно, тому таймаут із запасом
        public int TimeoutSeconds { get; set; } = 120;
    }
}
