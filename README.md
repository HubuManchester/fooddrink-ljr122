# FoodLens

A cross-platform mobile application built with .NET MAUI for discovering, exploring, and interacting with food recipes from around the world. Developed as part of the 6G6Z0014 Mobile Computing module.

**Author:** [Jiarui Li]  
**Student ID:** [21906377]  
**Module:** 6G6Z0014 Mobile Computing  
**Framework:** .NET MAUI (.NET Multi-platform App UI)  
**Theme:** Food and Drink  

---

## Application Overview

FoodLens is a recipe discovery application that allows users to browse international recipes, scan food items using the device camera with on-device computer vision, fetch real nutritional data from the Open Food Facts REST API, navigate recipe origins on a map, and customise the app's appearance for accessibility. The app demonstrates extensive use of mobile hardware sensors, networking, MVVM architecture, and WCAG 2.1 accessibility compliance.

---

## Features

### Recipe Browsing
- Browse recipes by category (All, Breakfast, Dinner, Dessert, Drinks)
- Search recipes by name or description with debounced input (350ms delay to optimise performance)
- View full recipe details including ingredients, step-by-step method, and nutritional information
- Swipe right to favourite a recipe, swipe left to share
- Pull-to-refresh recipe list

### Food Scanner with Computer Vision and Networking
- Capture food photos using the device camera or pick from gallery
- On-device computer vision analyses the JPEG colour histogram (YCbCr hue bucket classification) to identify food categories
- Fetches real nutritional data from the Open Food Facts REST API (live HTTP GET requests with JSON deserialisation)
- Manual food name search sends user-initiated API requests and displays per-100g nutrition breakdown
- Displays data source indicator showing whether nutrition came from the API or local fallback

### Compass (Magnetometer)
- Real-time compass heading display using the device's magnetometer sensor
- Visual compass needle that rotates based on magnetic north reading
- Cardinal direction display (N, NE, E, SE, S, SW, W, NW)
- Instructions provided for testing on Android emulator via Extended Controls > Virtual Sensors

### Food Map and Geolocation
- View recipe origins from around the world (Naples, Sapporo, Sydney, Delhi, Paris, Bangkok, Tokyo)
- Get current GPS location using the device's geolocation hardware
- Open recipe origins in the device's native map application

### Shake to Discover
- Shake the device to get a random recipe suggestion (accelerometer hardware)
- Floating action button alternative for devices without shake support
- Vibration feedback confirms shake detection

### Settings and Accessibility
- Dark mode / Light mode / System theme switching
- High contrast mode exceeding WCAG AAA 7:1 contrast ratio
- Font size scaling from 75% to 200% (WCAG 1.4.4 Resize Text)
- All preferences persist across app sessions via Preferences API
- Reset all settings to defaults with confirmation dialog

---

## Hardware Features Used (7 Features)

| # | Hardware Feature | Location in App | Description |
|---|----------------|-----------------|-------------|
| 1 | **Camera** | Food Scanner page | Captures food photos via MediaPicker for identification |
| 2 | **Compass / Magnetometer** | Compass page | Real-time heading direction from magnetometer sensor |
| 3 | **Text-to-Speech** | Recipe Detail page | Reads recipe steps aloud for hands-free cooking |
| 4 | **Accelerometer (Shake)** | Recipes page | Shake detection triggers random recipe discovery |
| 5 | **Geolocation / GPS** | Map page | Gets user's current geographic coordinates |
| 6 | **Haptic Feedback** | Throughout app | Tactile confirmation on interactions via HardwareHelper |
| 7 | **Vibration** | Shake discover, Map page | Vibration pulses confirm actions (e.g., 400ms on shake) |

**Advanced Usage:** The camera feature includes on-device computer vision that analyses the colour histogram of captured JPEG images to classify food by dominant hue (red/green/yellow/brown/white), mapping to food categories. This satisfies the 86-100% requirement for advanced methods alongside mobile hardware.

---

## Networking

The application connects to the **Open Food Facts REST API** (https://world.openfoodfacts.org/api/v2) to fetch real nutritional data:

- **Search by name:** `GET /api/v2/search?categories_tags_en={food}&fields=product_name,nutriments&page_size=1`
- **Search by barcode:** `GET /api/v2/product/{barcode}.json`
- Uses `IHttpClientFactory` via `AddHttpClient<NutritionApiService>()` for proper connection lifetime management
- JSON deserialisation with `System.Text.Json` and strongly-typed response models
- Comprehensive error handling for network failures, timeouts, and malformed responses
- User-visible data source indicator shows whether data came from the API or local fallback

---

## Accessibility (WCAG 2.1 Compliance)

The application follows the Web Content Accessibility Guidelines (WCAG 2.1) at AA and AAA levels:

| WCAG Criterion | Implementation |
|---------------|----------------|
| 1.4.3 Contrast (Minimum) | All text meets 4.5:1 contrast ratio in both light and dark themes |
| 1.4.4 Resize Text | Font size adjustable from 75% to 200% without loss of functionality |
| 1.4.6 Contrast (Enhanced) | High contrast mode exceeds 7:1 ratio (WCAG AAA) |
| 1.4.11 Non-text Contrast | UI components (buttons, borders) maintain 3:1 contrast |
| 2.4.1 Bypass Blocks | Tab-based navigation allows direct access to any section |
| 2.4.2 Page Titled | Every page has a descriptive title via ViewModel Title property |
| 3.2.3 Consistent Navigation | Tab bar remains consistent across all pages |
| 3.3.1 Error Identification | Validation errors specify exactly what went wrong and how to fix it |
| 3.3.3 Error Suggestion | Error messages include corrective suggestions |
| 4.1.2 Name, Role, Value | All controls have SemanticProperties.Description and Hint |
| 4.1.3 Status Messages | SemanticScreenReader.Announce() called after every state change |

**Additional accessibility features:**
- Screen reader support via `SemanticProperties` on all interactive elements
- `SemanticScreenReader.Default.Announce()` for TalkBack/VoiceOver status messages
- Dark mode reduces eye strain in low-light environments
- Text-to-speech reads recipe steps aloud for hands-free or visually impaired users
- Clear user instructions available on the dedicated Help page

---

## Validation and Error Handling

Comprehensive validation is implemented throughout the application:

### Shopping List (RecipeDetailViewModel)
1. Empty input check — field cannot be blank
2. Non-integer check — rejects decimals, letters, symbols with specific error message
3. Zero/negative check — servings must be at least 1
4. Maximum check — rejects values above 20 with the user's actual input shown in the error
5. Missing ingredients check — handles recipes with no ingredient data

### Food Scanner (CameraViewModel)
- Camera availability validation before capture attempt
- Permission handling with user-friendly messages
- Network error handling with timeout detection
- Empty search term validation for manual nutrition lookup
- JSON parse error handling for unexpected API responses

### Nutrition API (NutritionApiService)
- `ArgumentException.ThrowIfNullOrWhiteSpace()` for null/empty inputs
- Barcode format validation via `[GeneratedRegex]` (8-14 digits only)
- HTTP status code validation before reading response body
- `HttpRequestException` catch for network connectivity issues
- `TaskCanceledException` catch for request timeouts
- `JsonException` catch for malformed API responses

### General
- All async methods wrapped in try-catch-finally blocks
- `IsBusy` guard prevents concurrent execution of commands
- `FeatureNotSupportedException` handled for all hardware features
- `PermissionException` handled with guidance to enable in device settings
- Error messages are clear, specific, and actionable (not generic "An error occurred")

---

## Architecture and Code Quality

### Design Patterns
- **MVVM (Model-View-ViewModel):** Strict separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection:** All services, ViewModels, and pages registered in MauiProgram.cs
- **Repository Pattern:** RecipeService abstracts data access from ViewModels
- **Observer Pattern:** ObservableProperty source generators for reactive UI binding

### Code Quality Principles
- **DRY (Don't Repeat Yourself):** HardwareHelper centralises haptic/vibration calls; MapNutrimentsToNutritionInfo reused by both search and barcode lookup; SetValidationError extracted from repeated validation pattern; AdjustFontSizeAsync handles both increase and decrease
- **KISS (Keep It Simple, Stupid):** Each method has a single responsibility; switch expressions for concise mapping; guard clauses for early returns
- **Naming Conventions:** PascalCase for public members, _camelCase for private fields, consistent throughout
- **Comments:** XML documentation on every public class, method, and property; inline comments explain non-obvious logic
- **Roslyn Analyser Fixes:** Code addresses CA1854, CA2000, CA2213, CA5394, IDE0028, IDE0031, IDE0042, IDE0130, SYSLIB1045 warnings with documented justifications

### Project Structure
```
FoodLens/
├── Helpers/
│ └── HardwareHelper.cs # Centralised hardware interaction (DRY)
├── Models/
│ ├── NutritionInfo.cs # Nutrition data model
│ ├── OpenFoodFactsModels.cs # API response DTOs
│ └── Recipe.cs # Recipe entity with validation
├── Services/
│ ├── NutritionApiService.cs # Open Food Facts API client (Networking)
│ └── RecipeService.cs # Recipe data repository
├── ViewModels/
│ ├── BaseViewModel.cs # Shared IsBusy/Title base class
│ ├── CameraViewModel.cs # Camera + CV + API networking
│ ├── CompassViewModel.cs # Magnetometer sensor reading
│ ├── RecipeDetailViewModel.cs # TTS, map, shopping list validation
│ ├── RecipesViewModel.cs # List, search, shake, swipe gestures
│ └── SettingsViewModel.cs # Theme, font size, high contrast
├── Views/
│ ├── CameraPage.xaml/.cs # Food scanner UI
│ ├── CompassPage.xaml/.cs # Compass UI with needle rotation
│ ├── HelpPage.xaml/.cs # User instructions (A11y requirement)
│ ├── MapPage.xaml/.cs # Geolocation + recipe origins
│ ├── RecipeDetailPage.xaml/.cs # Detail with pinch-to-zoom gesture
│ ├── RecipesPage.xaml/.cs # Main list with shake detection
│ └── SettingsPage.xaml/.cs # Accessibility settings
├── App.xaml/.cs # Theme engine + dynamic colours
├── AppShell.xaml/.cs # Tab navigation + route registration
└── MauiProgram.cs # DI container configuration
   ```
---

## Development Plan

The application was developed iteratively over the module duration:

| Phase | Features Implemented |
|-------|---------------------|
| **Phase 1 — Foundation** | Project setup, MVVM architecture, RecipeService, basic recipe list page with XAML |
| **Phase 2 — Core UI** | Recipe detail page, category filtering, search with debounce, navigation |
| **Phase 3 — Hardware** | Camera capture, accelerometer shake detection, haptic feedback, vibration |
| **Phase 4 — Advanced Hardware** | Compass/magnetometer, geolocation/GPS, text-to-speech |
| **Phase 5 — Networking** | Open Food Facts API integration, NutritionApiService, manual search |
| **Phase 6 — Computer Vision** | On-device colour histogram analysis for food classification |
| **Phase 7 — Accessibility** | Dark mode, high contrast, font scaling, SemanticProperties, screen reader announcements |
| **Phase 8 — Validation** | Shopping list validation, input guards, error handling throughout |
| **Phase 9 — Polish** | Swipe gestures, pinch-to-zoom, toast notifications, Help page, code quality fixes |
| **Phase 10 — Deployment** | Testing on Android emulator and Windows, cross-platform verification |

---

## Deployment

The application has been tested and deployed on:

| Platform | Device Type | Status |
|----------|-------------|--------|
| **Android** | Pixel 5 Emulator (API 33) | ✅ Fully functional |
| **Windows** | Windows 11 Desktop | ✅ Fully functional |

Both platforms support all features including camera (via emulator camera or webcam), compass (via Virtual Sensors on Android emulator), geolocation (spoofed on emulator), and networking (Open Food Facts API).

---

## How to Build and Run

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload installed
- .NET 8.0 SDK
- Android SDK (API 33+) for Android deployment
- Windows 10/11 for Windows deployment

### Steps
1. Clone the repository:```git clone [repository-url] ```
2. Open `FoodLens.sln` in Visual Studio 2022.
3. Restore NuGet packages (automatic on build):
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
4. Select target platform (Android Emulator or Windows Machine).
5. Press F5 or click the Run button.

### Testing Hardware Features on Emulator
- **Camera:** The Android emulator provides a simulated camera environment.
- **Compass:** Open Extended Controls (⋯ button) → Virtual Sensors → rotate the 3D model or adjust Yaw.
- **Shake:** Open Extended Controls → Virtual Sensors → click the "Move" button rapidly.
- **Geolocation:** Open Extended Controls → Location → set coordinates manually.
- **Haptic/Vibration:** Logged to Debug output on emulators that don't support physical feedback.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.x | MVVM source generators, ObservableProperty, RelayCommand |
| CommunityToolkit.Maui | 7.x | Value converters (InvertedBoolConverter, IsStringNotNullOrEmptyConverter) |
| Microsoft.Extensions.Http | 8.x | IHttpClientFactory for NutritionApiService |

---

## API Reference

**Open Food Facts API** (free, no API key required)  
- Base URL: `https://world.openfoodfacts.org/api/v2`
- Documentation: https://world.openfoodfacts.org/data
- Used for: Real-time nutritional data retrieval by food name or barcode

---

## Screencast Checklist

The screencast demonstrates the following criteria:

- [x] UI/UX Design: Consistent styling, XAML layouts, smooth navigation, uncluttered design
- [x] Accessibility: Dark mode, high contrast, font scaling, screen reader support, WCAG references
- [x] Hardware (7 features): Camera, Compass, TTS, Shake, Geolocation, Haptic, Vibration
- [x] Computer Vision: On-device colour histogram analysis of captured photos
- [x] Networking: Live Open Food Facts API calls with visible data source indicator
- [x] Functionality: All buttons, gestures (swipe, pinch, shake), navigation, and features working
- [x] Validation: Shopping list input validation with 5 distinct checks and clear error messages
- [x] Error Handling: Network errors, permission errors, missing resources handled gracefully
- [x] Code Quality: Comments, naming conventions, DRY/KISS principles, Roslyn fixes
- [x] Deployment: Android emulator and Windows desktop
- [x] GitHub: Regular commits with descriptive messages showing development progress

---

## License

This project was developed for academic purposes as part of the 6G6Z0014 Mobile Computing module at Manchester Metropolitan University.# FoodLens

A cross-platform mobile application built with .NET MAUI for discovering, exploring, and interacting with food recipes from around the world. Developed as part of the 6G6Z0014 Mobile Computing module.

**Author:** [Your Name]  
**Student ID:** [Your Student ID]  
**Module:** 6G6Z0014 Mobile Computing  
**Framework:** .NET MAUI (.NET Multi-platform App UI)  
**Theme:** Food and Drink  

---

## Application Overview

FoodLens is a recipe discovery application that allows users to browse international recipes, scan food items using the device camera with on-device computer vision, fetch real nutritional data from the Open Food Facts REST API, navigate recipe origins on a map, and customise the app's appearance for accessibility. The app demonstrates extensive use of mobile hardware sensors, networking, MVVM architecture, and WCAG 2.1 accessibility compliance.

---

## Features

### Recipe Browsing
- Browse recipes by category (All, Breakfast, Dinner, Dessert, Drinks)
- Search recipes by name or description with debounced input (350ms delay to optimise performance)
- View full recipe details including ingredients, step-by-step method, and nutritional information
- Swipe right to favourite a recipe, swipe left to share
- Pull-to-refresh recipe list

### Food Scanner with Computer Vision and Networking
- Capture food photos using the device camera or pick from gallery
- On-device computer vision analyses the JPEG colour histogram (YCbCr hue bucket classification) to identify food categories
- Fetches real nutritional data from the Open Food Facts REST API (live HTTP GET requests with JSON deserialisation)
- Manual food name search sends user-initiated API requests and displays per-100g nutrition breakdown
- Displays data source indicator showing whether nutrition came from the API or local fallback

### Compass (Magnetometer)
- Real-time compass heading display using the device's magnetometer sensor
- Visual compass needle that rotates based on magnetic north reading
- Cardinal direction display (N, NE, E, SE, S, SW, W, NW)
- Instructions provided for testing on Android emulator via Extended Controls > Virtual Sensors

### Food Map and Geolocation
- View recipe origins from around the world (Naples, Sapporo, Sydney, Delhi, Paris, Bangkok, Tokyo)
- Get current GPS location using the device's geolocation hardware
- Open recipe origins in the device's native map application

### Shake to Discover
- Shake the device to get a random recipe suggestion (accelerometer hardware)
- Floating action button alternative for devices without shake support
- Vibration feedback confirms shake detection

### Settings and Accessibility
- Dark mode / Light mode / System theme switching
- High contrast mode exceeding WCAG AAA 7:1 contrast ratio
- Font size scaling from 75% to 200% (WCAG 1.4.4 Resize Text)
- All preferences persist across app sessions via Preferences API
- Reset all settings to defaults with confirmation dialog

---

## Hardware Features Used (7 Features)

| # | Hardware Feature | Location in App | Description |
|---|----------------|-----------------|-------------|
| 1 | **Camera** | Food Scanner page | Captures food photos via MediaPicker for identification |
| 2 | **Compass / Magnetometer** | Compass page | Real-time heading direction from magnetometer sensor |
| 3 | **Text-to-Speech** | Recipe Detail page | Reads recipe steps aloud for hands-free cooking |
| 4 | **Accelerometer (Shake)** | Recipes page | Shake detection triggers random recipe discovery |
| 5 | **Geolocation / GPS** | Map page | Gets user's current geographic coordinates |
| 6 | **Haptic Feedback** | Throughout app | Tactile confirmation on interactions via HardwareHelper |
| 7 | **Vibration** | Shake discover, Map page | Vibration pulses confirm actions (e.g., 400ms on shake) |

**Advanced Usage:** The camera feature includes on-device computer vision that analyses the colour histogram of captured JPEG images to classify food by dominant hue (red/green/yellow/brown/white), mapping to food categories. This satisfies the 86-100% requirement for advanced methods alongside mobile hardware.

---

## Networking

The application connects to the **Open Food Facts REST API** (https://world.openfoodfacts.org/api/v2) to fetch real nutritional data:

- **Search by name:** `GET /api/v2/search?categories_tags_en={food}&fields=product_name,nutriments&page_size=1`
- **Search by barcode:** `GET /api/v2/product/{barcode}.json`
- Uses `IHttpClientFactory` via `AddHttpClient<NutritionApiService>()` for proper connection lifetime management
- JSON deserialisation with `System.Text.Json` and strongly-typed response models
- Comprehensive error handling for network failures, timeouts, and malformed responses
- User-visible data source indicator shows whether data came from the API or local fallback

---

## Accessibility (WCAG 2.1 Compliance)

The application follows the Web Content Accessibility Guidelines (WCAG 2.1) at AA and AAA levels:

| WCAG Criterion | Implementation |
|---------------|----------------|
| 1.4.3 Contrast (Minimum) | All text meets 4.5:1 contrast ratio in both light and dark themes |
| 1.4.4 Resize Text | Font size adjustable from 75% to 200% without loss of functionality |
| 1.4.6 Contrast (Enhanced) | High contrast mode exceeds 7:1 ratio (WCAG AAA) |
| 1.4.11 Non-text Contrast | UI components (buttons, borders) maintain 3:1 contrast |
| 2.4.1 Bypass Blocks | Tab-based navigation allows direct access to any section |
| 2.4.2 Page Titled | Every page has a descriptive title via ViewModel Title property |
| 3.2.3 Consistent Navigation | Tab bar remains consistent across all pages |
| 3.3.1 Error Identification | Validation errors specify exactly what went wrong and how to fix it |
| 3.3.3 Error Suggestion | Error messages include corrective suggestions |
| 4.1.2 Name, Role, Value | All controls have SemanticProperties.Description and Hint |
| 4.1.3 Status Messages | SemanticScreenReader.Announce() called after every state change |

**Additional accessibility features:**
- Screen reader support via `SemanticProperties` on all interactive elements
- `SemanticScreenReader.Default.Announce()` for TalkBack/VoiceOver status messages
- Dark mode reduces eye strain in low-light environments
- Text-to-speech reads recipe steps aloud for hands-free or visually impaired users
- Clear user instructions available on the dedicated Help page

---

## Validation and Error Handling

Comprehensive validation is implemented throughout the application:

### Shopping List (RecipeDetailViewModel)
1. Empty input check — field cannot be blank
2. Non-integer check — rejects decimals, letters, symbols with specific error message
3. Zero/negative check — servings must be at least 1
4. Maximum check — rejects values above 20 with the user's actual input shown in the error
5. Missing ingredients check — handles recipes with no ingredient data

### Food Scanner (CameraViewModel)
- Camera availability validation before capture attempt
- Permission handling with user-friendly messages
- Network error handling with timeout detection
- Empty search term validation for manual nutrition lookup
- JSON parse error handling for unexpected API responses

### Nutrition API (NutritionApiService)
- `ArgumentException.ThrowIfNullOrWhiteSpace()` for null/empty inputs
- Barcode format validation via `[GeneratedRegex]` (8-14 digits only)
- HTTP status code validation before reading response body
- `HttpRequestException` catch for network connectivity issues
- `TaskCanceledException` catch for request timeouts
- `JsonException` catch for malformed API responses

### General
- All async methods wrapped in try-catch-finally blocks
- `IsBusy` guard prevents concurrent execution of commands
- `FeatureNotSupportedException` handled for all hardware features
- `PermissionException` handled with guidance to enable in device settings
- Error messages are clear, specific, and actionable (not generic "An error occurred")

---

## Architecture and Code Quality

### Design Patterns
- **MVVM (Model-View-ViewModel):** Strict separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection:** All services, ViewModels, and pages registered in MauiProgram.cs
- **Repository Pattern:** RecipeService abstracts data access from ViewModels
- **Observer Pattern:** ObservableProperty source generators for reactive UI binding

### Code Quality Principles
- **DRY (Don't Repeat Yourself):** HardwareHelper centralises haptic/vibration calls; MapNutrimentsToNutritionInfo reused by both search and barcode lookup; SetValidationError extracted from repeated validation pattern; AdjustFontSizeAsync handles both increase and decrease
- **KISS (Keep It Simple, Stupid):** Each method has a single responsibility; switch expressions for concise mapping; guard clauses for early returns
- **Naming Conventions:** PascalCase for public members, _camelCase for private fields, consistent throughout
- **Comments:** XML documentation on every public class, method, and property; inline comments explain non-obvious logic
- **Roslyn Analyser Fixes:** Code addresses CA1854, CA2000, CA2213, CA5394, IDE0028, IDE0031, IDE0042, IDE0130, SYSLIB1045 warnings with documented justifications

### Project Structure
FoodLens/
├── Helpers/
│ └── HardwareHelper.cs # Centralised hardware interaction (DRY)
├── Models/
│ ├── NutritionInfo.cs # Nutrition data model
│ ├── OpenFoodFactsModels.cs # API response DTOs
│ └── Recipe.cs # Recipe entity with validation
├── Services/
│ ├── NutritionApiService.cs # Open Food Facts API client (Networking)
│ └── RecipeService.cs # Recipe data repository
├── ViewModels/
│ ├── BaseViewModel.cs # Shared IsBusy/Title base class
│ ├── CameraViewModel.cs # Camera + CV + API networking
│ ├── CompassViewModel.cs # Magnetometer sensor reading
│ ├── RecipeDetailViewModel.cs # TTS, map, shopping list validation
│ ├── RecipesViewModel.cs # List, search, shake, swipe gestures
│ └── SettingsViewModel.cs # Theme, font size, high contrast
├── Views/
│ ├── CameraPage.xaml/.cs # Food scanner UI
│ ├── CompassPage.xaml/.cs # Compass UI with needle rotation
│ ├── HelpPage.xaml/.cs # User instructions (A11y requirement)
│ ├── MapPage.xaml/.cs # Geolocation + recipe origins
│ ├── RecipeDetailPage.xaml/.cs # Detail with pinch-to-zoom gesture
│ ├── RecipesPage.xaml/.cs # Main list with shake detection
│ └── SettingsPage.xaml/.cs # Accessibility settings
├── App.xaml/.cs # Theme engine + dynamic colours
├── AppShell.xaml/.cs # Tab navigation + route registration
└── MauiProgram.cs # DI container configuration

---

## Development Plan

The application was developed iteratively over the module duration:

| Phase | Features Implemented |
|-------|---------------------|
| **Phase 1 — Foundation** | Project setup, MVVM architecture, RecipeService, basic recipe list page with XAML |
| **Phase 2 — Core UI** | Recipe detail page, category filtering, search with debounce, navigation |
| **Phase 3 — Hardware** | Camera capture, accelerometer shake detection, haptic feedback, vibration |
| **Phase 4 — Advanced Hardware** | Compass/magnetometer, geolocation/GPS, text-to-speech |
| **Phase 5 — Networking** | Open Food Facts API integration, NutritionApiService, manual search |
| **Phase 6 — Computer Vision** | On-device colour histogram analysis for food classification |
| **Phase 7 — Accessibility** | Dark mode, high contrast, font scaling, SemanticProperties, screen reader announcements |
| **Phase 8 — Validation** | Shopping list validation, input guards, error handling throughout |
| **Phase 9 — Polish** | Swipe gestures, pinch-to-zoom, toast notifications, Help page, code quality fixes |
| **Phase 10 — Deployment** | Testing on Android emulator and Windows, cross-platform verification |

---

## Deployment

The application has been tested and deployed on:

| Platform | Device Type | Status |
|----------|-------------|--------|
| **Android** | Pixel 5 Emulator (API 33) | ✅ Fully functional |
| **Windows** | Windows 11 Desktop | ✅ Fully functional |

Both platforms support all features including camera (via emulator camera or webcam), compass (via Virtual Sensors on Android emulator), geolocation (spoofed on emulator), and networking (Open Food Facts API).

---

## How to Build and Run

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload installed
- .NET 8.0 SDK
- Android SDK (API 33+) for Android deployment
- Windows 10/11 for Windows deployment

### Steps
1. Clone the repository:
	git clone [repository-url]
2. Open `FoodLens.sln` in Visual Studio 2022.
3. Restore NuGet packages (automatic on build):
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
4. Select target platform (Android Emulator or Windows Machine).
5. Press F5 or click the Run button.

### Testing Hardware Features on Emulator
- **Camera:** The Android emulator provides a simulated camera environment.
- **Compass:** Open Extended Controls (⋯ button) → Virtual Sensors → rotate the 3D model or adjust Yaw.
- **Shake:** Open Extended Controls → Virtual Sensors → click the "Move" button rapidly.
- **Geolocation:** Open Extended Controls → Location → set coordinates manually.
- **Haptic/Vibration:** Logged to Debug output on emulators that don't support physical feedback.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.x | MVVM source generators, ObservableProperty, RelayCommand |
| CommunityToolkit.Maui | 7.x | Value converters (InvertedBoolConverter, IsStringNotNullOrEmptyConverter) |
| Microsoft.Extensions.Http | 8.x | IHttpClientFactory for NutritionApiService |

---

## API Reference

**Open Food Facts API** (free, no API key required)  
- Base URL: `https://world.openfoodfacts.org/api/v2`
- Documentation: https://world.openfoodfacts.org/data
- Used for: Real-time nutritional data retrieval by food name or barcode

---

## Screencast Checklist

The screencast demonstrates the following criteria:

- [x] UI/UX Design: Consistent styling, XAML layouts, smooth navigation, uncluttered design
- [x] Accessibility: Dark mode, high contrast, font scaling, screen reader support, WCAG references
- [x] Hardware (7 features): Camera, Compass, TTS, Shake, Geolocation, Haptic, Vibration
- [x] Computer Vision: On-device colour histogram analysis of captured photos
- [x] Networking: Live Open Food Facts API calls with visible data source indicator
- [x] Functionality: All buttons, gestures (swipe, pinch, shake), navigation, and features working
- [x] Validation: Shopping list input validation with 5 distinct checks and clear error messages
- [x] Error Handling: Network errors, permission errors, missing resources handled gracefully
- [x] Code Quality: Comments, naming conventions, DRY/KISS principles, Roslyn fixes
- [x] Deployment: Android emulator and Windows desktop
- [x] GitHub: Regular commits with descriptive messages showing development progress

---

## License

This project was developed for academic purposes as part of the 6G6Z0014 Mobile Computing module at Manchester Metropolitan University.# FoodLens

A cross-platform mobile application built with .NET MAUI for discovering, exploring, and interacting with food recipes from around the world. Developed as part of the 6G6Z0014 Mobile Computing module.

**Author:** [Your Name]  
**Student ID:** [Your Student ID]  
**Module:** 6G6Z0014 Mobile Computing  
**Framework:** .NET MAUI (.NET Multi-platform App UI)  
**Theme:** Food and Drink  

---

## Application Overview

FoodLens is a recipe discovery application that allows users to browse international recipes, scan food items using the device camera with on-device computer vision, fetch real nutritional data from the Open Food Facts REST API, navigate recipe origins on a map, and customise the app's appearance for accessibility. The app demonstrates extensive use of mobile hardware sensors, networking, MVVM architecture, and WCAG 2.1 accessibility compliance.

---

## Features

### Recipe Browsing
- Browse recipes by category (All, Breakfast, Dinner, Dessert, Drinks)
- Search recipes by name or description with debounced input (350ms delay to optimise performance)
- View full recipe details including ingredients, step-by-step method, and nutritional information
- Swipe right to favourite a recipe, swipe left to share
- Pull-to-refresh recipe list

### Food Scanner with Computer Vision and Networking
- Capture food photos using the device camera or pick from gallery
- On-device computer vision analyses the JPEG colour histogram (YCbCr hue bucket classification) to identify food categories
- Fetches real nutritional data from the Open Food Facts REST API (live HTTP GET requests with JSON deserialisation)
- Manual food name search sends user-initiated API requests and displays per-100g nutrition breakdown
- Displays data source indicator showing whether nutrition came from the API or local fallback

### Compass (Magnetometer)
- Real-time compass heading display using the device's magnetometer sensor
- Visual compass needle that rotates based on magnetic north reading
- Cardinal direction display (N, NE, E, SE, S, SW, W, NW)
- Instructions provided for testing on Android emulator via Extended Controls > Virtual Sensors

### Food Map and Geolocation
- View recipe origins from around the world (Naples, Sapporo, Sydney, Delhi, Paris, Bangkok, Tokyo)
- Get current GPS location using the device's geolocation hardware
- Open recipe origins in the device's native map application

### Shake to Discover
- Shake the device to get a random recipe suggestion (accelerometer hardware)
- Floating action button alternative for devices without shake support
- Vibration feedback confirms shake detection

### Settings and Accessibility
- Dark mode / Light mode / System theme switching
- High contrast mode exceeding WCAG AAA 7:1 contrast ratio
- Font size scaling from 75% to 200% (WCAG 1.4.4 Resize Text)
- All preferences persist across app sessions via Preferences API
- Reset all settings to defaults with confirmation dialog

---

## Hardware Features Used (7 Features)

| # | Hardware Feature | Location in App | Description |
|---|----------------|-----------------|-------------|
| 1 | **Camera** | Food Scanner page | Captures food photos via MediaPicker for identification |
| 2 | **Compass / Magnetometer** | Compass page | Real-time heading direction from magnetometer sensor |
| 3 | **Text-to-Speech** | Recipe Detail page | Reads recipe steps aloud for hands-free cooking |
| 4 | **Accelerometer (Shake)** | Recipes page | Shake detection triggers random recipe discovery |
| 5 | **Geolocation / GPS** | Map page | Gets user's current geographic coordinates |
| 6 | **Haptic Feedback** | Throughout app | Tactile confirmation on interactions via HardwareHelper |
| 7 | **Vibration** | Shake discover, Map page | Vibration pulses confirm actions (e.g., 400ms on shake) |

**Advanced Usage:** The camera feature includes on-device computer vision that analyses the colour histogram of captured JPEG images to classify food by dominant hue (red/green/yellow/brown/white), mapping to food categories. This satisfies the 86-100% requirement for advanced methods alongside mobile hardware.

---

## Networking

The application connects to the **Open Food Facts REST API** (https://world.openfoodfacts.org/api/v2) to fetch real nutritional data:

- **Search by name:** `GET /api/v2/search?categories_tags_en={food}&fields=product_name,nutriments&page_size=1`
- **Search by barcode:** `GET /api/v2/product/{barcode}.json`
- Uses `IHttpClientFactory` via `AddHttpClient<NutritionApiService>()` for proper connection lifetime management
- JSON deserialisation with `System.Text.Json` and strongly-typed response models
- Comprehensive error handling for network failures, timeouts, and malformed responses
- User-visible data source indicator shows whether data came from the API or local fallback

---

## Accessibility (WCAG 2.1 Compliance)

The application follows the Web Content Accessibility Guidelines (WCAG 2.1) at AA and AAA levels:

| WCAG Criterion | Implementation |
|---------------|----------------|
| 1.4.3 Contrast (Minimum) | All text meets 4.5:1 contrast ratio in both light and dark themes |
| 1.4.4 Resize Text | Font size adjustable from 75% to 200% without loss of functionality |
| 1.4.6 Contrast (Enhanced) | High contrast mode exceeds 7:1 ratio (WCAG AAA) |
| 1.4.11 Non-text Contrast | UI components (buttons, borders) maintain 3:1 contrast |
| 2.4.1 Bypass Blocks | Tab-based navigation allows direct access to any section |
| 2.4.2 Page Titled | Every page has a descriptive title via ViewModel Title property |
| 3.2.3 Consistent Navigation | Tab bar remains consistent across all pages |
| 3.3.1 Error Identification | Validation errors specify exactly what went wrong and how to fix it |
| 3.3.3 Error Suggestion | Error messages include corrective suggestions |
| 4.1.2 Name, Role, Value | All controls have SemanticProperties.Description and Hint |
| 4.1.3 Status Messages | SemanticScreenReader.Announce() called after every state change |

**Additional accessibility features:**
- Screen reader support via `SemanticProperties` on all interactive elements
- `SemanticScreenReader.Default.Announce()` for TalkBack/VoiceOver status messages
- Dark mode reduces eye strain in low-light environments
- Text-to-speech reads recipe steps aloud for hands-free or visually impaired users
- Clear user instructions available on the dedicated Help page

---

## Validation and Error Handling

Comprehensive validation is implemented throughout the application:

### Shopping List (RecipeDetailViewModel)
1. Empty input check — field cannot be blank
2. Non-integer check — rejects decimals, letters, symbols with specific error message
3. Zero/negative check — servings must be at least 1
4. Maximum check — rejects values above 20 with the user's actual input shown in the error
5. Missing ingredients check — handles recipes with no ingredient data

### Food Scanner (CameraViewModel)
- Camera availability validation before capture attempt
- Permission handling with user-friendly messages
- Network error handling with timeout detection
- Empty search term validation for manual nutrition lookup
- JSON parse error handling for unexpected API responses

### Nutrition API (NutritionApiService)
- `ArgumentException.ThrowIfNullOrWhiteSpace()` for null/empty inputs
- Barcode format validation via `[GeneratedRegex]` (8-14 digits only)
- HTTP status code validation before reading response body
- `HttpRequestException` catch for network connectivity issues
- `TaskCanceledException` catch for request timeouts
- `JsonException` catch for malformed API responses

### General
- All async methods wrapped in try-catch-finally blocks
- `IsBusy` guard prevents concurrent execution of commands
- `FeatureNotSupportedException` handled for all hardware features
- `PermissionException` handled with guidance to enable in device settings
- Error messages are clear, specific, and actionable (not generic "An error occurred")

---

## Architecture and Code Quality

### Design Patterns
- **MVVM (Model-View-ViewModel):** Strict separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection:** All services, ViewModels, and pages registered in MauiProgram.cs
- **Repository Pattern:** RecipeService abstracts data access from ViewModels
- **Observer Pattern:** ObservableProperty source generators for reactive UI binding

### Code Quality Principles
- **DRY (Don't Repeat Yourself):** HardwareHelper centralises haptic/vibration calls; MapNutrimentsToNutritionInfo reused by both search and barcode lookup; SetValidationError extracted from repeated validation pattern; AdjustFontSizeAsync handles both increase and decrease
- **KISS (Keep It Simple, Stupid):** Each method has a single responsibility; switch expressions for concise mapping; guard clauses for early returns
- **Naming Conventions:** PascalCase for public members, _camelCase for private fields, consistent throughout
- **Comments:** XML documentation on every public class, method, and property; inline comments explain non-obvious logic
- **Roslyn Analyser Fixes:** Code addresses CA1854, CA2000, CA2213, CA5394, IDE0028, IDE0031, IDE0042, IDE0130, SYSLIB1045 warnings with documented justifications

### Project Structure
FoodLens/
├── Helpers/
│ └── HardwareHelper.cs # Centralised hardware interaction (DRY)
├── Models/
│ ├── NutritionInfo.cs # Nutrition data model
│ ├── OpenFoodFactsModels.cs # API response DTOs
│ └── Recipe.cs # Recipe entity with validation
├── Services/
│ ├── NutritionApiService.cs # Open Food Facts API client (Networking)
│ └── RecipeService.cs # Recipe data repository
├── ViewModels/
│ ├── BaseViewModel.cs # Shared IsBusy/Title base class
│ ├── CameraViewModel.cs # Camera + CV + API networking
│ ├── CompassViewModel.cs # Magnetometer sensor reading
│ ├── RecipeDetailViewModel.cs # TTS, map, shopping list validation
│ ├── RecipesViewModel.cs # List, search, shake, swipe gestures
│ └── SettingsViewModel.cs # Theme, font size, high contrast
├── Views/
│ ├── CameraPage.xaml/.cs # Food scanner UI
│ ├── CompassPage.xaml/.cs # Compass UI with needle rotation
│ ├── HelpPage.xaml/.cs # User instructions (A11y requirement)
│ ├── MapPage.xaml/.cs # Geolocation + recipe origins
│ ├── RecipeDetailPage.xaml/.cs # Detail with pinch-to-zoom gesture
│ ├── RecipesPage.xaml/.cs # Main list with shake detection
│ └── SettingsPage.xaml/.cs # Accessibility settings
├── App.xaml/.cs # Theme engine + dynamic colours
├── AppShell.xaml/.cs # Tab navigation + route registration
└── MauiProgram.cs # DI container configuration

---

## Development Plan

The application was developed iteratively over the module duration:

| Phase | Features Implemented |
|-------|---------------------|
| **Phase 1 — Foundation** | Project setup, MVVM architecture, RecipeService, basic recipe list page with XAML |
| **Phase 2 — Core UI** | Recipe detail page, category filtering, search with debounce, navigation |
| **Phase 3 — Hardware** | Camera capture, accelerometer shake detection, haptic feedback, vibration |
| **Phase 4 — Advanced Hardware** | Compass/magnetometer, geolocation/GPS, text-to-speech |
| **Phase 5 — Networking** | Open Food Facts API integration, NutritionApiService, manual search |
| **Phase 6 — Computer Vision** | On-device colour histogram analysis for food classification |
| **Phase 7 — Accessibility** | Dark mode, high contrast, font scaling, SemanticProperties, screen reader announcements |
| **Phase 8 — Validation** | Shopping list validation, input guards, error handling throughout |
| **Phase 9 — Polish** | Swipe gestures, pinch-to-zoom, toast notifications, Help page, code quality fixes |
| **Phase 10 — Deployment** | Testing on Android emulator and Windows, cross-platform verification |

---

## Deployment

The application has been tested and deployed on:

| Platform | Device Type | Status |
|----------|-------------|--------|
| **Android** | Pixel 5 Emulator (API 33) | ✅ Fully functional |
| **Windows** | Windows 11 Desktop | ✅ Fully functional |

Both platforms support all features including camera (via emulator camera or webcam), compass (via Virtual Sensors on Android emulator), geolocation (spoofed on emulator), and networking (Open Food Facts API).

---

## How to Build and Run

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload installed
- .NET 8.0 SDK
- Android SDK (API 33+) for Android deployment
- Windows 10/11 for Windows deployment

### Steps
1. Clone the repository:
	git clone [repository-url]
2. Open `FoodLens.sln` in Visual Studio 2022.
3. Restore NuGet packages (automatic on build):
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
4. Select target platform (Android Emulator or Windows Machine).
5. Press F5 or click the Run button.

### Testing Hardware Features on Emulator
- **Camera:** The Android emulator provides a simulated camera environment.
- **Compass:** Open Extended Controls (⋯ button) → Virtual Sensors → rotate the 3D model or adjust Yaw.
- **Shake:** Open Extended Controls → Virtual Sensors → click the "Move" button rapidly.
- **Geolocation:** Open Extended Controls → Location → set coordinates manually.
- **Haptic/Vibration:** Logged to Debug output on emulators that don't support physical feedback.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.x | MVVM source generators, ObservableProperty, RelayCommand |
| CommunityToolkit.Maui | 7.x | Value converters (InvertedBoolConverter, IsStringNotNullOrEmptyConverter) |
| Microsoft.Extensions.Http | 8.x | IHttpClientFactory for NutritionApiService |

---

## API Reference

**Open Food Facts API** (free, no API key required)  
- Base URL: `https://world.openfoodfacts.org/api/v2`
- Documentation: https://world.openfoodfacts.org/data
- Used for: Real-time nutritional data retrieval by food name or barcode

---

## Screencast Checklist

The screencast demonstrates the following criteria:

- [x] UI/UX Design: Consistent styling, XAML layouts, smooth navigation, uncluttered design
- [x] Accessibility: Dark mode, high contrast, font scaling, screen reader support, WCAG references
- [x] Hardware (7 features): Camera, Compass, TTS, Shake, Geolocation, Haptic, Vibration
- [x] Computer Vision: On-device colour histogram analysis of captured photos
- [x] Networking: Live Open Food Facts API calls with visible data source indicator
- [x] Functionality: All buttons, gestures (swipe, pinch, shake), navigation, and features working
- [x] Validation: Shopping list input validation with 5 distinct checks and clear error messages
- [x] Error Handling: Network errors, permission errors, missing resources handled gracefully
- [x] Code Quality: Comments, naming conventions, DRY/KISS principles, Roslyn fixes
- [x] Deployment: Android emulator and Windows desktop
- [x] GitHub: Regular commits with descriptive messages showing development progress

---

## License

This project was developed for academic purposes as part of the 6G6Z0014 Mobile Computing module at Manchester Metropolitan University.# FoodLens

A cross-platform mobile application built with .NET MAUI for discovering, exploring, and interacting with food recipes from around the world. Developed as part of the 6G6Z0014 Mobile Computing module.

**Author:** [Your Name]  
**Student ID:** [Your Student ID]  
**Module:** 6G6Z0014 Mobile Computing  
**Framework:** .NET MAUI (.NET Multi-platform App UI)  
**Theme:** Food and Drink  

---

## Application Overview

FoodLens is a recipe discovery application that allows users to browse international recipes, scan food items using the device camera with on-device computer vision, fetch real nutritional data from the Open Food Facts REST API, navigate recipe origins on a map, and customise the app's appearance for accessibility. The app demonstrates extensive use of mobile hardware sensors, networking, MVVM architecture, and WCAG 2.1 accessibility compliance.

---

## Features

### Recipe Browsing
- Browse recipes by category (All, Breakfast, Dinner, Dessert, Drinks)
- Search recipes by name or description with debounced input (350ms delay to optimise performance)
- View full recipe details including ingredients, step-by-step method, and nutritional information
- Swipe right to favourite a recipe, swipe left to share
- Pull-to-refresh recipe list

### Food Scanner with Computer Vision and Networking
- Capture food photos using the device camera or pick from gallery
- On-device computer vision analyses the JPEG colour histogram (YCbCr hue bucket classification) to identify food categories
- Fetches real nutritional data from the Open Food Facts REST API (live HTTP GET requests with JSON deserialisation)
- Manual food name search sends user-initiated API requests and displays per-100g nutrition breakdown
- Displays data source indicator showing whether nutrition came from the API or local fallback

### Compass (Magnetometer)
- Real-time compass heading display using the device's magnetometer sensor
- Visual compass needle that rotates based on magnetic north reading
- Cardinal direction display (N, NE, E, SE, S, SW, W, NW)
- Instructions provided for testing on Android emulator via Extended Controls > Virtual Sensors

### Food Map and Geolocation
- View recipe origins from around the world (Naples, Sapporo, Sydney, Delhi, Paris, Bangkok, Tokyo)
- Get current GPS location using the device's geolocation hardware
- Open recipe origins in the device's native map application

### Shake to Discover
- Shake the device to get a random recipe suggestion (accelerometer hardware)
- Floating action button alternative for devices without shake support
- Vibration feedback confirms shake detection

### Settings and Accessibility
- Dark mode / Light mode / System theme switching
- High contrast mode exceeding WCAG AAA 7:1 contrast ratio
- Font size scaling from 75% to 200% (WCAG 1.4.4 Resize Text)
- All preferences persist across app sessions via Preferences API
- Reset all settings to defaults with confirmation dialog

---

## Hardware Features Used (7 Features)

| # | Hardware Feature | Location in App | Description |
|---|----------------|-----------------|-------------|
| 1 | **Camera** | Food Scanner page | Captures food photos via MediaPicker for identification |
| 2 | **Compass / Magnetometer** | Compass page | Real-time heading direction from magnetometer sensor |
| 3 | **Text-to-Speech** | Recipe Detail page | Reads recipe steps aloud for hands-free cooking |
| 4 | **Accelerometer (Shake)** | Recipes page | Shake detection triggers random recipe discovery |
| 5 | **Geolocation / GPS** | Map page | Gets user's current geographic coordinates |
| 6 | **Haptic Feedback** | Throughout app | Tactile confirmation on interactions via HardwareHelper |
| 7 | **Vibration** | Shake discover, Map page | Vibration pulses confirm actions (e.g., 400ms on shake) |

**Advanced Usage:** The camera feature includes on-device computer vision that analyses the colour histogram of captured JPEG images to classify food by dominant hue (red/green/yellow/brown/white), mapping to food categories. This satisfies the 86-100% requirement for advanced methods alongside mobile hardware.

---

## Networking

The application connects to the **Open Food Facts REST API** (https://world.openfoodfacts.org/api/v2) to fetch real nutritional data:

- **Search by name:** `GET /api/v2/search?categories_tags_en={food}&fields=product_name,nutriments&page_size=1`
- **Search by barcode:** `GET /api/v2/product/{barcode}.json`
- Uses `IHttpClientFactory` via `AddHttpClient<NutritionApiService>()` for proper connection lifetime management
- JSON deserialisation with `System.Text.Json` and strongly-typed response models
- Comprehensive error handling for network failures, timeouts, and malformed responses
- User-visible data source indicator shows whether data came from the API or local fallback

---

## Accessibility (WCAG 2.1 Compliance)

The application follows the Web Content Accessibility Guidelines (WCAG 2.1) at AA and AAA levels:

| WCAG Criterion | Implementation |
|---------------|----------------|
| 1.4.3 Contrast (Minimum) | All text meets 4.5:1 contrast ratio in both light and dark themes |
| 1.4.4 Resize Text | Font size adjustable from 75% to 200% without loss of functionality |
| 1.4.6 Contrast (Enhanced) | High contrast mode exceeds 7:1 ratio (WCAG AAA) |
| 1.4.11 Non-text Contrast | UI components (buttons, borders) maintain 3:1 contrast |
| 2.4.1 Bypass Blocks | Tab-based navigation allows direct access to any section |
| 2.4.2 Page Titled | Every page has a descriptive title via ViewModel Title property |
| 3.2.3 Consistent Navigation | Tab bar remains consistent across all pages |
| 3.3.1 Error Identification | Validation errors specify exactly what went wrong and how to fix it |
| 3.3.3 Error Suggestion | Error messages include corrective suggestions |
| 4.1.2 Name, Role, Value | All controls have SemanticProperties.Description and Hint |
| 4.1.3 Status Messages | SemanticScreenReader.Announce() called after every state change |

**Additional accessibility features:**
- Screen reader support via `SemanticProperties` on all interactive elements
- `SemanticScreenReader.Default.Announce()` for TalkBack/VoiceOver status messages
- Dark mode reduces eye strain in low-light environments
- Text-to-speech reads recipe steps aloud for hands-free or visually impaired users
- Clear user instructions available on the dedicated Help page

---

## Validation and Error Handling

Comprehensive validation is implemented throughout the application:

### Shopping List (RecipeDetailViewModel)
1. Empty input check — field cannot be blank
2. Non-integer check — rejects decimals, letters, symbols with specific error message
3. Zero/negative check — servings must be at least 1
4. Maximum check — rejects values above 20 with the user's actual input shown in the error
5. Missing ingredients check — handles recipes with no ingredient data

### Food Scanner (CameraViewModel)
- Camera availability validation before capture attempt
- Permission handling with user-friendly messages
- Network error handling with timeout detection
- Empty search term validation for manual nutrition lookup
- JSON parse error handling for unexpected API responses

### Nutrition API (NutritionApiService)
- `ArgumentException.ThrowIfNullOrWhiteSpace()` for null/empty inputs
- Barcode format validation via `[GeneratedRegex]` (8-14 digits only)
- HTTP status code validation before reading response body
- `HttpRequestException` catch for network connectivity issues
- `TaskCanceledException` catch for request timeouts
- `JsonException` catch for malformed API responses

### General
- All async methods wrapped in try-catch-finally blocks
- `IsBusy` guard prevents concurrent execution of commands
- `FeatureNotSupportedException` handled for all hardware features
- `PermissionException` handled with guidance to enable in device settings
- Error messages are clear, specific, and actionable (not generic "An error occurred")

---

## Architecture and Code Quality

### Design Patterns
- **MVVM (Model-View-ViewModel):** Strict separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection:** All services, ViewModels, and pages registered in MauiProgram.cs
- **Repository Pattern:** RecipeService abstracts data access from ViewModels
- **Observer Pattern:** ObservableProperty source generators for reactive UI binding

### Code Quality Principles
- **DRY (Don't Repeat Yourself):** HardwareHelper centralises haptic/vibration calls; MapNutrimentsToNutritionInfo reused by both search and barcode lookup; SetValidationError extracted from repeated validation pattern; AdjustFontSizeAsync handles both increase and decrease
- **KISS (Keep It Simple, Stupid):** Each method has a single responsibility; switch expressions for concise mapping; guard clauses for early returns
- **Naming Conventions:** PascalCase for public members, _camelCase for private fields, consistent throughout
- **Comments:** XML documentation on every public class, method, and property; inline comments explain non-obvious logic
- **Roslyn Analyser Fixes:** Code addresses CA1854, CA2000, CA2213, CA5394, IDE0028, IDE0031, IDE0042, IDE0130, SYSLIB1045 warnings with documented justifications

### Project Structure
FoodLens/
├── Helpers/
│ └── HardwareHelper.cs # Centralised hardware interaction (DRY)
├── Models/
│ ├── NutritionInfo.cs # Nutrition data model
│ ├── OpenFoodFactsModels.cs # API response DTOs
│ └── Recipe.cs # Recipe entity with validation
├── Services/
│ ├── NutritionApiService.cs # Open Food Facts API client (Networking)
│ └── RecipeService.cs # Recipe data repository
├── ViewModels/
│ ├── BaseViewModel.cs # Shared IsBusy/Title base class
│ ├── CameraViewModel.cs # Camera + CV + API networking
│ ├── CompassViewModel.cs # Magnetometer sensor reading
│ ├── RecipeDetailViewModel.cs # TTS, map, shopping list validation
│ ├── RecipesViewModel.cs # List, search, shake, swipe gestures
│ └── SettingsViewModel.cs # Theme, font size, high contrast
├── Views/
│ ├── CameraPage.xaml/.cs # Food scanner UI
│ ├── CompassPage.xaml/.cs # Compass UI with needle rotation
│ ├── HelpPage.xaml/.cs # User instructions (A11y requirement)
│ ├── MapPage.xaml/.cs # Geolocation + recipe origins
│ ├── RecipeDetailPage.xaml/.cs # Detail with pinch-to-zoom gesture
│ ├── RecipesPage.xaml/.cs # Main list with shake detection
│ └── SettingsPage.xaml/.cs # Accessibility settings
├── App.xaml/.cs # Theme engine + dynamic colours
├── AppShell.xaml/.cs # Tab navigation + route registration
└── MauiProgram.cs # DI container configuration

---

## Development Plan

The application was developed iteratively over the module duration:

| Phase | Features Implemented |
|-------|---------------------|
| **Phase 1 — Foundation** | Project setup, MVVM architecture, RecipeService, basic recipe list page with XAML |
| **Phase 2 — Core UI** | Recipe detail page, category filtering, search with debounce, navigation |
| **Phase 3 — Hardware** | Camera capture, accelerometer shake detection, haptic feedback, vibration |
| **Phase 4 — Advanced Hardware** | Compass/magnetometer, geolocation/GPS, text-to-speech |
| **Phase 5 — Networking** | Open Food Facts API integration, NutritionApiService, manual search |
| **Phase 6 — Computer Vision** | On-device colour histogram analysis for food classification |
| **Phase 7 — Accessibility** | Dark mode, high contrast, font scaling, SemanticProperties, screen reader announcements |
| **Phase 8 — Validation** | Shopping list validation, input guards, error handling throughout |
| **Phase 9 — Polish** | Swipe gestures, pinch-to-zoom, toast notifications, Help page, code quality fixes |
| **Phase 10 — Deployment** | Testing on Android emulator and Windows, cross-platform verification |

---

## Deployment

The application has been tested and deployed on:

| Platform | Device Type | Status |
|----------|-------------|--------|
| **Android** | Pixel 5 Emulator (API 33) | ✅ Fully functional |
| **Windows** | Windows 11 Desktop | ✅ Fully functional |

Both platforms support all features including camera (via emulator camera or webcam), compass (via Virtual Sensors on Android emulator), geolocation (spoofed on emulator), and networking (Open Food Facts API).

---

## How to Build and Run

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload installed
- .NET 8.0 SDK
- Android SDK (API 33+) for Android deployment
- Windows 10/11 for Windows deployment

### Steps
1. Clone the repository:
	git clone [repository-url]
2. Open `FoodLens.sln` in Visual Studio 2022.
3. Restore NuGet packages (automatic on build):
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
4. Select target platform (Android Emulator or Windows Machine).
5. Press F5 or click the Run button.

### Testing Hardware Features on Emulator
- **Camera:** The Android emulator provides a simulated camera environment.
- **Compass:** Open Extended Controls (⋯ button) → Virtual Sensors → rotate the 3D model or adjust Yaw.
- **Shake:** Open Extended Controls → Virtual Sensors → click the "Move" button rapidly.
- **Geolocation:** Open Extended Controls → Location → set coordinates manually.
- **Haptic/Vibration:** Logged to Debug output on emulators that don't support physical feedback.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.x | MVVM source generators, ObservableProperty, RelayCommand |
| CommunityToolkit.Maui | 7.x | Value converters (InvertedBoolConverter, IsStringNotNullOrEmptyConverter) |
| Microsoft.Extensions.Http | 8.x | IHttpClientFactory for NutritionApiService |

---

## API Reference

**Open Food Facts API** (free, no API key required)  
- Base URL: `https://world.openfoodfacts.org/api/v2`
- Documentation: https://world.openfoodfacts.org/data
- Used for: Real-time nutritional data retrieval by food name or barcode

---

## Screencast Checklist

The screencast demonstrates the following criteria:

- [x] UI/UX Design: Consistent styling, XAML layouts, smooth navigation, uncluttered design
- [x] Accessibility: Dark mode, high contrast, font scaling, screen reader support, WCAG references
- [x] Hardware (7 features): Camera, Compass, TTS, Shake, Geolocation, Haptic, Vibration
- [x] Computer Vision: On-device colour histogram analysis of captured photos
- [x] Networking: Live Open Food Facts API calls with visible data source indicator
- [x] Functionality: All buttons, gestures (swipe, pinch, shake), navigation, and features working
- [x] Validation: Shopping list input validation with 5 distinct checks and clear error messages
- [x] Error Handling: Network errors, permission errors, missing resources handled gracefully
- [x] Code Quality: Comments, naming conventions, DRY/KISS principles, Roslyn fixes
- [x] Deployment: Android emulator and Windows desktop
- [x] GitHub: Regular commits with descriptive messages showing development progress

---

## License

This project was developed for academic purposes as part of the 6G6Z0014 Mobile Computing module at Manchester Metropolitan University.# FoodLens

A cross-platform mobile application built with .NET MAUI for discovering, exploring, and interacting with food recipes from around the world. Developed as part of the 6G6Z0014 Mobile Computing module.

**Author:** [Your Name]  
**Student ID:** [Your Student ID]  
**Module:** 6G6Z0014 Mobile Computing  
**Framework:** .NET MAUI (.NET Multi-platform App UI)  
**Theme:** Food and Drink  

---

## Application Overview

FoodLens is a recipe discovery application that allows users to browse international recipes, scan food items using the device camera with on-device computer vision, fetch real nutritional data from the Open Food Facts REST API, navigate recipe origins on a map, and customise the app's appearance for accessibility. The app demonstrates extensive use of mobile hardware sensors, networking, MVVM architecture, and WCAG 2.1 accessibility compliance.

---

## Features

### Recipe Browsing
- Browse recipes by category (All, Breakfast, Dinner, Dessert, Drinks)
- Search recipes by name or description with debounced input (350ms delay to optimise performance)
- View full recipe details including ingredients, step-by-step method, and nutritional information
- Swipe right to favourite a recipe, swipe left to share
- Pull-to-refresh recipe list

### Food Scanner with Computer Vision and Networking
- Capture food photos using the device camera or pick from gallery
- On-device computer vision analyses the JPEG colour histogram (YCbCr hue bucket classification) to identify food categories
- Fetches real nutritional data from the Open Food Facts REST API (live HTTP GET requests with JSON deserialisation)
- Manual food name search sends user-initiated API requests and displays per-100g nutrition breakdown
- Displays data source indicator showing whether nutrition came from the API or local fallback

### Compass (Magnetometer)
- Real-time compass heading display using the device's magnetometer sensor
- Visual compass needle that rotates based on magnetic north reading
- Cardinal direction display (N, NE, E, SE, S, SW, W, NW)
- Instructions provided for testing on Android emulator via Extended Controls > Virtual Sensors

### Food Map and Geolocation
- View recipe origins from around the world (Naples, Sapporo, Sydney, Delhi, Paris, Bangkok, Tokyo)
- Get current GPS location using the device's geolocation hardware
- Open recipe origins in the device's native map application

### Shake to Discover
- Shake the device to get a random recipe suggestion (accelerometer hardware)
- Floating action button alternative for devices without shake support
- Vibration feedback confirms shake detection

### Settings and Accessibility
- Dark mode / Light mode / System theme switching
- High contrast mode exceeding WCAG AAA 7:1 contrast ratio
- Font size scaling from 75% to 200% (WCAG 1.4.4 Resize Text)
- All preferences persist across app sessions via Preferences API
- Reset all settings to defaults with confirmation dialog

---

## Hardware Features Used (7 Features)

| # | Hardware Feature | Location in App | Description |
|---|----------------|-----------------|-------------|
| 1 | **Camera** | Food Scanner page | Captures food photos via MediaPicker for identification |
| 2 | **Compass / Magnetometer** | Compass page | Real-time heading direction from magnetometer sensor |
| 3 | **Text-to-Speech** | Recipe Detail page | Reads recipe steps aloud for hands-free cooking |
| 4 | **Accelerometer (Shake)** | Recipes page | Shake detection triggers random recipe discovery |
| 5 | **Geolocation / GPS** | Map page | Gets user's current geographic coordinates |
| 6 | **Haptic Feedback** | Throughout app | Tactile confirmation on interactions via HardwareHelper |
| 7 | **Vibration** | Shake discover, Map page | Vibration pulses confirm actions (e.g., 400ms on shake) |

**Advanced Usage:** The camera feature includes on-device computer vision that analyses the colour histogram of captured JPEG images to classify food by dominant hue (red/green/yellow/brown/white), mapping to food categories. This satisfies the 86-100% requirement for advanced methods alongside mobile hardware.

---

## Networking

The application connects to the **Open Food Facts REST API** (https://world.openfoodfacts.org/api/v2) to fetch real nutritional data:

- **Search by name:** `GET /api/v2/search?categories_tags_en={food}&fields=product_name,nutriments&page_size=1`
- **Search by barcode:** `GET /api/v2/product/{barcode}.json`
- Uses `IHttpClientFactory` via `AddHttpClient<NutritionApiService>()` for proper connection lifetime management
- JSON deserialisation with `System.Text.Json` and strongly-typed response models
- Comprehensive error handling for network failures, timeouts, and malformed responses
- User-visible data source indicator shows whether data came from the API or local fallback

---

## Accessibility (WCAG 2.1 Compliance)

The application follows the Web Content Accessibility Guidelines (WCAG 2.1) at AA and AAA levels:

| WCAG Criterion | Implementation |
|---------------|----------------|
| 1.4.3 Contrast (Minimum) | All text meets 4.5:1 contrast ratio in both light and dark themes |
| 1.4.4 Resize Text | Font size adjustable from 75% to 200% without loss of functionality |
| 1.4.6 Contrast (Enhanced) | High contrast mode exceeds 7:1 ratio (WCAG AAA) |
| 1.4.11 Non-text Contrast | UI components (buttons, borders) maintain 3:1 contrast |
| 2.4.1 Bypass Blocks | Tab-based navigation allows direct access to any section |
| 2.4.2 Page Titled | Every page has a descriptive title via ViewModel Title property |
| 3.2.3 Consistent Navigation | Tab bar remains consistent across all pages |
| 3.3.1 Error Identification | Validation errors specify exactly what went wrong and how to fix it |
| 3.3.3 Error Suggestion | Error messages include corrective suggestions |
| 4.1.2 Name, Role, Value | All controls have SemanticProperties.Description and Hint |
| 4.1.3 Status Messages | SemanticScreenReader.Announce() called after every state change |

**Additional accessibility features:**
- Screen reader support via `SemanticProperties` on all interactive elements
- `SemanticScreenReader.Default.Announce()` for TalkBack/VoiceOver status messages
- Dark mode reduces eye strain in low-light environments
- Text-to-speech reads recipe steps aloud for hands-free or visually impaired users
- Clear user instructions available on the dedicated Help page

---

## Validation and Error Handling

Comprehensive validation is implemented throughout the application:

### Shopping List (RecipeDetailViewModel)
1. Empty input check — field cannot be blank
2. Non-integer check — rejects decimals, letters, symbols with specific error message
3. Zero/negative check — servings must be at least 1
4. Maximum check — rejects values above 20 with the user's actual input shown in the error
5. Missing ingredients check — handles recipes with no ingredient data

### Food Scanner (CameraViewModel)
- Camera availability validation before capture attempt
- Permission handling with user-friendly messages
- Network error handling with timeout detection
- Empty search term validation for manual nutrition lookup
- JSON parse error handling for unexpected API responses

### Nutrition API (NutritionApiService)
- `ArgumentException.ThrowIfNullOrWhiteSpace()` for null/empty inputs
- Barcode format validation via `[GeneratedRegex]` (8-14 digits only)
- HTTP status code validation before reading response body
- `HttpRequestException` catch for network connectivity issues
- `TaskCanceledException` catch for request timeouts
- `JsonException` catch for malformed API responses

### General
- All async methods wrapped in try-catch-finally blocks
- `IsBusy` guard prevents concurrent execution of commands
- `FeatureNotSupportedException` handled for all hardware features
- `PermissionException` handled with guidance to enable in device settings
- Error messages are clear, specific, and actionable (not generic "An error occurred")

---

## Architecture and Code Quality

### Design Patterns
- **MVVM (Model-View-ViewModel):** Strict separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection:** All services, ViewModels, and pages registered in MauiProgram.cs
- **Repository Pattern:** RecipeService abstracts data access from ViewModels
- **Observer Pattern:** ObservableProperty source generators for reactive UI binding

### Code Quality Principles
- **DRY (Don't Repeat Yourself):** HardwareHelper centralises haptic/vibration calls; MapNutrimentsToNutritionInfo reused by both search and barcode lookup; SetValidationError extracted from repeated validation pattern; AdjustFontSizeAsync handles both increase and decrease
- **KISS (Keep It Simple, Stupid):** Each method has a single responsibility; switch expressions for concise mapping; guard clauses for early returns
- **Naming Conventions:** PascalCase for public members, _camelCase for private fields, consistent throughout
- **Comments:** XML documentation on every public class, method, and property; inline comments explain non-obvious logic
- **Roslyn Analyser Fixes:** Code addresses CA1854, CA2000, CA2213, CA5394, IDE0028, IDE0031, IDE0042, IDE0130, SYSLIB1045 warnings with documented justifications

### Project Structure
FoodLens/
├── Helpers/
│ └── HardwareHelper.cs # Centralised hardware interaction (DRY)
├── Models/
│ ├── NutritionInfo.cs # Nutrition data model
│ ├── OpenFoodFactsModels.cs # API response DTOs
│ └── Recipe.cs # Recipe entity with validation
├── Services/
│ ├── NutritionApiService.cs # Open Food Facts API client (Networking)
│ └── RecipeService.cs # Recipe data repository
├── ViewModels/
│ ├── BaseViewModel.cs # Shared IsBusy/Title base class
│ ├── CameraViewModel.cs # Camera + CV + API networking
│ ├── CompassViewModel.cs # Magnetometer sensor reading
│ ├── RecipeDetailViewModel.cs # TTS, map, shopping list validation
│ ├── RecipesViewModel.cs # List, search, shake, swipe gestures
│ └── SettingsViewModel.cs # Theme, font size, high contrast
├── Views/
│ ├── CameraPage.xaml/.cs # Food scanner UI
│ ├── CompassPage.xaml/.cs # Compass UI with needle rotation
│ ├── HelpPage.xaml/.cs # User instructions (A11y requirement)
│ ├── MapPage.xaml/.cs # Geolocation + recipe origins
│ ├── RecipeDetailPage.xaml/.cs # Detail with pinch-to-zoom gesture
│ ├── RecipesPage.xaml/.cs # Main list with shake detection
│ └── SettingsPage.xaml/.cs # Accessibility settings
├── App.xaml/.cs # Theme engine + dynamic colours
├── AppShell.xaml/.cs # Tab navigation + route registration
└── MauiProgram.cs # DI container configuration

---

## Development Plan

The application was developed iteratively over the module duration:

| Phase | Features Implemented |
|-------|---------------------|
| **Phase 1 — Foundation** | Project setup, MVVM architecture, RecipeService, basic recipe list page with XAML |
| **Phase 2 — Core UI** | Recipe detail page, category filtering, search with debounce, navigation |
| **Phase 3 — Hardware** | Camera capture, accelerometer shake detection, haptic feedback, vibration |
| **Phase 4 — Advanced Hardware** | Compass/magnetometer, geolocation/GPS, text-to-speech |
| **Phase 5 — Networking** | Open Food Facts API integration, NutritionApiService, manual search |
| **Phase 6 — Computer Vision** | On-device colour histogram analysis for food classification |
| **Phase 7 — Accessibility** | Dark mode, high contrast, font scaling, SemanticProperties, screen reader announcements |
| **Phase 8 — Validation** | Shopping list validation, input guards, error handling throughout |
| **Phase 9 — Polish** | Swipe gestures, pinch-to-zoom, toast notifications, Help page, code quality fixes |
| **Phase 10 — Deployment** | Testing on Android emulator and Windows, cross-platform verification |

---

## Deployment

The application has been tested and deployed on:

| Platform | Device Type | Status |
|----------|-------------|--------|
| **Android** | Pixel 5 Emulator (API 33) | ✅ Fully functional |
| **Windows** | Windows 11 Desktop | ✅ Fully functional |

Both platforms support all features including camera (via emulator camera or webcam), compass (via Virtual Sensors on Android emulator), geolocation (spoofed on emulator), and networking (Open Food Facts API).

---

## How to Build and Run

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload installed
- .NET 8.0 SDK
- Android SDK (API 33+) for Android deployment
- Windows 10/11 for Windows deployment

### Steps
1. Clone the repository:
	git clone [repository-url]
2. Open `FoodLens.sln` in Visual Studio 2022.
3. Restore NuGet packages (automatic on build):
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
4. Select target platform (Android Emulator or Windows Machine).
5. Press F5 or click the Run button.

### Testing Hardware Features on Emulator
- **Camera:** The Android emulator provides a simulated camera environment.
- **Compass:** Open Extended Controls (⋯ button) → Virtual Sensors → rotate the 3D model or adjust Yaw.
- **Shake:** Open Extended Controls → Virtual Sensors → click the "Move" button rapidly.
- **Geolocation:** Open Extended Controls → Location → set coordinates manually.
- **Haptic/Vibration:** Logged to Debug output on emulators that don't support physical feedback.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.x | MVVM source generators, ObservableProperty, RelayCommand |
| CommunityToolkit.Maui | 7.x | Value converters (InvertedBoolConverter, IsStringNotNullOrEmptyConverter) |
| Microsoft.Extensions.Http | 8.x | IHttpClientFactory for NutritionApiService |

---

## API Reference

**Open Food Facts API** (free, no API key required)  
- Base URL: `https://world.openfoodfacts.org/api/v2`
- Documentation: https://world.openfoodfacts.org/data
- Used for: Real-time nutritional data retrieval by food name or barcode

---

## Screencast Checklist

The screencast demonstrates the following criteria:

- [x] UI/UX Design: Consistent styling, XAML layouts, smooth navigation, uncluttered design
- [x] Accessibility: Dark mode, high contrast, font scaling, screen reader support, WCAG references
- [x] Hardware (7 features): Camera, Compass, TTS, Shake, Geolocation, Haptic, Vibration
- [x] Computer Vision: On-device colour histogram analysis of captured photos
- [x] Networking: Live Open Food Facts API calls with visible data source indicator
- [x] Functionality: All buttons, gestures (swipe, pinch, shake), navigation, and features working
- [x] Validation: Shopping list input validation with 5 distinct checks and clear error messages
- [x] Error Handling: Network errors, permission errors, missing resources handled gracefully
- [x] Code Quality: Comments, naming conventions, DRY/KISS principles, Roslyn fixes
- [x] Deployment: Android emulator and Windows desktop
- [x] GitHub: Regular commits with descriptive messages showing development progress

---

## License

This project was developed for academic purposes as part of the 6G6Z0014 Mobile Computing module at Manchester Metropolitan University.# FoodLens

A cross-platform mobile application built with .NET MAUI for discovering, exploring, and interacting with food recipes from around the world. Developed as part of the 6G6Z0014 Mobile Computing module.

**Author:** [Your Name]  
**Student ID:** [Your Student ID]  
**Module:** 6G6Z0014 Mobile Computing  
**Framework:** .NET MAUI (.NET Multi-platform App UI)  
**Theme:** Food and Drink  

---

## Application Overview

FoodLens is a recipe discovery application that allows users to browse international recipes, scan food items using the device camera with on-device computer vision, fetch real nutritional data from the Open Food Facts REST API, navigate recipe origins on a map, and customise the app's appearance for accessibility. The app demonstrates extensive use of mobile hardware sensors, networking, MVVM architecture, and WCAG 2.1 accessibility compliance.

---

## Features

### Recipe Browsing
- Browse recipes by category (All, Breakfast, Dinner, Dessert, Drinks)
- Search recipes by name or description with debounced input (350ms delay to optimise performance)
- View full recipe details including ingredients, step-by-step method, and nutritional information
- Swipe right to favourite a recipe, swipe left to share
- Pull-to-refresh recipe list

### Food Scanner with Computer Vision and Networking
- Capture food photos using the device camera or pick from gallery
- On-device computer vision analyses the JPEG colour histogram (YCbCr hue bucket classification) to identify food categories
- Fetches real nutritional data from the Open Food Facts REST API (live HTTP GET requests with JSON deserialisation)
- Manual food name search sends user-initiated API requests and displays per-100g nutrition breakdown
- Displays data source indicator showing whether nutrition came from the API or local fallback

### Compass (Magnetometer)
- Real-time compass heading display using the device's magnetometer sensor
- Visual compass needle that rotates based on magnetic north reading
- Cardinal direction display (N, NE, E, SE, S, SW, W, NW)
- Instructions provided for testing on Android emulator via Extended Controls > Virtual Sensors

### Food Map and Geolocation
- View recipe origins from around the world (Naples, Sapporo, Sydney, Delhi, Paris, Bangkok, Tokyo)
- Get current GPS location using the device's geolocation hardware
- Open recipe origins in the device's native map application

### Shake to Discover
- Shake the device to get a random recipe suggestion (accelerometer hardware)
- Floating action button alternative for devices without shake support
- Vibration feedback confirms shake detection

### Settings and Accessibility
- Dark mode / Light mode / System theme switching
- High contrast mode exceeding WCAG AAA 7:1 contrast ratio
- Font size scaling from 75% to 200% (WCAG 1.4.4 Resize Text)
- All preferences persist across app sessions via Preferences API
- Reset all settings to defaults with confirmation dialog

---

## Hardware Features Used (7 Features)

| # | Hardware Feature | Location in App | Description |
|---|----------------|-----------------|-------------|
| 1 | **Camera** | Food Scanner page | Captures food photos via MediaPicker for identification |
| 2 | **Compass / Magnetometer** | Compass page | Real-time heading direction from magnetometer sensor |
| 3 | **Text-to-Speech** | Recipe Detail page | Reads recipe steps aloud for hands-free cooking |
| 4 | **Accelerometer (Shake)** | Recipes page | Shake detection triggers random recipe discovery |
| 5 | **Geolocation / GPS** | Map page | Gets user's current geographic coordinates |
| 6 | **Haptic Feedback** | Throughout app | Tactile confirmation on interactions via HardwareHelper |
| 7 | **Vibration** | Shake discover, Map page | Vibration pulses confirm actions (e.g., 400ms on shake) |

**Advanced Usage:** The camera feature includes on-device computer vision that analyses the colour histogram of captured JPEG images to classify food by dominant hue (red/green/yellow/brown/white), mapping to food categories. This satisfies the 86-100% requirement for advanced methods alongside mobile hardware.

---

## Networking

The application connects to the **Open Food Facts REST API** (https://world.openfoodfacts.org/api/v2) to fetch real nutritional data:

- **Search by name:** `GET /api/v2/search?categories_tags_en={food}&fields=product_name,nutriments&page_size=1`
- **Search by barcode:** `GET /api/v2/product/{barcode}.json`
- Uses `IHttpClientFactory` via `AddHttpClient<NutritionApiService>()` for proper connection lifetime management
- JSON deserialisation with `System.Text.Json` and strongly-typed response models
- Comprehensive error handling for network failures, timeouts, and malformed responses
- User-visible data source indicator shows whether data came from the API or local fallback

---

## Accessibility (WCAG 2.1 Compliance)

The application follows the Web Content Accessibility Guidelines (WCAG 2.1) at AA and AAA levels:

| WCAG Criterion | Implementation |
|---------------|----------------|
| 1.4.3 Contrast (Minimum) | All text meets 4.5:1 contrast ratio in both light and dark themes |
| 1.4.4 Resize Text | Font size adjustable from 75% to 200% without loss of functionality |
| 1.4.6 Contrast (Enhanced) | High contrast mode exceeds 7:1 ratio (WCAG AAA) |
| 1.4.11 Non-text Contrast | UI components (buttons, borders) maintain 3:1 contrast |
| 2.4.1 Bypass Blocks | Tab-based navigation allows direct access to any section |
| 2.4.2 Page Titled | Every page has a descriptive title via ViewModel Title property |
| 3.2.3 Consistent Navigation | Tab bar remains consistent across all pages |
| 3.3.1 Error Identification | Validation errors specify exactly what went wrong and how to fix it |
| 3.3.3 Error Suggestion | Error messages include corrective suggestions |
| 4.1.2 Name, Role, Value | All controls have SemanticProperties.Description and Hint |
| 4.1.3 Status Messages | SemanticScreenReader.Announce() called after every state change |

**Additional accessibility features:**
- Screen reader support via `SemanticProperties` on all interactive elements
- `SemanticScreenReader.Default.Announce()` for TalkBack/VoiceOver status messages
- Dark mode reduces eye strain in low-light environments
- Text-to-speech reads recipe steps aloud for hands-free or visually impaired users
- Clear user instructions available on the dedicated Help page

---

## Validation and Error Handling

Comprehensive validation is implemented throughout the application:

### Shopping List (RecipeDetailViewModel)
1. Empty input check — field cannot be blank
2. Non-integer check — rejects decimals, letters, symbols with specific error message
3. Zero/negative check — servings must be at least 1
4. Maximum check — rejects values above 20 with the user's actual input shown in the error
5. Missing ingredients check — handles recipes with no ingredient data

### Food Scanner (CameraViewModel)
- Camera availability validation before capture attempt
- Permission handling with user-friendly messages
- Network error handling with timeout detection
- Empty search term validation for manual nutrition lookup
- JSON parse error handling for unexpected API responses

### Nutrition API (NutritionApiService)
- `ArgumentException.ThrowIfNullOrWhiteSpace()` for null/empty inputs
- Barcode format validation via `[GeneratedRegex]` (8-14 digits only)
- HTTP status code validation before reading response body
- `HttpRequestException` catch for network connectivity issues
- `TaskCanceledException` catch for request timeouts
- `JsonException` catch for malformed API responses

### General
- All async methods wrapped in try-catch-finally blocks
- `IsBusy` guard prevents concurrent execution of commands
- `FeatureNotSupportedException` handled for all hardware features
- `PermissionException` handled with guidance to enable in device settings
- Error messages are clear, specific, and actionable (not generic "An error occurred")

---

## Architecture and Code Quality

### Design Patterns
- **MVVM (Model-View-ViewModel):** Strict separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection:** All services, ViewModels, and pages registered in MauiProgram.cs
- **Repository Pattern:** RecipeService abstracts data access from ViewModels
- **Observer Pattern:** ObservableProperty source generators for reactive UI binding

### Code Quality Principles
- **DRY (Don't Repeat Yourself):** HardwareHelper centralises haptic/vibration calls; MapNutrimentsToNutritionInfo reused by both search and barcode lookup; SetValidationError extracted from repeated validation pattern; AdjustFontSizeAsync handles both increase and decrease
- **KISS (Keep It Simple, Stupid):** Each method has a single responsibility; switch expressions for concise mapping; guard clauses for early returns
- **Naming Conventions:** PascalCase for public members, _camelCase for private fields, consistent throughout
- **Comments:** XML documentation on every public class, method, and property; inline comments explain non-obvious logic
- **Roslyn Analyser Fixes:** Code addresses CA1854, CA2000, CA2213, CA5394, IDE0028, IDE0031, IDE0042, IDE0130, SYSLIB1045 warnings with documented justifications

### Project Structure
FoodLens/
├── Helpers/
│ └── HardwareHelper.cs # Centralised hardware interaction (DRY)
├── Models/
│ ├── NutritionInfo.cs # Nutrition data model
│ ├── OpenFoodFactsModels.cs # API response DTOs
│ └── Recipe.cs # Recipe entity with validation
├── Services/
│ ├── NutritionApiService.cs # Open Food Facts API client (Networking)
│ └── RecipeService.cs # Recipe data repository
├── ViewModels/
│ ├── BaseViewModel.cs # Shared IsBusy/Title base class
│ ├── CameraViewModel.cs # Camera + CV + API networking
│ ├── CompassViewModel.cs # Magnetometer sensor reading
│ ├── RecipeDetailViewModel.cs # TTS, map, shopping list validation
│ ├── RecipesViewModel.cs # List, search, shake, swipe gestures
│ └── SettingsViewModel.cs # Theme, font size, high contrast
├── Views/
│ ├── CameraPage.xaml/.cs # Food scanner UI
│ ├── CompassPage.xaml/.cs # Compass UI with needle rotation
│ ├── HelpPage.xaml/.cs # User instructions (A11y requirement)
│ ├── MapPage.xaml/.cs # Geolocation + recipe origins
│ ├── RecipeDetailPage.xaml/.cs # Detail with pinch-to-zoom gesture
│ ├── RecipesPage.xaml/.cs # Main list with shake detection
│ └── SettingsPage.xaml/.cs # Accessibility settings
├── App.xaml/.cs # Theme engine + dynamic colours
├── AppShell.xaml/.cs # Tab navigation + route registration
└── MauiProgram.cs # DI container configuration

---

## Development Plan

The application was developed iteratively over the module duration:

| Phase | Features Implemented |
|-------|---------------------|
| **Phase 1 — Foundation** | Project setup, MVVM architecture, RecipeService, basic recipe list page with XAML |
| **Phase 2 — Core UI** | Recipe detail page, category filtering, search with debounce, navigation |
| **Phase 3 — Hardware** | Camera capture, accelerometer shake detection, haptic feedback, vibration |
| **Phase 4 — Advanced Hardware** | Compass/magnetometer, geolocation/GPS, text-to-speech |
| **Phase 5 — Networking** | Open Food Facts API integration, NutritionApiService, manual search |
| **Phase 6 — Computer Vision** | On-device colour histogram analysis for food classification |
| **Phase 7 — Accessibility** | Dark mode, high contrast, font scaling, SemanticProperties, screen reader announcements |
| **Phase 8 — Validation** | Shopping list validation, input guards, error handling throughout |
| **Phase 9 — Polish** | Swipe gestures, pinch-to-zoom, toast notifications, Help page, code quality fixes |
| **Phase 10 — Deployment** | Testing on Android emulator and Windows, cross-platform verification |

---

## Deployment

The application has been tested and deployed on:

| Platform | Device Type | Status |
|----------|-------------|--------|
| **Android** | Pixel 5 Emulator (API 33) | ✅ Fully functional |
| **Windows** | Windows 11 Desktop | ✅ Fully functional |

Both platforms support all features including camera (via emulator camera or webcam), compass (via Virtual Sensors on Android emulator), geolocation (spoofed on emulator), and networking (Open Food Facts API).

---

## How to Build and Run

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload installed
- .NET 8.0 SDK
- Android SDK (API 33+) for Android deployment
- Windows 10/11 for Windows deployment

### Steps
1. Clone the repository:
	git clone [repository-url]
2. Open `FoodLens.sln` in Visual Studio 2022.
3. Restore NuGet packages (automatic on build):
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
4. Select target platform (Android Emulator or Windows Machine).
5. Press F5 or click the Run button.

### Testing Hardware Features on Emulator
- **Camera:** The Android emulator provides a simulated camera environment.
- **Compass:** Open Extended Controls (⋯ button) → Virtual Sensors → rotate the 3D model or adjust Yaw.
- **Shake:** Open Extended Controls → Virtual Sensors → click the "Move" button rapidly.
- **Geolocation:** Open Extended Controls → Location → set coordinates manually.
- **Haptic/Vibration:** Logged to Debug output on emulators that don't support physical feedback.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.x | MVVM source generators, ObservableProperty, RelayCommand |
| CommunityToolkit.Maui | 7.x | Value converters (InvertedBoolConverter, IsStringNotNullOrEmptyConverter) |
| Microsoft.Extensions.Http | 8.x | IHttpClientFactory for NutritionApiService |

---

## API Reference

**Open Food Facts API** (free, no API key required)  
- Base URL: `https://world.openfoodfacts.org/api/v2`
- Documentation: https://world.openfoodfacts.org/data
- Used for: Real-time nutritional data retrieval by food name or barcode

---

## Screencast Checklist

The screencast demonstrates the following criteria:

- [x] UI/UX Design: Consistent styling, XAML layouts, smooth navigation, uncluttered design
- [x] Accessibility: Dark mode, high contrast, font scaling, screen reader support, WCAG references
- [x] Hardware (7 features): Camera, Compass, TTS, Shake, Geolocation, Haptic, Vibration
- [x] Computer Vision: On-device colour histogram analysis of captured photos
- [x] Networking: Live Open Food Facts API calls with visible data source indicator
- [x] Functionality: All buttons, gestures (swipe, pinch, shake), navigation, and features working
- [x] Validation: Shopping list input validation with 5 distinct checks and clear error messages
- [x] Error Handling: Network errors, permission errors, missing resources handled gracefully
- [x] Code Quality: Comments, naming conventions, DRY/KISS principles, Roslyn fixes
- [x] Deployment: Android emulator and Windows desktop
- [x] GitHub: Regular commits with descriptive messages showing development progress

---

## License

This project was developed for academic purposes as part of the 6G6Z0014 Mobile Computing module at Manchester Metropolitan University.
## Application Overview

FoodLens is a recipe discovery application that allows users to browse international recipes, scan food items using the device camera with on-device computer vision, fetch real nutritional data from the Open Food Facts REST API, navigate recipe origins on a map, and customise the app's appearance for accessibility. The app demonstrates extensive use of mobile hardware sensors, networking, MVVM architecture, and WCAG 2.1 accessibility compliance.

---

## Features

### Recipe Browsing
- Browse recipes by category (All, Breakfast, Dinner, Dessert, Drinks)
- Search recipes by name or description with debounced input (350ms delay to optimise performance)
- View full recipe details including ingredients, step-by-step method, and nutritional information
- Swipe right to favourite a recipe, swipe left to share
- Pull-to-refresh recipe list

### Food Scanner with Computer Vision and Networking
- Capture food photos using the device camera or pick from gallery
- On-device computer vision analyses the JPEG colour histogram (YCbCr hue bucket classification) to identify food categories
- Fetches real nutritional data from the Open Food Facts REST API (live HTTP GET requests with JSON deserialisation)
- Manual food name search sends user-initiated API requests and displays per-100g nutrition breakdown
- Displays data source indicator showing whether nutrition came from the API or local fallback

### Compass (Magnetometer)
- Real-time compass heading display using the device's magnetometer sensor
- Visual compass needle that rotates based on magnetic north reading
- Cardinal direction display (N, NE, E, SE, S, SW, W, NW)
- Instructions provided for testing on Android emulator via Extended Controls > Virtual Sensors

### Food Map and Geolocation
- View recipe origins from around the world (Naples, Sapporo, Sydney, Delhi, Paris, Bangkok, Tokyo)
- Get current GPS location using the device's geolocation hardware
- Open recipe origins in the device's native map application

### Shake to Discover
- Shake the device to get a random recipe suggestion (accelerometer hardware)
- Floating action button alternative for devices without shake support
- Vibration feedback confirms shake detection

### Settings and Accessibility
- Dark mode / Light mode / System theme switching
- High contrast mode exceeding WCAG AAA 7:1 contrast ratio
- Font size scaling from 75% to 200% (WCAG 1.4.4 Resize Text)
- All preferences persist across app sessions via Preferences API
- Reset all settings to defaults with confirmation dialog

---

## Hardware Features Used (7 Features)

| # | Hardware Feature | Location in App | Description |
|---|----------------|-----------------|-------------|
| 1 | **Camera** | Food Scanner page | Captures food photos via MediaPicker for identification |
| 2 | **Compass / Magnetometer** | Compass page | Real-time heading direction from magnetometer sensor |
| 3 | **Text-to-Speech** | Recipe Detail page | Reads recipe steps aloud for hands-free cooking |
| 4 | **Accelerometer (Shake)** | Recipes page | Shake detection triggers random recipe discovery |
| 5 | **Geolocation / GPS** | Map page | Gets user's current geographic coordinates |
| 6 | **Haptic Feedback** | Throughout app | Tactile confirmation on interactions via HardwareHelper |
| 7 | **Vibration** | Shake discover, Map page | Vibration pulses confirm actions (e.g., 400ms on shake) |

**Advanced Usage:** The camera feature includes on-device computer vision that analyses the colour histogram of captured JPEG images to classify food by dominant hue (red/green/yellow/brown/white), mapping to food categories. This satisfies the 86-100% requirement for advanced methods alongside mobile hardware.

---

## Networking

The application connects to the **Open Food Facts REST API** (https://world.openfoodfacts.org/api/v2) to fetch real nutritional data:

- **Search by name:** `GET /api/v2/search?categories_tags_en={food}&fields=product_name,nutriments&page_size=1`
- **Search by barcode:** `GET /api/v2/product/{barcode}.json`
- Uses `IHttpClientFactory` via `AddHttpClient<NutritionApiService>()` for proper connection lifetime management
- JSON deserialisation with `System.Text.Json` and strongly-typed response models
- Comprehensive error handling for network failures, timeouts, and malformed responses
- User-visible data source indicator shows whether data came from the API or local fallback

---

## Accessibility (WCAG 2.1 Compliance)

The application follows the Web Content Accessibility Guidelines (WCAG 2.1) at AA and AAA levels:

| WCAG Criterion | Implementation |
|---------------|----------------|
| 1.4.3 Contrast (Minimum) | All text meets 4.5:1 contrast ratio in both light and dark themes |
| 1.4.4 Resize Text | Font size adjustable from 75% to 200% without loss of functionality |
| 1.4.6 Contrast (Enhanced) | High contrast mode exceeds 7:1 ratio (WCAG AAA) |
| 1.4.11 Non-text Contrast | UI components (buttons, borders) maintain 3:1 contrast |
| 2.4.1 Bypass Blocks | Tab-based navigation allows direct access to any section |
| 2.4.2 Page Titled | Every page has a descriptive title via ViewModel Title property |
| 3.2.3 Consistent Navigation | Tab bar remains consistent across all pages |
| 3.3.1 Error Identification | Validation errors specify exactly what went wrong and how to fix it |
| 3.3.3 Error Suggestion | Error messages include corrective suggestions |
| 4.1.2 Name, Role, Value | All controls have SemanticProperties.Description and Hint |
| 4.1.3 Status Messages | SemanticScreenReader.Announce() called after every state change |

**Additional accessibility features:**
- Screen reader support via `SemanticProperties` on all interactive elements
- `SemanticScreenReader.Default.Announce()` for TalkBack/VoiceOver status messages
- Dark mode reduces eye strain in low-light environments
- Text-to-speech reads recipe steps aloud for hands-free or visually impaired users
- Clear user instructions available on the dedicated Help page

---

## Validation and Error Handling

Comprehensive validation is implemented throughout the application:

### Shopping List (RecipeDetailViewModel)
1. Empty input check — field cannot be blank
2. Non-integer check — rejects decimals, letters, symbols with specific error message
3. Zero/negative check — servings must be at least 1
4. Maximum check — rejects values above 20 with the user's actual input shown in the error
5. Missing ingredients check — handles recipes with no ingredient data

### Food Scanner (CameraViewModel)
- Camera availability validation before capture attempt
- Permission handling with user-friendly messages
- Network error handling with timeout detection
- Empty search term validation for manual nutrition lookup
- JSON parse error handling for unexpected API responses

### Nutrition API (NutritionApiService)
- `ArgumentException.ThrowIfNullOrWhiteSpace()` for null/empty inputs
- Barcode format validation via `[GeneratedRegex]` (8-14 digits only)
- HTTP status code validation before reading response body
- `HttpRequestException` catch for network connectivity issues
- `TaskCanceledException` catch for request timeouts
- `JsonException` catch for malformed API responses

### General
- All async methods wrapped in try-catch-finally blocks
- `IsBusy` guard prevents concurrent execution of commands
- `FeatureNotSupportedException` handled for all hardware features
- `PermissionException` handled with guidance to enable in device settings
- Error messages are clear, specific, and actionable (not generic "An error occurred")

---

## Architecture and Code Quality

### Design Patterns
- **MVVM (Model-View-ViewModel):** Strict separation of concerns using CommunityToolkit.Mvvm
- **Dependency Injection:** All services, ViewModels, and pages registered in MauiProgram.cs
- **Repository Pattern:** RecipeService abstracts data access from ViewModels
- **Observer Pattern:** ObservableProperty source generators for reactive UI binding

### Code Quality Principles
- **DRY (Don't Repeat Yourself):** HardwareHelper centralises haptic/vibration calls; MapNutrimentsToNutritionInfo reused by both search and barcode lookup; SetValidationError extracted from repeated validation pattern; AdjustFontSizeAsync handles both increase and decrease
- **KISS (Keep It Simple, Stupid):** Each method has a single responsibility; switch expressions for concise mapping; guard clauses for early returns
- **Naming Conventions:** PascalCase for public members, _camelCase for private fields, consistent throughout
- **Comments:** XML documentation on every public class, method, and property; inline comments explain non-obvious logic
- **Roslyn Analyser Fixes:** Code addresses CA1854, CA2000, CA2213, CA5394, IDE0028, IDE0031, IDE0042, IDE0130, SYSLIB1045 warnings with documented justifications

### Project Structure
FoodLens/
├── Helpers/
│ └── HardwareHelper.cs # Centralised hardware interaction (DRY)
├── Models/
│ ├── NutritionInfo.cs # Nutrition data model
│ ├── OpenFoodFactsModels.cs # API response DTOs
│ └── Recipe.cs # Recipe entity with validation
├── Services/
│ ├── NutritionApiService.cs # Open Food Facts API client (Networking)
│ └── RecipeService.cs # Recipe data repository
├── ViewModels/
│ ├── BaseViewModel.cs # Shared IsBusy/Title base class
│ ├── CameraViewModel.cs # Camera + CV + API networking
│ ├── CompassViewModel.cs # Magnetometer sensor reading
│ ├── RecipeDetailViewModel.cs # TTS, map, shopping list validation
│ ├── RecipesViewModel.cs # List, search, shake, swipe gestures
│ └── SettingsViewModel.cs # Theme, font size, high contrast
├── Views/
│ ├── CameraPage.xaml/.cs # Food scanner UI
│ ├── CompassPage.xaml/.cs # Compass UI with needle rotation
│ ├── HelpPage.xaml/.cs # User instructions (A11y requirement)
│ ├── MapPage.xaml/.cs # Geolocation + recipe origins
│ ├── RecipeDetailPage.xaml/.cs # Detail with pinch-to-zoom gesture
│ ├── RecipesPage.xaml/.cs # Main list with shake detection
│ └── SettingsPage.xaml/.cs # Accessibility settings
├── App.xaml/.cs # Theme engine + dynamic colours
├── AppShell.xaml/.cs # Tab navigation + route registration
└── MauiProgram.cs # DI container configuration

---

## Development Plan

The application was developed iteratively over the module duration:

| Phase | Features Implemented |
|-------|---------------------|
| **Phase 1 — Foundation** | Project setup, MVVM architecture, RecipeService, basic recipe list page with XAML |
| **Phase 2 — Core UI** | Recipe detail page, category filtering, search with debounce, navigation |
| **Phase 3 — Hardware** | Camera capture, accelerometer shake detection, haptic feedback, vibration |
| **Phase 4 — Advanced Hardware** | Compass/magnetometer, geolocation/GPS, text-to-speech |
| **Phase 5 — Networking** | Open Food Facts API integration, NutritionApiService, manual search |
| **Phase 6 — Computer Vision** | On-device colour histogram analysis for food classification |
| **Phase 7 — Accessibility** | Dark mode, high contrast, font scaling, SemanticProperties, screen reader announcements |
| **Phase 8 — Validation** | Shopping list validation, input guards, error handling throughout |
| **Phase 9 — Polish** | Swipe gestures, pinch-to-zoom, toast notifications, Help page, code quality fixes |
| **Phase 10 — Deployment** | Testing on Android emulator and Windows, cross-platform verification |

---

## Deployment

The application has been tested and deployed on:

| Platform | Device Type | Status |
|----------|-------------|--------|
| **Android** | Pixel 5 Emulator (API 33) | ✅ Fully functional |
| **Windows** | Windows 11 Desktop | ✅ Fully functional |

Both platforms support all features including camera (via emulator camera or webcam), compass (via Virtual Sensors on Android emulator), geolocation (spoofed on emulator), and networking (Open Food Facts API).

---

## How to Build and Run

### Prerequisites
- Visual Studio 2022 (17.8+) with .NET MAUI workload installed
- .NET 8.0 SDK
- Android SDK (API 33+) for Android deployment
- Windows 10/11 for Windows deployment

### Steps
1. Clone the repository:
	git clone [repository-url]
2. Open `FoodLens.sln` in Visual Studio 2022.
3. Restore NuGet packages (automatic on build):
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui
4. Select target platform (Android Emulator or Windows Machine).
5. Press F5 or click the Run button.

### Testing Hardware Features on Emulator
- **Camera:** The Android emulator provides a simulated camera environment.
- **Compass:** Open Extended Controls (⋯ button) → Virtual Sensors → rotate the 3D model or adjust Yaw.
- **Shake:** Open Extended Controls → Virtual Sensors → click the "Move" button rapidly.
- **Geolocation:** Open Extended Controls → Location → set coordinates manually.
- **Haptic/Vibration:** Logged to Debug output on emulators that don't support physical feedback.

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.x | MVVM source generators, ObservableProperty, RelayCommand |
| CommunityToolkit.Maui | 7.x | Value converters (InvertedBoolConverter, IsStringNotNullOrEmptyConverter) |
| Microsoft.Extensions.Http | 8.x | IHttpClientFactory for NutritionApiService |

---

## API Reference

**Open Food Facts API** (free, no API key required)  
- Base URL: `https://world.openfoodfacts.org/api/v2`
- Documentation: https://world.openfoodfacts.org/data
- Used for: Real-time nutritional data retrieval by food name or barcode

---

## Screencast Checklist

The screencast demonstrates the following criteria:

- [x] UI/UX Design: Consistent styling, XAML layouts, smooth navigation, uncluttered design
- [x] Accessibility: Dark mode, high contrast, font scaling, screen reader support, WCAG references
- [x] Hardware (7 features): Camera, Compass, TTS, Shake, Geolocation, Haptic, Vibration
- [x] Computer Vision: On-device colour histogram analysis of captured photos
- [x] Networking: Live Open Food Facts API calls with visible data source indicator
- [x] Functionality: All buttons, gestures (swipe, pinch, shake), navigation, and features working
- [x] Validation: Shopping list input validation with 5 distinct checks and clear error messages
- [x] Error Handling: Network errors, permission errors, missing resources handled gracefully
- [x] Code Quality: Comments, naming conventions, DRY/KISS principles, Roslyn fixes
- [x] Deployment: Android emulator and Windows desktop
- [x] GitHub: Regular commits with descriptive messages showing development progress

---

## License

This project was developed for academic purposes as part of the 6G6Z0014 Mobile Computing module at Manchester Metropolitan University.