namespace LR2_Callories.Services
{
    // налаштування нейромережі, читаються з appsettings
    public class AiOptions
    {
        public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-4o-mini";
    }
}
