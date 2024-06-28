# CustomAutoAdapterMapper
In Organizations that synchronize information from different systems supplied by specified endpoints, mapping unknown types in real-time is a pain, considering C# is "strongly typed." Creating contracts for every third-party system to be implemented is also challenging, as development work is needed each time. 

Additionally, most properties or fields supplied might not match the expected properties or fields of the known type. Hence, custom mapping needs to be established.

This library solves the problems of mapping a JSON string to a known type.

# Usage

Example endpoint: https://api.publicapis.org/entries

```JSON
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
            "Category": "Animals",
            "Parent": {
              "SomeProperties": "ABC",
              "SubParent": {
                "SubParentProperty": "123"
              }
            }
        },
        {
            "API": "Axolotl",
            "Description": "Collection of axolotl pictures and facts",
            "Auth": "",
            "HTTPS": true,
            "Cors": "no",
            "Link": "https://theaxolotlapi.netlify.app/",
            "Category": "Animals",
            "Parent": {
              "SomeProperties": "DEF",
              "SubParent": {
                "SubParentProperty": "456"
              }
            }
        }
    ]
}
```

```C#
var result = JSON_STRING_FROM_ENDPOINT;
var destinationCollection = new List<T>();
result.MapCollection(destinationCollection.Entries, options =>
{
    options.RootKey = "entries";
    options.ItemKey = "API";
    options.Mappings = new Dictionary<string, string>
    {
        { "DescriptionVariation", "Description" },
        { "AuthVariation", "Auth" },
        { "PropertyOne", "Parent.SomeProperties" },
        { "PropertyTwo", "Parent.SubParent.SubParentProperty" },
    };
});
```

The dictionary In the example above maps the value of `Description` in `TestObject` to `DescriptionVariation` in `TestObjectWithVariation`

```C#
public class TestObjectWithVariation
{
    public string API { get; set; }
    public string DescriptionVariation { get; set; }
    public string AuthVariation { get; set; }
    public string HTTPS { get; set; }
    public string Cors { get; set; }
    public string Link { get; set; }
    public string CategoryVariation { get; set; }
    public string PropertyOne { get; set; }
    public string PropertyTwo { get; set; }
}
```

```C#
public class TestObject
{
    public string API { get; set; }
    public string Description { get; set; }
    public string Auth { get; set; }
    public string HTTPS { get; set; }
    public string Cors { get; set; }
    public string Link { get; set; }
    public string Category { get; set; }
}
```

# Options
Value  | Description
------------- | -------------
RootKey | Specifies what Property holds the collection that needs to be mapped
Mappings  | Specified the dictionary that holds the custom property/field mappings needed
ItemKey  | Used to Identify the Unique Identifier of an item in the collection

