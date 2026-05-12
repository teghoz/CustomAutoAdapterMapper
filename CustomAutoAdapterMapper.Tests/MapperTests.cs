using AutoFixture;
using CustomAutoAdapterMapper.Exceptions;
using Newtonsoft.Json;

namespace CustomAutoAdapterMapper.Tests;

public class MapperTests
{
    private string _result;

    [SetUp]
    public void Setup()
    {
        try
        {
            var client = new HttpClient();
            var result = client.GetAsync("https://dummyjson.com/products?limit=5").Result;
            _result = result.Content.ReadAsStringAsync().Result;
        }
        catch
        {
            // network unavailable (e.g. CI), fall through to static fixture
        }

        if (string.IsNullOrEmpty(_result)) _result = Fallback();
    }

    private string Fallback()
    {
        return @"
            {
                'total': 2,
                'products': [
                    {
                        'id': 1,
                        'title': 'Essence Mascara Lash Princess',
                        'description': 'A popular mascara known for volumizing effects.',
                        'brand': 'Essence',
                        'category': 'beauty',
                        'price': 9.99,
                        'sku': 'RCH45Q1A'
                    },
                    {
                        'id': 2,
                        'title': 'Eyeshadow Palette with Mirror',
                        'description': 'An eyeshadow palette for all occasions.',
                        'brand': 'Glamour Beauty',
                        'category': 'beauty',
                        'price': 19.99,
                        'sku': 'MVCFH27F'
                    }
                ]
            }
        ";
    }

    [Test]
    public void TestMapperThrowsExceptionWhenRootKeyOptionIsNotPassed()
    {
        var endpointResult = JsonConvert.SerializeObject(new { Test = "ABCDE" });
        var destinationCollection = new List<TestObject>();
        Assert.Throws<RootKeyOptionNullException>(() => endpointResult.MapCollection(destinationCollection, null));
    }

    [Test]
    public void TestMapperThrowsExceptionWhenJsonStringIsInvalid()
    {
        var endpointResult = "";
        var destinationCollection = new List<TestObject>();
        Assert.Throws<JsonContentException>(() => endpointResult.MapCollection(destinationCollection, null));
    }

    [Test]
    public void TestMapperThrowsExceptionWhenEndpointResultCannotBePassedToAnObject()
    {
        var endpointResult = "[1,2,3,4,5]";
        var destinationCollection = new List<TestObject>();

        Assert.Throws<JsonReaderException>(() =>
            endpointResult.MapCollection(destinationCollection, options => { options.RootKey = "SOMETHING"; }));
    }

    [Test]
    public void TestMapperShouldReturnEmptyWhenEndpointResultIsEmptyOrNull()
    {
        var endpointResult = JsonConvert.SerializeObject(new { Name = "123", State = "Arizona" });
        var destinationCollection = new List<TestObject>();

        Assert.Throws<RootKeyPropertyNullException>(() =>
            endpointResult.MapCollection(destinationCollection, options => { options.RootKey = "SOMETHING"; }));
    }

    [Test]
    public void TestMapperReturnsCollectionWhenRootKeyIsCorrect()
    {
        var destinationCollection = new List<TestObject>();
        var result = _result.MapCollection(destinationCollection, options => { options.RootKey = "products"; });
        Assert.That(result, Is.EqualTo(destinationCollection));
    }

    [Test]
    public void TestMapperReturnsCollectionWithMappingConfigurationCorrect()
    {
        var destinationCollection = new List<TestObjectWithVariation>();
        var result = _result.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "description" },
                { "BrandVariation", "brand" },
                { "CategoryVariation", "category" }
            };
        });

        var firstItem = result.FirstOrDefault();

        Assert.That(result, Is.EqualTo(destinationCollection));
        Assert.That(firstItem.DescriptionVariation, Is.Not.Null);
    }

    [Test]
    public void TestMapperThrowsExceptionWhenCollectionIsNotEmptyAndItemKeyIsNeeded()
    {
        var publicApiResult = JsonConvert.DeserializeObject<PublicApiViewModelWithVariation>(_result);
        var destinationCollection = publicApiResult.Products;

        Assert.Throws<ItemKeyOptionNullException>(() => _result.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "description" },
                { "BrandVariation", "brand" },
                { "CategoryVariation", "category" }
            };
        }));
    }

    [Test]
    public void TestMapperReturnsCorrectCollectionWithMappingConfigurationWithAnEmptyCollectionSupplied()
    {
        var publicApiResult = JsonConvert.DeserializeObject<PublicApiViewModel>(_result);

        var destinationCollection = new List<TestObjectWithVariation>();
        var result = _result.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "description" },
                { "BrandVariation", "brand" },
                { "CategoryVariation", "category" }
            };
        });

        var firstItemFromOriginal = publicApiResult.Products.FirstOrDefault();
        var firstItem = result.FirstOrDefault();

        Assert.That(firstItemFromOriginal, Is.Not.Null);
        Assert.That(firstItem.DescriptionVariation, Is.Not.Null);

        Assert.That(firstItemFromOriginal.Description, Is.EqualTo(firstItem.DescriptionVariation));
        Assert.That(firstItemFromOriginal.Brand, Is.EqualTo(firstItem.BrandVariation));
        Assert.That(firstItemFromOriginal.Category, Is.EqualTo(firstItem.CategoryVariation));
        Assert.That(result, Is.EqualTo(destinationCollection));
    }

    [Test]
    public void TestMapperReturnsCorrectCollectionWithMappingConfigurationWithCollection()
    {
        var publicApiResult = JsonConvert.DeserializeObject<PublicApiViewModel>(_result);
        var destinationCollection = JsonConvert.DeserializeObject<PublicApiViewModelWithVariation>(_result);

        var result = _result.MapCollection(destinationCollection.Products, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.Mappings = new Dictionary<string, string>
            {
                { "Title", "title" },
                { "DescriptionVariation", "description" },
                { "BrandVariation", "brand" },
                { "CategoryVariation", "category" }
            };
        });

        var firstItemFromOriginal = publicApiResult.Products.FirstOrDefault();
        var firstItem = result.FirstOrDefault();

        Assert.That(firstItemFromOriginal, Is.Not.Null);
        Assert.That(firstItem.DescriptionVariation, Is.Not.Null);

        Assert.That(firstItemFromOriginal.Description, Is.EqualTo(firstItem.DescriptionVariation));
        Assert.That(firstItemFromOriginal.Brand, Is.EqualTo(firstItem.BrandVariation));
        Assert.That(firstItemFromOriginal.Category, Is.EqualTo(firstItem.CategoryVariation));
        Assert.That(result, Is.EqualTo(destinationCollection.Products));
    }

    private string SampleCollectionWithNestedObjectsAsProperties()
    {
        return @"
            {
                'total': 2,
                'products': [
                    {
                        'id': 1,
                        'title': 'Essence Mascara',
                        'description': 'Resource to help get pets adopted',
                        'brand': 'Essence',
                        'category': 'beauty',
                        'work': {
                            'reportsToIdInCompany' : 64,
                            'employeeIdInCompany' : 140,
                            'reportsTo': {
                                'email': 'somebody@nomail.com'
                            }
                        }
                    },
                    {
                        'id': 2,
                        'title': 'Eyeshadow Palette',
                        'description': 'Collection of eyeshadow shades',
                        'brand': 'Glamour',
                        'category': 'beauty',
                        'work': {
                            'reportsToIdInCompany' : 50,
                            'employeeIdInCompany' : 160,
                            'reportsTo': {
                                'email': 'somebodyelse@nomail.com'
                            }
                        }
                    }
                ]
            }
        ";
    }

    [Test]
    public void TestMapperReturnsCollectionWithMappingConfigurationCorrectWithNestedProperties()
    {
        var destinationCollection = new List<TestObjectWithVariation>();
        var nestedResult = SampleCollectionWithNestedObjectsAsProperties();
        var result = nestedResult.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "description" },
                { "BrandVariation", "brand" },
                { "CategoryVariation", "category" },
                { "ReportsToIdInCompanyVariation", "work.reportsToIdInCompany" },
                { "ReportsToEmailVariation", "work.reportsTo.email" }
            };
        });

        var firstItem = result.FirstOrDefault();

        Assert.That(result, Is.EqualTo(destinationCollection));
        Assert.That(firstItem.DescriptionVariation, Is.Not.Null);
    }

    [Test]
    public void TestMapperSupportsNestedRootKeyWithDotNotation()
    {
        var json = @"
        {
            'data': {
                'products': [
                    { 'Id': 1, 'Title': 'ServiceA', 'Description': 'First', 'Brand': 'BrandA', 'Category': 'Test', 'Price': 9.99, 'Sku': 'ABC' },
                    { 'Id': 2, 'Title': 'ServiceB', 'Description': 'Second', 'Brand': 'BrandB', 'Category': 'Test', 'Price': 19.99, 'Sku': 'DEF' }
                ]
            }
        }";

        var destinationCollection = new List<TestObject>();
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "data.products";
        });

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("ServiceA"));
    }

    [Test]
    public void TestMapperSupportsThreeLevelNestedRootKeyWithDotNotation()
    {
        var json = @"
        {
            'response': {
                'data': {
                    'products': [
                        { 'Id': 1, 'Title': 'ServiceA', 'Description': 'First', 'Brand': 'BrandA', 'Category': 'Test', 'Price': 9.99, 'Sku': 'ABC' }
                    ]
                }
            }
        }";

        var destinationCollection = new List<TestObject>();
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "response.data.products";
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Title, Is.EqualTo("ServiceA"));
    }

    [Test]
    public void TestMapperDoesNotSetMappedPropertyWhenSourceFieldAbsentFromJson()
    {
        var json = @"{'products': [{'Id': 1, 'Title': 'ServiceA', 'Description': 'First', 'Brand': 'BrandA', 'Category': 'Test', 'Price': 9.99, 'Sku': 'ABC'}]}";

        var destinationCollection = new List<TestObjectWithVariation>();
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "NonExistentField" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().DescriptionVariation, Is.Null);
    }

    [Test]
    public void TestSeedKnownCollectionReturnsEarlyWhenNoMatchingRecordFoundInFeed()
    {
        var json = @"{'products': [{'Id': 1, 'Title': 'ServiceA', 'Description': 'First', 'Brand': 'BrandA', 'Category': 'Test', 'Price': 9.99, 'Sku': 'ABC'}]}";

        var destinationCollection = new List<TestObject> { new TestObject { Title = "NotInFeed" } };
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.Mappings = new Dictionary<string, string>
            {
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Title, Is.EqualTo("NotInFeed"));
        Assert.That(result.First().Description, Is.Null);
    }

    [Test]
    public void TestSeedKnownCollectionUsesRemappedJsonKeyWhenItemKeyIsMapped()
    {
        var json = @"{'products': [{'product_name': 'ServiceA', 'Description': 'First', 'Brand': 'BrandA', 'Category': 'Test', 'Price': 9.99, 'Sku': 'ABC'}]}";

        var destinationCollection = new List<TestObject> { new TestObject { Title = "ServiceA" } };
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.Mappings = new Dictionary<string, string>
            {
                { "Title", "product_name" },
                { "Description", "Description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Description, Is.EqualTo("First"));
    }

    [Test]
    public void TestCreateModeClearsDestinationWhenAllItemKeyValuesAreEmpty()
    {
        var json = @"{'products': [
            {'product_name': 'ServiceA', 'description': 'First'},
            {'product_name': 'ServiceB', 'description': 'Second'}
        ]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = null, Description = "ghost-1" },
            new TestObject { Title = "",   Description = "ghost-2" }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.Mappings = new Dictionary<string, string>
            {
                { "Title", "product_name" },
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(r => r.Description == "ghost-1"), Is.False);
        Assert.That(result.Any(r => r.Description == "ghost-2"), Is.False);
        Assert.That(result.Select(r => r.Title), Is.EquivalentTo(new[] { "ServiceA", "ServiceB" }));
    }

    [Test]
    public void TestUpdateModeStillRunsWhenSomeItemKeysArePopulated()
    {
        var json = @"{'products': [
            {'product_name': 'ServiceA', 'description': 'Fresh'}
        ]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = "ServiceA", Description = "old" },
            new TestObject { Title = null,       Description = "ghost" }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.Mappings = new Dictionary<string, string>
            {
                { "Title", "product_name" },
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.First(r => r.Title == "ServiceA").Description, Is.EqualTo("Fresh"));
        Assert.That(result.Any(r => r.Description == "ghost"), Is.True);
    }

    [Test]
    public void TestIsItemEmptyCallbackTriggersCreateModeAndClearsDestination()
    {
        var json = @"{'products': [
            {'product_name': 'ServiceA', 'description': 'First'},
            {'product_name': 'ServiceB', 'description': 'Second'}
        ]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = "ServiceA", Description = null, Brand = null },
            new TestObject { Title = "ServiceB", Description = null, Brand = null }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.IsItemEmpty = item => string.IsNullOrEmpty(((TestObject)item).Description);
            options.Mappings = new Dictionary<string, string>
            {
                { "Title", "product_name" },
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(r => !string.IsNullOrEmpty(r.Description)), Is.True);
    }

    [Test]
    public void TestIsItemEmptyCallbackTakesPrecedenceOverItemKeyDefault()
    {
        var json = @"{'products': [
            {'product_name': 'ServiceA', 'description': 'Fresh'}
        ]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = "ServiceA", Description = "old" }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.IsItemEmpty = _ => true;
            options.Mappings = new Dictionary<string, string>
            {
                { "Title", "product_name" },
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Description, Is.EqualTo("Fresh"));
        Assert.That(result.Any(r => r.Description == "old"), Is.False);
    }

    [Test]
    public void TestUpdateModeReturnsEarlyWhenItemKeyDoesNotExistOnDestinationType()
    {
        var json = @"{'products': [{'product_name': 'A', 'description': 'Fresh'}]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = "A", Description = "old" }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "NonExistentProperty";
            options.IsItemEmpty = _ => false;
            options.Mappings = new Dictionary<string, string>
            {
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Description, Is.EqualTo("old"));
    }

    [Test]
    public void TestUpdateModeReturnsEarlyWhenItemKeyValueIsNullOnDestinationItem()
    {
        var json = @"{'products': [{'title': 'A', 'description': 'Fresh'}]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = null, Description = "old" }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.IsItemEmpty = _ => false;
            options.Mappings = new Dictionary<string, string>
            {
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Description, Is.EqualTo("old"));
    }

    [Test]
    public void TestUpdateModeSkipsEmptyMappedJsonValuesForExistingItem()
    {
        var json = @"{'products': [{'title': 'A', 'description': ''}]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = "A", Description = "original" }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "Title";
            options.Mappings = new Dictionary<string, string>
            {
                { "Description", "description" }
            };
        });

        Assert.That(result.First().Description, Is.EqualTo("original"));
    }

    [Test]
    public void TestDefaultEmptyCheckTreatsNonExistentItemKeyPropertyAsEmpty()
    {
        var json = @"{'products': [{'product_name': 'A', 'description': 'Fresh'}]}";

        var destinationCollection = new List<TestObject>
        {
            new TestObject { Title = "A", Description = "ghost" }
        };

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.ItemKey = "NonExistentProperty";
            options.Mappings = new Dictionary<string, string>
            {
                { "Title", "product_name" },
                { "Description", "description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Title, Is.EqualTo("A"));
        Assert.That(result.First().Description, Is.EqualTo("Fresh"));
        Assert.That(result.Any(r => r.Description == "ghost"), Is.False);
    }

    [Test]
    public void TestSetPropertyValueSkipsReadOnlyProperties()
    {
        var json = @"{'products': [{'ro': 'newval'}]}";

        var destinationCollection = new List<TestObjectWithReadOnly>();

        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "products";
            options.Mappings = new Dictionary<string, string>
            {
                { "ReadOnlyField", "ro" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().ReadOnlyField, Is.EqualTo("fixed"));
    }
}
