using System.Text.Json.Serialization;

// FIX (Roslyn): Namespace corrected from "FoodLens.Services" to "FoodLens.Models"
// to match the file's folder location (Models/OpenFoodFactsModels.cs).
// Mismatched namespaces are flagged by the IDE0130 analyser and make the type
// harder to discover — types should always live in the namespace that mirrors
// their folder path (Microsoft .NET naming conventions).
namespace FoodLens.Models;

/// <summary>
/// Response model for the Open Food Facts search API endpoint.
/// Maps the JSON structure returned by /api/v2/search.
/// Documentation: https://world.openfoodfacts.org/data
/// </summary>
public class OpenFoodFactsResponse
{
    /// <summary>Total number of products matching the search query.</summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>List of product results returned by the search.</summary>
    [JsonPropertyName("products")]
    // FIX (Roslyn IDE0028): Use collection expression [] instead of new List<T>() / new().
    // The collection expression is more concise and is the preferred idiom in modern C#.
    public List<OpenFoodFactsProduct> Products { get; init; } = [];
}

/// <summary>
/// Response model for a single product lookup by barcode.
/// Maps the JSON structure returned by /api/v2/product/{barcode}.json.
/// </summary>
public class OpenFoodFactsSingleProduct
{
    /// <summary>Status code indicating whether the product was found (1 = found).</summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>The product data, or null if not found.</summary>
    [JsonPropertyName("product")]
    public OpenFoodFactsProduct? Product { get; set; }
}

/// <summary>
/// Represents a single product from the Open Food Facts database.
/// Contains the product name and nutritional data (nutriments).
/// </summary>
public class OpenFoodFactsProduct
{
    /// <summary>The display name of the product.</summary>
    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    /// <summary>Nutritional information per 100g of the product.</summary>
    [JsonPropertyName("nutriments")]
    public OpenFoodFactsNutriments? Nutriments { get; set; }
}

/// <summary>
/// Nutritional data per 100g as returned by the Open Food Facts API.
/// All values are in grams unless otherwise specified.
/// Property names use JsonPropertyName to map from the API's snake_case format.
/// </summary>
public class OpenFoodFactsNutriments
{
    /// <summary>Energy in kilocalories per 100g.</summary>
    [JsonPropertyName("energy-kcal_100g")]
    public double EnergyKcal100g { get; set; }

    /// <summary>Protein in grams per 100g.</summary>
    [JsonPropertyName("proteins_100g")]
    public double Proteins100g { get; set; }

    /// <summary>Carbohydrates in grams per 100g.</summary>
    [JsonPropertyName("carbohydrates_100g")]
    public double Carbohydrates100g { get; set; }

    /// <summary>Fat in grams per 100g.</summary>
    [JsonPropertyName("fat_100g")]
    public double Fat100g { get; set; }

    /// <summary>Dietary fiber in grams per 100g.</summary>
    [JsonPropertyName("fiber_100g")]
    public double Fiber100g { get; set; }

    /// <summary>Sugars in grams per 100g.</summary>
    [JsonPropertyName("sugars_100g")]
    public double Sugars100g { get; set; }

    /// <summary>Sodium in grams per 100g (converted to mg in the mapping layer).</summary>
    [JsonPropertyName("sodium_100g")]
    public double Sodium100g { get; set; }
}