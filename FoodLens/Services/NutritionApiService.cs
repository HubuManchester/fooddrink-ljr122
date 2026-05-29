using System.Text.Json;
using FoodLens.Models;

namespace FoodLens.Services;

/// <summary>
/// Service for fetching nutritional data from the Open Food Facts API.
/// Demonstrates NETWORKING functionality required for the 86-100% Functionality band.
/// Uses the Open Food Facts API (free, no API key required) for real food data.
///
/// API documentation: https://world.openfoodfacts.org/data
/// Response models are defined separately in Models/OpenFoodFactsModels.cs.
/// </summary>
public class NutritionApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://world.openfoodfacts.org/api/v2";

    /// <summary>
    /// Initialises the NutritionApiService with a configured HttpClient.
    /// The HttpClient is injected via AddHttpClient&lt;T&gt;() in MauiProgram.cs,
    /// which uses IHttpClientFactory to manage connection lifetimes correctly.
    /// </summary>
    /// <param name="httpClient">The HTTP client provided by the DI container.</param>
    public NutritionApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _httpClient.DefaultRequestHeaders.Add(
            "User-Agent", "FoodLens/1.0 (.NET MAUI Student Project)");
    }

    /// <summary>
    /// Searches for a food product by name and returns nutritional information.
    /// Uses the Open Food Facts search endpoint for real-world data.
    ///
    /// Demonstrates: HTTP GET request, JSON deserialisation, comprehensive error handling.
    /// </summary>
    /// <param name="foodName">Name of the food to search for.</param>
    /// <returns>
    /// A <see cref="NutritionInfo"/> object if a matching product is found; null otherwise.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="foodName"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a network or timeout error occurs.</exception>
    public async Task<NutritionInfo?> GetNutritionByNameAsync(string foodName)
    {
        // Input validation — guard against empty search terms
        if (string.IsNullOrWhiteSpace(foodName))
        {
            throw new ArgumentException("Food name cannot be empty.", nameof(foodName));
        }

        try
        {
            // URL-encode the search term to handle special characters safely
            string encodedName = Uri.EscapeDataString(foodName.Trim());
            string url = $"{BaseUrl}/search?categories_tags_en={encodedName}" +
                         $"&fields=product_name,nutriments&page_size=1";

            // Make HTTP GET request — NETWORKING FEATURE
            HttpResponseMessage response = await _httpClient.GetAsync(url)
                .ConfigureAwait(false);

            // Validate HTTP response status before attempting to read the body
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[NutritionAPI] HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                return null;
            }

            // Read and deserialise the JSON response body
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<OpenFoodFactsResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Validate that at least one product was returned
            if (result?.Products is null || result.Products.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[NutritionAPI] No results found for '{foodName}'");
                return null;
            }

            // Map the first API result to our internal NutritionInfo model
            var nutriments = result.Products[0].Nutriments;
            if (nutriments is null) return null;

            return MapNutrimentsToNutritionInfo(nutriments);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionAPI] Network error: {ex.Message}");
            throw new InvalidOperationException(
                "Unable to connect to the nutrition database. " +
                "Please check your internet connection.", ex);
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("[NutritionAPI] Request timed out.");
            throw new InvalidOperationException(
                "The request timed out. Please check your internet connection and try again.");
        }
        catch (JsonException ex)
        {
            // Non-fatal: API response format may have changed — return null gracefully
            System.Diagnostics.Debug.WriteLine($"[NutritionAPI] JSON parse error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionAPI] Unexpected error: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Looks up a food product by its barcode (EAN/UPC format).
    /// Intended for future integration with the camera barcode scanning feature.
    /// </summary>
    /// <param name="barcode">The product barcode string (numeric digits only).</param>
    /// <returns>
    /// A tuple of (ProductName, NutritionInfo) if the product is found; null otherwise.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="barcode"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a network or timeout error occurs.</exception>
    public async Task<(string ProductName, NutritionInfo? Nutrition)?> GetProductByBarcodeAsync(
        string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new ArgumentException("Barcode cannot be empty.", nameof(barcode));

        try
        {
            string url = $"{BaseUrl}/product/{Uri.EscapeDataString(barcode)}.json";
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return null;

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<OpenFoodFactsSingleProduct>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Product is null) return null;

            string name = result.Product.ProductName ?? "Unknown Product";
            var nutriments = result.Product.Nutriments;
            NutritionInfo? nutrition = nutriments is not null
                ? MapNutrimentsToNutritionInfo(nutriments)
                : null;

            return (name, nutrition);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Network error while fetching product data.", ex);
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("Request timed out.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionAPI] Barcode lookup error: {ex}");
            return null;
        }
    }

    /// <summary>
    /// Maps an <see cref="OpenFoodFactsNutriments"/> API object to the internal
    /// <see cref="NutritionInfo"/> model used throughout the application.
    /// Extracted to follow the DRY principle — both search and barcode lookup use
    /// identical mapping logic, so it lives in one place.
    /// </summary>
    /// <param name="nutriments">The API nutriment data to convert.</param>
    /// <returns>A populated <see cref="NutritionInfo"/> instance.</returns>
    private static NutritionInfo MapNutrimentsToNutritionInfo(OpenFoodFactsNutriments nutriments)
    {
        return new NutritionInfo
        {
            Calories = (int)nutriments.EnergyKcal100g,
            ProteinGrams = Math.Round(nutriments.Proteins100g, 1),
            CarbsGrams = Math.Round(nutriments.Carbohydrates100g, 1),
            FatGrams = Math.Round(nutriments.Fat100g, 1),
            FiberGrams = Math.Round(nutriments.Fiber100g, 1),
            SugarGrams = Math.Round(nutriments.Sugars100g, 1),
            SodiumMg = Math.Round(nutriments.Sodium100g * 1000, 0),
            IsFromApi = true
        };
    }
}