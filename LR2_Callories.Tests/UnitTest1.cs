using Microsoft.AspNetCore.Http;
using LR2_Callories.Services;
using Xunit; 
namespace LR2_Callories.Tests; 
public class ImageValidatorTests
{
    private class FakeFormFile : IFormFile
    {
        public FakeFormFile(string fileName, long length)
        {
            FileName = fileName;
            Length = length;
        }
        public string FileName { get; }
        public long Length { get; }
        public string ContentType => "image/jpeg";
        public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{FileName}\"";
        public IHeaderDictionary Headers => new HeaderDictionary();
        public string Name => "file"; 
        public void CopyTo(Stream target) { }
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Stream OpenReadStream() => new MemoryStream(new byte[Length > 0 ? (int)Math.Min(Length, 1024) : 0]);
    }

    [Fact]
    public void Validate_NullFile_ReturnsSelectPhotoError()
    { 
        var result = ImageValidator.Validate(null); 
        Assert.Equal("спочатку виберіть фото страви", result);
    }

    [Fact]
    public void Validate_EmptyFile_ReturnsSelectPhotoError()
    { 
        var file = new FakeFormFile("meal.jpg", 0); 
        var result = ImageValidator.Validate(file); 
        Assert.Equal("спочатку виберіть фото страви", result);
    }

    [Fact]
    public void Validate_FileLargerThan10Mb_ReturnsTooLargeError()
    { 
        long overLimitBytes = 10L * 1024 * 1024 + 1;
        var file = new FakeFormFile("meal.png", overLimitBytes); 
        var result = ImageValidator.Validate(file); 
        Assert.Equal("файл завеликий, максимум 10 мб", result);
    }

    [Fact]
    public void Validate_UnsupportedExtension_ReturnsFormatError()
    { 
        var file = new FakeFormFile("document.pdf", 500); 
        var result = ImageValidator.Validate(file); 
        Assert.Equal("підтримуються тільки jpg, png і webp", result);
    }

    [Fact]
    public void Validate_ValidJpgFile_ReturnsNull()
    { 
        var file = new FakeFormFile("dinner.JPG", 2048); 
        var result = ImageValidator.Validate(file); 
        Assert.Null(result);
    }
}