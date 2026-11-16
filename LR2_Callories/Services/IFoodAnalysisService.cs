using LR2_Callories.Models;

namespace LR2_Callories.Services
{
    // спільний інтерфейс для будь-якого розпізнавача їжі
    public interface IFoodAnalysisService
    {
        Task<MealAnalysisResult> AnalyzeAsync(Stream image, string contentType, CancellationToken cancellationToken = default);
    }
}
