using LR2_Callories.Models;
using Microsoft.Extensions.Options;
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
            // модель іноді обгортає json в ``` тому прибираємо зайві символи
            var cleaned = text.Replace("```json", string.Empty).Replace("```", string.Empty).Trim();

            using var document = JsonDocument.Parse(cleaned);
            var root = document.RootElement;

            return new MealAnalysisResult
            {
                DishName = root.GetProperty("dishName").GetString() ?? "невідома страва",
                Calories = root.GetProperty("calories").GetInt32(),
                Proteins = root.GetProperty("proteins").GetDouble(),
                Fats = root.GetProperty("fats").GetDouble(),
                Carbs = root.GetProperty("carbs").GetDouble()
            };
        }
    }
}
