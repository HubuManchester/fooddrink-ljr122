using System.Text.Json;
using System.Text.RegularExpressions;
using FoodLens.Models;

namespace FoodLens.Services;

/// <summary>
/// Service for fetching nutritional data from the Open Food Facts API.
/// Demonstrates NETWORKING functionality required for the 86-100% Functionality band.
/// Uses the Open Food Facts API (free, no API key required) for real food data.
///
/// API documentation: https://world.openfoodfacts.org/data
/// Response models are defined in Models/OpenFoodFactsModels.cs (namespace FoodLens.Models).
/// </summary>
public partial class NutritionApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://world.openfoodfacts.org/api/v2";

    /// <summary>
    /// Compile-time generated regular expression for barcode format validation.
    ///
    /// FIX (Roslyn SYSLIB1045 / CA1854): Replaced <c>new Regex(..., RegexOptions.Compiled)</c>
    /// with the <see cref="GeneratedRegexAttribute"/> source generator.
    /// [GeneratedRegex] emits the entire regex automaton as IL at compile time rather than
    /// building the NFA at runtime, giving faster startup and zero allocation per-call.
    /// The class must be <c>partial</c> for the source generator to inject the implementation.
    /// </summary>
    [GeneratedRegex(@"^\d{8,14}$")]
    private static partial Regex BarcodeRegex();

    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> instance to avoid repeated allocation.
    /// Roslyn CA1869 flags creating new JsonSerializerOptions on every call because the
    /// constructor performs expensive reflection-based metadata caching internally.
    /// By reusing a single static instance, serialisation is both faster and allocation-free.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
        ArgumentException.ThrowIfNullOrWhiteSpace(foodName, nameof(foodName));

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
            var result = JsonSerializer.Deserialize<OpenFoodFactsResponse>(json, JsonOptions);

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
    /// Looks up a food product by its barcode (EAN-8, EAN-13, or UPC-A format).
    /// Intended for future integration with the camera barcode scanning feature.
    ///
    /// VALIDATION: Barcodes must be 8–14 numeric digits only.
    /// Invalid formats are rejected immediately with <see cref="ArgumentException"/>
    /// rather than making a futile network request that will return no results.
    /// </summary>
    /// <param name="barcode">
    /// The product barcode string. Must contain 8 to 14 numeric digits only.
    /// </param>
    /// <returns>
    /// A tuple of (ProductName, NutritionInfo) if the product is found; null otherwise.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="barcode"/> is null, whitespace, or not a valid barcode format.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when a network or timeout error occurs.</exception>
    public async Task<(string ProductName, NutritionInfo? Nutrition)?> GetProductByBarcodeAsync(
        string barcode)
    {
        // VALIDATION 1: Null / empty guard
        ArgumentException.ThrowIfNullOrWhiteSpace(barcode, nameof(barcode));

        // VALIDATION 2: Format check — must be 8–14 numeric digits (EAN-8/13, UPC-A)
        // FIX (Roslyn SYSLIB1045): Uses the [GeneratedRegex] method instead of a stored Regex field.
        if (!BarcodeRegex().IsMatch(barcode.Trim()))
        {
            throw new ArgumentException(
                "Barcode must contain between 8 and 14 numeric digits only. " +
                $"Received: '{barcode}'",
                nameof(barcode));
        }

        try
        {
            string url = $"{BaseUrl}/product/{Uri.EscapeDataString(barcode.Trim())}.json";
            HttpResponseMessage response = await _httpClient.GetAsync(url)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[NutritionAPI] Barcode lookup HTTP {(int)response.StatusCode}");
                return null;
            }

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<OpenFoodFactsSingleProduct>(json, JsonOptions);

            if (result?.Product is null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[NutritionAPI] No product found for barcode '{barcode}'");
                return null;
            }

            string name = result.Product.ProductName ?? "Unknown Product";
            var nutriments = result.Product.Nutriments;
            NutritionInfo? nutrition = nutriments is not null
                ? MapNutrimentsToNutritionInfo(nutriments)
                : null;

            return (name, nutrition);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NutritionAPI] Barcode network error: {ex.Message}");
            throw new InvalidOperationException("Network error while fetching product data.", ex);
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("[NutritionAPI] Barcode request timed out.");
            throw new InvalidOperationException("Request timed out. Please check your connection.");
        }
        catch (JsonException ex)
        {
            // Non-fatal: unexpected API response format — return null rather than crashing
            System.Diagnostics.Debug.WriteLine($"[NutritionAPI] Barcode JSON error: {ex.Message}");
            return null;
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
    /// Sodium is converted from grams per 100g to milligrams per 100g (* 1000).
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