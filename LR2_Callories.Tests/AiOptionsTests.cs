using LR2_Callories.Services;
using Xunit; 
namespace LR2_Callories.Tests; 
public class AiOptionsTests
{
    [Fact]
    public void AiOptions_DefaultValues_AreSetAsExpected()
    { 
        var options = new AiOptions(); 
        Assert.Equal("https://generativelanguage.googleapis.com/v1beta/openai/", options.BaseUrl);
        Assert.Equal(string.Empty, options.ApiKey);
        Assert.False(string.IsNullOrWhiteSpace(options.Model));
        Assert.Equal(120, options.TimeoutSeconds);
    } 
    [Fact]
    public void AiOptions_CustomValues_CanBeAssigned()
    { 
        var options = new AiOptions(); 
        options.BaseUrl = "https://localhost:5000/v1/";
        options.ApiKey = "test-key";
        options.Model = "custom-model";
        options.TimeoutSeconds = 60; 
        Assert.Equal("https://localhost:5000/v1/", options.BaseUrl);
        Assert.Equal("test-key", options.ApiKey);
        Assert.Equal("custom-model", options.Model);
        Assert.Equal(60, options.TimeoutSeconds);
    }
}