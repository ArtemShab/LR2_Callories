using LR2_Callories.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace LR2_Callories.Services
{
    // відправляє фото в нейромережу і розбирає json з відповіддю
    // підходить будь-який провайдер який працює як openai chat completions
    public class OpenAiFoodAnalysisService : IFoodAnalysisService
    {
        private const string SystemPrompt =
            "ти кулінарний експерт. подивись на фото страви і оціни її калорійність. " +
            "відповідай тільки json без пояснень у форматі " +
            "{\"dishName\":\"назва\",\"calories\":0,\"proteins\":0,\"fats\":0,\"carbs\":0}";

        private readonly HttpClient _httpClient;
        private readonly AiOptions _options;

        public OpenAiFoodAnalysisService(HttpClient httpClient, IOptions<AiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<MealAnalysisResult> AnalyzeAsync(Stream image, string contentType, CancellationToken cancellationToken = default)
        {
            // api приймає картинку в base64, тому спочатку читаємо стрім в память
            using var memory = new MemoryStream();
            await image.CopyToAsync(memory, cancellationToken);
            var base64 = Convert.ToBase64String(memory.ToArray());

            var request = new
            {
                model = _options.Model,
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "що це за страва і скільки в ній калорій та бжу?" },
                            new { type = "image_url", image_url = new { url = $"data:{contentType};base64,{base64}" } }
                        }
                    }
                }
            };

            using var response = await _httpClient.PostAsJsonAsync("chat/completions", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var text = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return ParseResult(text ?? string.Empty);
        }

        private static MealAnalysisResult ParseResult(string text)
        {
            // модель може обгорнути json у ``` або додати текст навколо, тому вирізаємо перший об'єкт
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                text = text[start..(end + 1)];
            }

            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;

            return new MealAnalysisResult
            {
                DishName = GetString(root, "dishName", "невідома страва"),
                Calories = (int)Math.Round(GetNumber(root, "calories")),
                Proteins = GetNumber(root, "proteins"),
                Fats = GetNumber(root, "fats"),
                Carbs = GetNumber(root, "carbs")
            };
        }

        private static string GetString(JsonElement root, string property, string fallback) =>
            root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? fallback
                : fallback;

        private static double GetNumber(JsonElement root, string property)
        {
            if (!root.TryGetProperty(property, out var value))
            {
                return 0;
            }

            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetDouble(),
                // модель іноді повертає число рядком або з десятковою комою
                JsonValueKind.String => double.TryParse(
                    (value.GetString() ?? string.Empty).Replace(',', '.'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsed) ? parsed : 0,
                _ => 0
            };
        }
    }
}
