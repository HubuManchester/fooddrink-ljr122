namespace FoodLens.Models;

/// <summary>
/// Nutritional information for a recipe per serving.
/// </summary>
public class NutritionInfo
{
    /// <summary>Calories per serving.</summary>
    public int Calories { get; set; }

    /// <summary>Protein in grams.</summary>
    public double ProteinGrams { get; set; }

    /// <summary>Carbohydrates in grams.</summary>
    public double CarbsGrams { get; set; }

    /// <summary>Fat in grams.</summary>
    public double FatGrams { get; set; }

    /// <summary>Fiber in grams.</summary>
    public double FiberGrams { get; set; }
}