using System.Net;
using System.Text;
using LR2_Callories.Services;
using Microsoft.Extensions.Options;
using Xunit; 
namespace LR2_Callories.Tests; 
public class OpenAiFoodAnalysisServiceTests
{
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode; 
        public FakeHttpMessageHandler(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        } 
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var payload = $$"""
                {
                  "choices": [
                    {
                      "message": {
                        "content": {{System.Text.Json.JsonSerializer.Serialize(_responseContent)}}
                      }
                    }
                  ]
                }
                """; 
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            }; 
            return Task.FromResult(response);
        }
    } 
    private static OpenAiFoodAnalysisService CreateService(string modelContent)
    {
        var handler = new FakeHttpMessageHandler(modelContent);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/v1/")
        }; 
        var options = Options.Create(new AiOptions
        {
            Model = "test-model"
        }); 
        return new OpenAiFoodAnalysisService(httpClient, options);
    } 
    [Fact]
    public async Task AnalyzeAsync_JsonWrappedInMarkdown_ParsesDishAndMacros()
    { 
        var aiReply = """
            Ось результат аналізу:
            ```json
            {"dishName":"Омлет з овочами","calories":310,"proteins":18,"fats":22,"carbs":8}
            ```
            """;
        var service = CreateService(aiReply);
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 }); 
        var result = await service.AnalyzeAsync(stream, "image/jpeg"); 
        Assert.Equal("Омлет з овочами", result.DishName);
        Assert.Equal(310, result.Calories);
        Assert.Equal(18, result.Proteins);
        Assert.Equal(22, result.Fats);
        Assert.Equal(8, result.Carbs);
    } 
    [Fact]
    public async Task AnalyzeAsync_NumbersAsStringsWithComma_ParsesCorrectly()
    { 
        var aiReply = """{"dishName":"Каша","calories":"245,6","proteins":"7,5","fats":"4,2","carbs":"41,8"}""";
        var service = CreateService(aiReply);
        using var stream = new MemoryStream(new byte[] { 10, 20 }); 
        var result = await service.AnalyzeAsync(stream, "image/png"); 
        Assert.Equal("Каша", result.DishName);
        Assert.Equal(246, result.Calories);
        Assert.Equal(7.5, result.Proteins);
        Assert.Equal(4.2, result.Fats);
        Assert.Equal(41.8, result.Carbs);
    } 
    [Fact]
    public async Task AnalyzeAsync_MissingDishName_UsesFallbackName()
    { 
        var aiReply = """{"calories":150,"proteins":3,"fats":2,"carbs":30}""";
        var service = CreateService(aiReply);
        using var stream = new MemoryStream(new byte[] { 5 }); 
        var result = await service.AnalyzeAsync(stream, "image/jpeg"); 
        Assert.Equal("невідома страва", result.DishName);
        Assert.Equal(150, result.Calories);
    } 
    [Fact]
    public async Task AnalyzeAsync_HttpErrorStatus_ThrowsHttpRequestException()
    { 
        var handler = new FakeHttpMessageHandler("{}", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost/v1/")
        };
        var options = Options.Create(new AiOptions { Model = "test-model" });
        var service = new OpenAiFoodAnalysisService(httpClient, options);
        using var stream = new MemoryStream(new byte[] { 1, 2 }); 
        await Assert.ThrowsAsync<HttpRequestException>(() => service.AnalyzeAsync(stream, "image/jpeg"));
    }
}