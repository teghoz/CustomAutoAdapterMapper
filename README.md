# CustomAutoAdapterMapper

[![NuGet](https://img.shields.io/nuget/v/CustomAutoAdapterMapper.svg)](https://www.nuget.org/packages/CustomAutoAdapterMapper/)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

A lightweight, flexible JSON-to-object mapper for C# that handles third-party API responses with mismatched property names and nested structures without requiring contract definitions.

## 🎯 Problem Statement

In organizations that integrate with multiple external systems:

- **Strongly-typed languages** like C# make it difficult to map unknown or dynamic JSON structures at runtime
- **Creating contracts** for every third-party API is time-consuming and requires development work for each new integration
- **Property mismatches** between external APIs and internal models require custom mapping logic
- **Nested properties** in JSON need to be flattened or mapped to different structures

**CustomAutoAdapterMapper** solves these challenges by providing a flexible, configuration-driven approach to mapping JSON strings to strongly-typed C# objects.

---

## 📦 Installation

```bash
dotnet add package CustomAutoAdapterMapper
```

Or via NuGet Package Manager:

```bash
Install-Package CustomAutoAdapterMapper
```

---

## 🚀 Quick Start

### Basic Usage: Direct Property Mapping

When JSON property names match your C# class properties:

```csharp
using CustomAutoAdapterMapper;

var jsonResponse = await httpClient.GetStringAsync("https://api.example.com/data");
var destinationCollection = new List<MyClass>();

var result = jsonResponse.MapCollection(destinationCollection, options =>
{
    options.RootKey = "entries"; // JSON property containing the array
});
```

### Custom Property Mapping

When JSON property names differ from your C# class properties:

```csharp
var result = jsonResponse.MapCollection(destinationCollection, options =>
{
    options.RootKey = "entries";
    options.Mappings = new Dictionary<string, string>
    {
        { "MyProperty", "TheirProperty" },        // Map TheirProperty -> MyProperty
        { "Description", "desc" },                // Map desc -> Description
        { "AuthType", "authentication_type" }     // Map authentication_type -> AuthType
    };
});
```

---

## 📖 Comprehensive Examples

### Example 1: Simple Mapping with Variations

**JSON Response** from `https://api.publicapis.org/entries`:

```json
{
    "count": 1427,
    "entries": [
        {
            "API": "AdoptAPet",
            "Description": "Resource to help get pets adopted",
            "Auth": "apiKey",
            "HTTPS": true,
            "Cors": "yes",
            "Link": "https://www.adoptapet.com/public/apis/pet_list.html",
            "Category": "Animals"
        },
        {
            "API": "Axolotl",
            "Description": "Collection of axolotl pictures and facts",
            "Auth": "",
            "HTTPS": true,
            "Cors": "no",
            "Link": "https://theaxolotlapi.netlify.app/",
            "Category": "Animals"
        }
    ]
}
```

**Your C# Model** (with different property names):

```csharp
public class ApiEntry
{
    public string API { get; set; }
    public string DescriptionText { get; set; }  // Different name
    public string AuthType { get; set; }         // Different name
    public bool HTTPS { get; set; }
    public string Cors { get; set; }
    public string Link { get; set; }
    public string CategoryName { get; set; }     // Different name
}
```

**Mapping Code**:

```csharp
var destinationCollection = new List<ApiEntry>();
var result = jsonResponse.MapCollection(destinationCollection, options =>
{
    options.RootKey = "entries";
    options.Mappings = new Dictionary<string, string>
    {
        { "DescriptionText", "Description" },
        { "AuthType", "Auth" },
        { "CategoryName", "Category" }
    };
});
```

### Example 2: Nested Property Mapping (Dot Notation)

Map deeply nested JSON properties to flat C# properties using **dot notation**.

**JSON Response**:

```json
{
    "entries": [
        {
            "API": "AdoptAPet",
            "Description": "Resource to help get pets adopted",
            "work": {
                "reportsToIdInCompany": 64,
                "employeeIdInCompany": 140,
                "reportsTo": {
                    "email": "manager@company.com",
                    "name": "John Doe"
                }
            }
        }
    ]
}
```

**Your C# Model** (flattened structure):

```csharp
public class Employee
{
    public string API { get; set; }
    public string Description { get; set; }
    public int ManagerId { get; set; }
    public int EmployeeId { get; set; }
    public string ManagerEmail { get; set; }
    public string ManagerName { get; set; }
}
```

**Mapping Code**:

```csharp
var employees = new List<Employee>();
var result = jsonResponse.MapCollection(employees, options =>
{
    options.RootKey = "entries";
    options.Mappings = new Dictionary<string, string>
    {
        { "ManagerId", "work.reportsToIdInCompany" },      // Nested property
        { "EmployeeId", "work.employeeIdInCompany" },      // Nested property
        { "ManagerEmail", "work.reportsTo.email" },        // Deeply nested
        { "ManagerName", "work.reportsTo.name" }           // Deeply nested
    };
});
```

### Example 3: Updating Existing Collections

Use `ItemKey` to update an existing collection instead of creating a new one.

**Scenario**: You have a pre-populated list and want to update specific items based on a unique identifier.

```csharp
// Pre-populated collection
var existingApis = new List<ApiEntry>
{
    new ApiEntry { API = "AdoptAPet", DescriptionText = "Old description" },
    new ApiEntry { API = "Axolotl", DescriptionText = "Old description" }
};

// Update the collection with fresh data from the API
var result = jsonResponse.MapCollection(existingApis, options =>
{
    options.RootKey = "entries";
    options.ItemKey = "API";  // Match items by the "API" property
    options.Mappings = new Dictionary<string, string>
    {
        { "DescriptionText", "Description" },
        { "AuthType", "Auth" }
    };
});

// Only mapped properties are updated; other properties remain unchanged
```

---

## ⚙️ Configuration Options

### `Option` Class Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| **`RootKey`** | `string` | ✅ Yes | The JSON property name that contains the array/collection to map. |
| **`Mappings`** | `Dictionary<string, string>` | ⚠️ Optional | Custom property mappings. **Key** = your C# property name, **Value** = JSON property path (supports dot notation for nested properties). |
| **`ItemKey`** | `string` | ⚠️ Conditional | Unique identifier property name. **Required** when updating an existing non-empty collection. Used to match items between JSON and your collection. |

### Configuration Details

#### `RootKey`
- Identifies which JSON property contains the array of items to map
- Must be a valid property in the root JSON object
- **Throws `RootKeyOptionNullException`** if not provided
- **Throws `RootKeyPropertyNullException`** if the property doesn't exist in the JSON

#### `Mappings`
- Optional dictionary for custom property mappings
- **Key**: Your C# class property name
- **Value**: JSON property path (supports nested properties with dot notation)
- If not provided, the mapper attempts direct property name matching

**Examples**:
```csharp
options.Mappings = new Dictionary<string, string>
{
    { "MyProperty", "their_property" },              // Simple mapping
    { "Email", "user.contact.email" },              // Nested property
    { "ManagerId", "employee.reports_to.id" }       // Deeply nested
};
```

#### `ItemKey`
- Specifies a unique identifier property for matching items
- **Required when**:
  - Updating an existing collection (non-empty `List<T>`)
  - You want to preserve existing items and only update mapped properties
- **Not required when**:
  - Creating a new collection from scratch (empty or null list)
- **Throws `ItemKeyOptionNullException`** if required but not provided

---

## 🔍 How It Works

### Mapping Behavior

The mapper operates in two modes:

#### 1. **Create Mode** (Empty/Null Collection)
When you pass an empty or null collection:
- Creates new instances of your type `T`
- Maps all matching properties automatically
- Applies custom mappings from `options.Mappings`
- Adds items to your collection

```csharp
var newCollection = new List<MyClass>();  // Empty collection
jsonResponse.MapCollection(newCollection, options => {
    options.RootKey = "data";
    // ItemKey not required
});
```

#### 2. **Update Mode** (Existing Collection)
When you pass a non-empty collection:
- Matches items using `ItemKey`
- Only updates properties defined in `options.Mappings`
- Preserves all other properties in existing items
- Does not add new items

```csharp
var existingCollection = GetExistingData();  // Non-empty collection
jsonResponse.MapCollection(existingCollection, options => {
    options.RootKey = "data";
    options.ItemKey = "Id";  // Required!
    options.Mappings = new Dictionary<string, string> { /* ... */ };
});
```

### Type Conversion

- The mapper uses **`Newtonsoft.Json`** for type conversion
- Automatically converts JSON types to C# property types
- Supports:
  - Primitives (`string`, `int`, `bool`, `decimal`, etc.)
  - Nullable types (`int?`, `DateTime?`, etc.)
  - Complex types (nested objects)
  - Collections and arrays

---

## ⚠️ Exception Handling

The library throws custom exceptions for common configuration errors:

| Exception | When Thrown | Solution |
|-----------|-------------|----------|
| **`JsonContentException`** | The provided string is not valid JSON | Ensure the input string is valid JSON |
| **`RootKeyOptionNullException`** | `RootKey` is not provided in options | Set `options.RootKey` to the JSON array property name |
| **`RootKeyPropertyNullException`** | `RootKey` doesn't exist in the JSON object | Verify the JSON structure and `RootKey` value |
| **`ItemKeyOptionNullException`** | `ItemKey` is required but not provided (when updating existing collections) | Set `options.ItemKey` to a unique identifier property |
| **`JsonReaderException`** | JSON cannot be parsed as an object (e.g., it's a raw array) | Ensure JSON is an object with a root property containing the array |

### Error Handling Example

```csharp
try
{
    var result = jsonResponse.MapCollection(collection, options =>
    {
        options.RootKey = "entries";
    });
}
catch (JsonContentException ex)
{
    // Invalid JSON string
    Console.WriteLine($"Invalid JSON: {ex.Message}");
}
catch (RootKeyPropertyNullException ex)
{
    // RootKey doesn't exist in JSON
    Console.WriteLine($"Property not found: {ex.Message}");
}
catch (ItemKeyOptionNullException ex)
{
    // ItemKey required but not provided
    Console.WriteLine($"Missing ItemKey: {ex.Message}");
}
```

---

## 🎓 Advanced Usage

### Complex Nested Structures

You can map multiple levels of nesting:

```csharp
options.Mappings = new Dictionary<string, string>
{
    { "Street", "address.street" },
    { "City", "address.city" },
    { "ZipCode", "address.location.zipCode" },
    { "Country", "address.location.country.name" },
    { "CountryCode", "address.location.country.code" }
};
```

### Combining Direct and Custom Mappings

Properties not in `Mappings` are mapped directly by name:

```csharp
public class Product
{
    public string Id { get; set; }           // Mapped directly from JSON "Id"
    public string Name { get; set; }         // Mapped directly from JSON "Name"
    public decimal Cost { get; set; }        // Custom mapping required
}

options.Mappings = new Dictionary<string, string>
{
    { "Cost", "pricing.unitPrice" }  // Only Cost needs custom mapping
};
// Id and Name are automatically mapped if they exist in the JSON
```

### Type Safety with Nullability

The mapper handles null values gracefully:

```csharp
public class SafeModel
{
    public string Required { get; set; }     // Will be null if not in JSON
    public int? OptionalNumber { get; set; } // Nullable type
    public DateTime? OptionalDate { get; set; }
}
```

---

## 🧪 Testing

The library includes comprehensive unit tests covering:

- ✅ Basic property mapping
- ✅ Custom property mappings
- ✅ Nested property mapping with dot notation
- ✅ Collection creation (empty destination)
- ✅ Collection updates (existing destination with ItemKey)
- ✅ Exception scenarios
- ✅ Type conversions

Run tests:

```bash
dotnet test
```

---

## 🛠️ Technical Details

- **Target Framework**: .NET Standard 2.0
- **Dependencies**: Newtonsoft.Json (>= 13.0.3)
- **Namespace**: `CustomAutoAdapterMapper`
- **Primary Method**: `MapCollection<T>` (extension method on `string`)

---

## 📝 Best Practices

1. **Always set `RootKey`** - It's required and identifies your data array
2. **Use `ItemKey` for updates** - When updating existing collections, always specify a unique identifier
3. **Leverage dot notation** - For nested properties, use `"parent.child.property"` syntax
4. **Handle exceptions** - Wrap mapping calls in try-catch for production code
5. **Validate JSON first** - Ensure external API responses are valid before mapping
6. **Use nullable types** - For optional properties, use nullable types (`int?`, `DateTime?`, etc.)

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

## 📄 License

See [license.txt](license.txt) for details.

---

## 🔗 Links

- **NuGet Package**: https://www.nuget.org/packages/CustomAutoAdapterMapper/
- **GitHub Repository**: https://github.com/teghoz/CustomAutoAdapterMapper

---

## 📧 Support

For issues, questions, or feature requests, please open an issue on GitHub

