using LR2_Callories.Models;
using LR2_Callories.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LR2_Callories.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFoodAnalysisService _foodAnalysisService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFoodAnalysisService foodAnalysisService, ILogger<HomeController> logger)
        {
            _foodAnalysisService = foodAnalysisService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> AnalyzeImage(IFormFile? imageFile, CancellationToken cancellationToken)
        {
            // спочатку перевіряємо файл, а вже потім відправляємо його на аналіз
            var error = ImageValidator.Validate(imageFile);
            if (error != null)
            {
                ViewBag.ErrorMessage = error;
                return View("Index");
            }

            try
            {
                using var stream = imageFile!.OpenReadStream();
                var result = await _foodAnalysisService.AnalyzeAsync(stream, imageFile.ContentType, cancellationToken);
                return View("Index", result);
            }
            catch (Exception ex)
            {
                // деталі пишемо в лог, а користувачу показуємо просте повідомлення без технічних деталей
                _logger.LogError(ex, "Не вдалося проаналізувати фото страви");
                ViewBag.ErrorMessage = "не вдалося проаналізувати фото, спробуйте ще раз";
                return View("Index");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
