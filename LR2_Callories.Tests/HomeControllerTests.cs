using System.Text;
using LR2_Callories.Controllers;
using LR2_Callories.Models;
using LR2_Callories.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit; 
namespace LR2_Callories.Tests; 
public class HomeControllerTests
{
    private class StubFoodAnalysisService : IFoodAnalysisService
    {
        private readonly MealAnalysisResult? _resultToReturn;
        private readonly Exception? _exceptionToThrow; 
        public StubFoodAnalysisService(MealAnalysisResult? resultToReturn = null, Exception? exceptionToThrow = null)
        {
            _resultToReturn = resultToReturn;
            _exceptionToThrow = exceptionToThrow;
        } 
        public Task<MealAnalysisResult> AnalyzeAsync(Stream image, string contentType, CancellationToken cancellationToken = default)
        {
            if (_exceptionToThrow != null)
            {
                throw _exceptionToThrow;
            } 
            return Task.FromResult(_resultToReturn ?? new MealAnalysisResult());
        }
    } 
    private class TestFormFile : IFormFile
    {
        private readonly byte[] _content; 
        public TestFormFile(string fileName, string contentType, byte[] content)
        {
            FileName = fileName;
            ContentType = contentType;
            _content = content;
        } 
        public string ContentType { get; }
        public string ContentDisposition => $"form-data; name=\"imageFile\"; filename=\"{FileName}\"";
        public IHeaderDictionary Headers => new HeaderDictionary();
        public long Length => _content.Length;
        public string Name => "imageFile";
        public string FileName { get; } 
        public Stream OpenReadStream() => new MemoryStream(_content);
        public void CopyTo(Stream target) => target.Write(_content, 0, _content.Length);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) =>
            target.WriteAsync(_content, 0, _content.Length, cancellationToken);
    } 
    [Fact]
    public void Index_ReturnsViewResult()
    {
        var controller = new HomeController(new StubFoodAnalysisService(), NullLogger<HomeController>.Instance); 
        var result = controller.Index(); 
        Assert.IsType<ViewResult>(result);
    } 
    [Fact]
    public async Task AnalyzeImage_NullFile_SetsErrorMessageAndReturnsIndexView()
    {
        var controller = new HomeController(new StubFoodAnalysisService(), NullLogger<HomeController>.Instance); 
        var result = await controller.AnalyzeImage(null, CancellationToken.None); 
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal("спочатку виберіть фото страви", controller.ViewBag.ErrorMessage);
    } 
    [Fact]
    public async Task AnalyzeImage_ValidFile_ReturnsIndexViewWithAnalysisModel()
    {
        var expected = new MealAnalysisResult { DishName = "Борщ", Calories = 260 };
        var controller = new HomeController(new StubFoodAnalysisService(expected), NullLogger<HomeController>.Instance);
        var file = new TestFormFile("borsch.jpg", "image/jpeg", Encoding.UTF8.GetBytes("fake-image-bytes")); 
        var result = await controller.AnalyzeImage(file, CancellationToken.None); 
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        var model = Assert.IsType<MealAnalysisResult>(viewResult.Model);
        Assert.Equal("Борщ", model.DishName);
        Assert.Equal(260, model.Calories);
    } 
    [Fact]
    public async Task AnalyzeImage_ServiceThrowsException_SetsFallbackErrorMessage()
    {
        var controller = new HomeController(
            new StubFoodAnalysisService(exceptionToThrow: new InvalidOperationException("API error")),
            NullLogger<HomeController>.Instance);
        var file = new TestFormFile("meal.png", "image/png", Encoding.UTF8.GetBytes("fake-image-bytes")); 
        var result = await controller.AnalyzeImage(file, CancellationToken.None); 
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        Assert.Equal("не вдалося проаналізувати фото, спробуйте ще раз", controller.ViewBag.ErrorMessage);
    }
}