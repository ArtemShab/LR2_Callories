namespace LR2_Callories.Models
{
    public class MealAnalysisResult
    {
        public string DishName { get; set; } = string.Empty;
        public int Calories { get; set; }
        public float Proteins { get; set; }
        public float Fats { get; set; }
        public float Carbs { get; set; }
    }
}