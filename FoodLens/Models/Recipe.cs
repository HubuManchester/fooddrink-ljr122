namespace FoodLens.Models;

/// <summary>
/// Represents a food or drink recipe with full details including
/// ingredients, steps, nutrition, and geographic origin.
/// </summary>
public class Recipe
{
    /// <summary>
    /// Known valid image resources bundled with the application.
    /// Used to detect missing images and show user-friendly error messages.
    /// </summary>
    private static readonly HashSet<string> KnownValidImages = new(StringComparer.OrdinalIgnoreCase)
    {
        "pizza.png",
        "ramen.png",
        "avocado_toast.png",
        "mango_lassi.png",
        "lava_cake.png",
        "thai_curry.png"
    };

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

    /// <summary>Whether this recipe is marked as a favourite by the user.</summary>
    public bool IsFavourite { get; set; }

    /// <summary>Total time combining prep and cook time.</summary>
    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;

    /// <summary>
    /// Indicates whether the recipe image resource is available in the app bundle.
    /// Returns false for images that do not exist, allowing the UI to show
    /// a user-friendly placeholder message instead of a broken image.
    /// </summary>
    public bool IsImageAvailable =>
        !string.IsNullOrWhiteSpace(ImageUrl) && KnownValidImages.Contains(ImageUrl);

    /// <summary>
    /// User-friendly message displayed when the image cannot be loaded.
    /// Demonstrates validation and error handling for missing resources.
    /// </summary>
    public string ImageErrorMessage => IsImageAvailable
        ? string.Empty
        : "⚠️ Image unavailable — the photo for this recipe could not be loaded. " +
          "This may be due to a missing resource or network issue.";
}