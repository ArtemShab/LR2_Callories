using LR2_Callories.Services;
using Xunit; 
namespace LR2_Callories.Tests; 
public class MockFoodAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_LengthZero_ReturnsFirstDishCarbonara()
    { 
        var service = new MockFoodAnalysisService();
        using var stream = new MemoryStream(new byte[0]); 
        var result = await service.AnalyzeAsync(stream, "image/jpeg"); 
        Assert.NotNull(result);
        Assert.Equal("Паста карбонара", result.DishName);
        Assert.Equal(620, result.Calories);
    }

    [Fact]
    public async Task AnalyzeAsync_LengthOne_ReturnsSecondDishGreekSalad()
    { 
        var service = new MockFoodAnalysisService();
        using var stream = new MemoryStream(new byte[1]); 
        var result = await service.AnalyzeAsync(stream, "image/png"); 
        Assert.Equal("Грецький салат", result.DishName);
        Assert.Equal(180, result.Calories);
        Assert.Equal(5, result.Proteins);
        Assert.Equal(14, result.Fats);
        Assert.Equal(9, result.Carbs);
    }

    [Fact]
    public async Task AnalyzeAsync_LengthSix_WrapsAroundToFirstDish()
    { 
        var service = new MockFoodAnalysisService();
        using var stream = new MemoryStream(new byte[6]); 
        var result = await service.AnalyzeAsync(stream, "image/webp"); 
        Assert.Equal("Паста карбонара", result.DishName);
        Assert.Equal(620, result.Calories);
    }

    [Fact]
    public async Task AnalyzeAsync_SameStreamLength_ReturnsConsistentResult()
    { 
        var service = new MockFoodAnalysisService();
        using var stream1 = new MemoryStream(new byte[2]);
        using var stream2 = new MemoryStream(new byte[2]); 
        var firstCall = await service.AnalyzeAsync(stream1, "image/jpeg");
        var secondCall = await service.AnalyzeAsync(stream2, "image/jpeg"); 
        Assert.Equal(firstCall.DishName, secondCall.DishName);
        Assert.Equal(firstCall.Calories, secondCall.Calories);
    }
}