namespace LR2_Callories.Services
{
    // перевіряємо фото ще до відправки в нейромережу
    // так користувач швидше побачить помилку і ми не платимо за пустий запрос
    public static class ImageValidator
    {
        private const long MaxSizeBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return "спочатку виберіть фото страви";
            }

            if (file.Length > MaxSizeBytes)
            {
                return "файл завеликий, максимум 10 мб";
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return "підтримуються тільки jpg, png і webp";
            }

            return null;
        }
    }
}
