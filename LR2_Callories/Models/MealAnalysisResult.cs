namespace LR2_Callories.Models
{
    // результат одного аналізу фото страви
    public class MealAnalysisResult
    {
        public string DishName { get; set; } = string.Empty;

        public int Calories { get; set; }

        public double Proteins { get; set; }

        public double Fats { get; set; }

        public double Carbs { get; set; }
    }
}
