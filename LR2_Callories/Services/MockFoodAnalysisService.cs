using LR2_Callories.Models;

namespace LR2_Callories.Services
{
    // заглушка на випадок коли немає ключа до нейромережі
    // потрібна щоб можна було спокійно тестувати сторінку без витрат на api
    public class MockFoodAnalysisService : IFoodAnalysisService
    {
        private static readonly (string Name, int Calories, double Proteins, double Fats, double Carbs)[] Dishes =
        {
            ("Паста карбонара", 620, 24, 32, 58),
            ("Грецький салат", 180, 5, 14, 9),
            ("Куряча грудка з рисом", 450, 38, 9, 52),
            ("Піца Маргарита", 780, 29, 28, 96),
            ("Вівсянка з бананом", 320, 9, 6, 58),
            ("Борщ зі сметаною", 260, 7, 12, 30)
        };

        public Task<MealAnalysisResult> AnalyzeAsync(Stream image, string contentType, CancellationToken cancellationToken = default)
        {
            // беремо страву по розміру файлу, щоб для одної картинки результат не мінявся
            var index = (int)(image.Length % Dishes.Length);
            var dish = Dishes[index];

            var result = new MealAnalysisResult
            {
                DishName = dish.Name,
                Calories = dish.Calories,
                Proteins = dish.Proteins,
                Fats = dish.Fats,
                Carbs = dish.Carbs
            };

            return Task.FromResult(result);
        }
    }
}
