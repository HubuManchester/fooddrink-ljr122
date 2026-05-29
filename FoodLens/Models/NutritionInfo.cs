namespace FoodLens.Models;

/// <summary>
/// Nutritional information for a recipe per serving.
/// Provides comprehensive macronutrient and micronutrient data.
/// </summary>
public class NutritionInfo
{
    /// <summary>Calories per serving in kilocalories.</summary>
    public int Calories { get; set; }

    /// <summary>Protein in grams per serving.</summary>
    public double ProteinGrams { get; set; }

    /// <summary>Carbohydrates in grams per serving.</summary>
    public double CarbsGrams { get; set; }

    /// <summary>Fat in grams per serving.</summary>
    public double FatGrams { get; set; }

    /// <summary>Dietary fiber in grams per serving.</summary>
    public double FiberGrams { get; set; }

    /// <summary>Sugar in grams per serving.</summary>
    public double SugarGrams { get; set; }

    /// <summary>Sodium in milligrams per serving.</summary>
    public double SodiumMg { get; set; }

    /// <summary>
    /// Indicates whether this nutrition data was retrieved from an external API.
    /// When false, the data is from the local sample database.
    /// Used to display a data source indicator in the UI.
    /// </summary>
    public bool IsFromApi { get; set; }
}