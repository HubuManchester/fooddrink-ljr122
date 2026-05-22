namespace FoodLens.Models;

/// <summary>
/// Represents a food or drink recipe with full details including
/// ingredients, steps, nutrition, and geographic origin.
/// </summary>
public class Recipe
{
    /// <summary>Unique identifier for the recipe.</summary>
    public int Id { get; set; }

    /// <summary>Name of the recipe.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description of the recipe.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Category (Breakfast, Lunch, Dinner, Dessert, Drinks).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Image filename for the recipe.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Preparation time in minutes.</summary>
    public int PrepTimeMinutes { get; set; }

    /// <summary>Cooking time in minutes.</summary>
    public int CookTimeMinutes { get; set; }

    /// <summary>Number of servings.</summary>
    public int Servings { get; set; }

    /// <summary>Difficulty level (Easy, Medium, Hard).</summary>
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>List of ingredients required.</summary>
    public List<string> Ingredients { get; set; } = new();

    /// <summary>Step-by-step cooking instructions.</summary>
    public List<string> Steps { get; set; } = new();

    /// <summary>Nutritional information per serving.</summary>
    public NutritionInfo Nutrition { get; set; } = new();

    /// <summary>Latitude of the recipe's origin location.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude of the recipe's origin location.</summary>
    public double Longitude { get; set; }

    /// <summary>Country or region of origin.</summary>
    public string Origin { get; set; } = string.Empty;

    /// <summary>Total time combining prep and cook time.</summary>
    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
}