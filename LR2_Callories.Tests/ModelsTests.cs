using LR2_Callories.Models;
using Xunit; 
namespace LR2_Callories.Tests; 
public class ModelsTests
{
    [Fact]
    public void ErrorViewModel_NullOrEmptyRequestId_ShowRequestIdIsFalse()
    {
        var nullModel = new ErrorViewModel { RequestId = null };
        var emptyModel = new ErrorViewModel { RequestId = string.Empty }; 
        Assert.False(nullModel.ShowRequestId);
        Assert.False(emptyModel.ShowRequestId);
    } 
    [Fact]
    public void ErrorViewModel_NonEmptyRequestId_ShowRequestIdIsTrue()
    {
        var model = new ErrorViewModel { RequestId = "req-123" }; 
        Assert.True(model.ShowRequestId);
    } 
    [Fact]
    public void MealAnalysisResult_DefaultAndAssignedValues_WorkAsExpected()
    {
        var model = new MealAnalysisResult();
        Assert.Equal(string.Empty, model.DishName); 
        model.DishName = "Суп";
        model.Calories = 210;
        model.Proteins = 10.5;
        model.Fats = 7.0;
        model.Carbs = 25.0; 
        Assert.Equal("Суп", model.DishName);
        Assert.Equal(210, model.Calories);
        Assert.Equal(10.5, model.Proteins);
        Assert.Equal(7.0, model.Fats);
        Assert.Equal(25.0, model.Carbs);
    }
}