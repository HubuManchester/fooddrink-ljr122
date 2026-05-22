using FoodLens.Models;

namespace FoodLens.Services;

/// <summary>
/// Service responsible for providing recipe data to the application.
/// Implements a repository pattern for data access.
/// </summary>
public class RecipeService
{
    private List<Recipe> _recipes = new();

    /// <summary>
    /// Retrieves all available recipes asynchronously.
    /// </summary>
    public async Task<List<Recipe>> GetRecipesAsync()
    {
        await Task.Delay(300);

        if (_recipes.Count > 0)
            return _recipes;

        _recipes = GenerateSampleRecipes();
        return _recipes;
    }

    /// <summary>
    /// Retrieves a single recipe by its unique ID.
    /// </summary>
    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        var recipes = await GetRecipesAsync();
        return recipes.FirstOrDefault(r => r.Id == id);
    }

    /// <summary>
    /// Returns a random recipe for the shake-to-discover feature.
    /// </summary>
    public async Task<Recipe> GetRandomRecipeAsync()
    {
        var recipes = await GetRecipesAsync();
        var random = new Random();
        return recipes[random.Next(recipes.Count)];
    }

    /// <summary>
    /// Filters recipes by category.
    /// </summary>
    public async Task<List<Recipe>> GetRecipesByCategoryAsync(string category)
    {
        var recipes = await GetRecipesAsync();

        if (string.IsNullOrWhiteSpace(category) || category == "All")
            return recipes;

        return recipes.Where(r =>
            r.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Searches recipes by name or description.
    /// </summary>
    public async Task<List<Recipe>> SearchRecipesAsync(string searchTerm)
    {
        var recipes = await GetRecipesAsync();

        if (string.IsNullOrWhiteSpace(searchTerm))
            return recipes;

        return recipes.Where(r =>
            r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Generates sample recipe data for the application.
    /// </summary>
    private static List<Recipe> GenerateSampleRecipes()
    {
        return new List<Recipe>
        {
            new Recipe
            {
                Id = 1,
                Name = "Classic Margherita Pizza",
                Description = "Traditional Italian pizza with fresh mozzarella, tomatoes, and basil.",
                Category = "Dinner",
                ImageUrl = "pizza.png",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 15,
                Servings = 4,
                Difficulty = "Medium",
                Ingredients = new List<string>
                {
                    "500g strong bread flour", "7g dried yeast", "1 tsp salt",
                    "325ml warm water", "2 tbsp olive oil", "200g fresh mozzarella",
                    "400g canned San Marzano tomatoes", "Fresh basil leaves", "2 cloves garlic"
                },
                Steps = new List<string>
                {
                    "Mix flour, yeast, and salt in a large bowl.",
                    "Add warm water and olive oil, knead for 10 minutes until smooth.",
                    "Cover and let dough rise for 1 hour until doubled in size.",
                    "Preheat oven to 250 degrees Celsius.",
                    "Blend tomatoes with garlic and salt for the sauce.",
                    "Divide dough into 4 portions and roll out on floured surface.",
                    "Spread tomato sauce evenly, leaving a 1cm border.",
                    "Tear mozzarella and distribute over the pizza.",
                    "Bake for 12 to 15 minutes until golden and bubbly.",
                    "Top with fresh basil leaves before serving."
                },
                Nutrition = new NutritionInfo { Calories = 285, ProteinGrams = 12.5, CarbsGrams = 38.0, FatGrams = 9.5, FiberGrams = 2.1 },
                Latitude = 40.8518, Longitude = 14.2681, Origin = "Naples, Italy"
            },
            new Recipe
            {
                Id = 2,
                Name = "Japanese Miso Ramen",
                Description = "Rich miso-based ramen with soft-boiled egg and chashu pork.",
                Category = "Dinner",
                ImageUrl = "ramen.png",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 45,
                Servings = 2,
                Difficulty = "Hard",
                Ingredients = new List<string>
                {
                    "200g fresh ramen noodles", "3 tbsp white miso paste", "1 litre chicken stock",
                    "2 soft-boiled eggs", "200g pork belly", "2 tbsp soy sauce",
                    "1 tbsp sesame oil", "Spring onions", "Nori sheets", "Sweetcorn"
                },
                Steps = new List<string>
                {
                    "Marinate pork belly in soy sauce and mirin for 30 minutes.",
                    "Sear pork then braise in oven at 160 degrees for 2 hours.",
                    "Boil eggs for 6.5 minutes for soft centre, then peel.",
                    "Heat chicken stock in a large pot.",
                    "Dissolve miso paste into the hot stock.",
                    "Cook ramen noodles for 2 to 3 minutes.",
                    "Slice braised pork into thin pieces.",
                    "Divide noodles between bowls, pour over broth.",
                    "Top with pork, halved egg, spring onions, nori, and sweetcorn.",
                    "Drizzle with sesame oil and serve immediately."
                },
                Nutrition = new NutritionInfo { Calories = 520, ProteinGrams = 32.0, CarbsGrams = 48.0, FatGrams = 22.0, FiberGrams = 3.5 },
                Latitude = 43.0618, Longitude = 141.3545, Origin = "Sapporo, Japan"
            },
            new Recipe
            {
                Id = 3,
                Name = "Avocado Breakfast Toast",
                Description = "Creamy smashed avocado with poached eggs on sourdough toast.",
                Category = "Breakfast",
                ImageUrl = "avocado_toast.png",
                PrepTimeMinutes = 5,
                CookTimeMinutes = 5,
                Servings = 2,
                Difficulty = "Easy",
                Ingredients = new List<string>
                {
                    "2 slices sourdough bread", "1 ripe avocado", "2 fresh eggs",
                    "1 tbsp white vinegar", "Salt and pepper", "Chilli flakes", "Lemon juice"
                },
                Steps = new List<string>
                {
                    "Toast sourdough bread until golden and crispy.",
                    "Halve avocado, remove stone, scoop into a bowl.",
                    "Mash avocado with lemon juice, salt, and pepper.",
                    "Bring water to a gentle simmer, add vinegar.",
                    "Create a whirlpool and carefully drop in each egg.",
                    "Poach for 3 minutes for a runny yolk.",
                    "Spread mashed avocado on each toast slice.",
                    "Place poached egg on top of each toast.",
                    "Season with salt, pepper, and chilli flakes.",
                    "Serve immediately while warm."
                },
                Nutrition = new NutritionInfo { Calories = 320, ProteinGrams = 14.0, CarbsGrams = 28.0, FatGrams = 18.0, FiberGrams = 7.0 },
                Latitude = -33.8688, Longitude = 151.2093, Origin = "Sydney, Australia"
            },
            new Recipe
            {
                Id = 4,
                Name = "Mango Lassi",
                Description = "Refreshing Indian yoghurt drink with sweet mango and cardamom.",
                Category = "Drinks",
                ImageUrl = "mango_lassi.png",
                PrepTimeMinutes = 5,
                CookTimeMinutes = 0,
                Servings = 2,
                Difficulty = "Easy",
                Ingredients = new List<string>
                {
                    "2 ripe mangoes", "200ml natural yoghurt", "100ml whole milk",
                    "2 tbsp honey", "Quarter tsp ground cardamom", "Ice cubes", "Pinch of saffron"
                },
                Steps = new List<string>
                {
                    "Peel and chop mangoes into chunks.",
                    "Add mango, yoghurt, and milk to a blender.",
                    "Add honey and ground cardamom.",
                    "Blend on high until completely smooth.",
                    "Taste and adjust sweetness if needed.",
                    "Add ice cubes and blend briefly.",
                    "Pour into tall glasses.",
                    "Garnish with saffron or cardamom on top.",
                    "Serve immediately while cold."
                },
                Nutrition = new NutritionInfo { Calories = 210, ProteinGrams = 6.0, CarbsGrams = 42.0, FatGrams = 3.5, FiberGrams = 2.5 },
                Latitude = 28.6139, Longitude = 77.2090, Origin = "Delhi, India"
            },
            new Recipe
            {
                Id = 5,
                Name = "Chocolate Lava Cake",
                Description = "Decadent dessert with a molten chocolate centre.",
                Category = "Dessert",
                ImageUrl = "lava_cake.png",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 12,
                Servings = 4,
                Difficulty = "Medium",
                Ingredients = new List<string>
                {
                    "200g dark chocolate 70% cocoa", "150g unsalted butter", "3 large eggs",
                    "3 egg yolks", "75g caster sugar", "50g plain flour",
                    "Butter and cocoa for greasing", "Vanilla ice cream to serve"
                },
                Steps = new List<string>
                {
                    "Preheat oven to 200 degrees Celsius.",
                    "Melt chocolate and butter over simmering water.",
                    "Whisk eggs, yolks, and sugar until thick and pale.",
                    "Fold melted chocolate into egg mixture.",
                    "Sift in flour and fold gently until just combined.",
                    "Grease 4 ramekins with butter and dust with cocoa.",
                    "Divide batter evenly between ramekins.",
                    "Bake for exactly 12 minutes until edges firm but centre soft.",
                    "Let stand 1 minute then invert onto plates.",
                    "Serve immediately with vanilla ice cream."
                },
                Nutrition = new NutritionInfo { Calories = 480, ProteinGrams = 8.0, CarbsGrams = 35.0, FatGrams = 34.0, FiberGrams = 3.0 },
                Latitude = 48.8566, Longitude = 2.3522, Origin = "Paris, France"
            },
            new Recipe
            {
                Id = 6,
                Name = "Thai Green Curry",
                Description = "Fragrant spicy Thai curry with chicken and coconut milk.",
                Category = "Dinner",
                ImageUrl = "thai_curry.png",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 25,
                Servings = 4,
                Difficulty = "Medium",
                Ingredients = new List<string>
                {
                    "400ml coconut milk", "3 tbsp green curry paste", "300g chicken breast",
                    "1 aubergine cubed", "100g green beans", "2 tbsp fish sauce",
                    "1 tbsp palm sugar", "Thai basil leaves", "2 kaffir lime leaves", "Jasmine rice"
                },
                Steps = new List<string>
                {
                    "Heat a wok over high heat.",
                    "Fry curry paste in coconut cream for 2 minutes.",
                    "Add sliced chicken and stir-fry until sealed.",
                    "Pour in remaining coconut milk and bring to simmer.",
                    "Add aubergine and kaffir lime leaves.",
                    "Cook for 10 minutes until aubergine is tender.",
                    "Add green beans and cook 5 more minutes.",
                    "Season with fish sauce and palm sugar.",
                    "Stir in Thai basil leaves.",
                    "Serve over steamed jasmine rice."
                },
                Nutrition = new NutritionInfo { Calories = 380, ProteinGrams = 28.0, CarbsGrams = 12.0, FatGrams = 25.0, FiberGrams = 4.0 },
                Latitude = 13.7563, Longitude = 100.5018, Origin = "Bangkok, Thailand"
            }
        };
    }
}