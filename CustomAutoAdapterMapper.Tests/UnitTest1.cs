using AutoFixture;
using CustomAutoAdapterMapper.Exceptions;
using Newtonsoft.Json;

namespace CustomAutoAdapterMapper.Tests;

public class Tests
{
    private string _result;

    [SetUp]
    public void Setup()
    {
        try
        {
            var client = new HttpClient();
            var result = client.GetAsync("https://api.publicapis.org/entries").Result;
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
                    'count': 1427,
                    'entries': [
                        {
                            'API': 'AdoptAPet',
                            'Description': 'Resource to help get pets adopted',
                            'Auth': 'apiKey',
                            'HTTPS': true,
                            'Cors': 'yes',
                            'Link': 'https://www.adoptapet.com/public/apis/pet_list.html',
                            'Category': 'Animals'
                        },
                        {
                            'API': 'Axolotl',
                            'Description': 'Collection of axolotl pictures and facts',
                            'Auth': '',
                            'HTTPS': true,
                            'Cors': 'no',
                            'Link': 'https://theaxolotlapi.netlify.app/',
                            'Category': 'Animals'
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
        var result = _result.MapCollection(destinationCollection, options => { options.RootKey = "entries"; });
        Assert.That(result, Is.EqualTo(destinationCollection));
    }

    [Test]
    public void TestMapperReturnsCollectionWithMappingConfigurationCorrect()
    {
        var destinationCollection = new List<TestObjectWithVariation>();
        var result = _result.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "entries";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "Description" },
                { "AuthVariation", "Auth" },
                { "CategoryVariation", "Category" }
            };
        });

        var firstItem = result.FirstOrDefault();

        Assert.That(result, Is.EqualTo(destinationCollection));
        Assert.That(firstItem.DescriptionVariation, Is.Not.Null);
    }

    [Test]
    public void TestMapperThrowsExceptionWhenCollectionIsNotEmptyAndItemKeyIsNeeded()
    {
        var client = new HttpClient();
        var publicApiResult = JsonConvert.DeserializeObject<PublicApiViewModelWithVariation>(_result);

        var destinationCollection = publicApiResult.Entries;

        Assert.Throws<ItemKeyOptionNullException>(() => _result.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "entries";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "Description" },
                { "AuthVariation", "Auth" },
                { "CategoryVariation", "Category" }
            };
        }));
    }

    [Test]
    public void TestMapperReturnsCorrectCollectionWithMappingConfigurationWithAnEmptyCollectionSupplied()
    {
        var client = new HttpClient();
        var publicApiResult = JsonConvert.DeserializeObject<PublicApiViewModel>(_result);

        var destinationCollection = new List<TestObjectWithVariation>();
        var result = _result.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "entries";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "Description" },
                { "AuthVariation", "Auth" },
                { "CategoryVariation", "Category" }
            };
        });

        var firstItemFromOriginal = publicApiResult.Entries.FirstOrDefault();
        var firstItem = result.FirstOrDefault();

        Assert.That(firstItemFromOriginal, Is.Not.Null);
        Assert.That(firstItem.DescriptionVariation, Is.Not.Null);

        Assert.That(firstItemFromOriginal.Description, Is.EqualTo(firstItem.DescriptionVariation));
        Assert.That(firstItemFromOriginal.Auth, Is.EqualTo(firstItem.AuthVariation));
        Assert.That(firstItemFromOriginal.Category, Is.EqualTo(firstItem.CategoryVariation));
        Assert.That(result, Is.EqualTo(destinationCollection));
    }

    [Test]
    public void TestMapperReturnsCorrectCollectionWithMappingConfigurationWithCollection()
    {
        var client = new HttpClient();
        var publicApiResult = JsonConvert.DeserializeObject<PublicApiViewModel>(_result);

        var fixture = new Fixture();
        var destinationCollection = JsonConvert.DeserializeObject<PublicApiViewModelWithVariation>(_result);

        var result = _result.MapCollection(destinationCollection.Entries, options =>
        {
            options.RootKey = "entries";
            options.ItemKey = "API";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "Description" },
                { "AuthVariation", "Auth" },
                { "CategoryVariation", "Category" }
            };
        });

        var firstItemFromOriginal = publicApiResult.Entries.FirstOrDefault();
        var firstItem = result.FirstOrDefault();

        Assert.That(firstItemFromOriginal, Is.Not.Null);
        Assert.That(firstItem.DescriptionVariation, Is.Not.Null);

        Assert.That(firstItemFromOriginal.Description, Is.EqualTo(firstItem.DescriptionVariation));
        Assert.That(firstItemFromOriginal.Auth, Is.EqualTo(firstItem.AuthVariation));
        Assert.That(firstItemFromOriginal.Category, Is.EqualTo(firstItem.CategoryVariation));
        Assert.That(result, Is.EqualTo(destinationCollection.Entries));
    }

    private string SampleCollectionWithNestedObjectsAsProperties()
    {
        return @"
                {
                    'count': 1427,
                    'entries': [
                        {
                            'API': 'AdoptAPet',
                            'Description': 'Resource to help get pets adopted',
                            'Auth': 'apiKey',
                            'HTTPS': true,
                            'Cors': 'yes',
                            'work': {
                                'reportsToIdInCompany' : 64,
                                'employeeIdInCompany' : 140,
                                'reportsTo': {
                                    'email': 'somebody@nomail.com'
                                }
                            },
                            'Link': 'https://www.adoptapet.com/public/apis/pet_list.html',
                            'Category': 'Animals'
                        },
                        {
                            'API': 'Axolotl',
                            'Description': 'Collection of axolotl pictures and facts',
                            'Auth': '',
                            'HTTPS': true,
                            'Cors': 'no',
                            'work': {
                                'reportsToIdInCompany' : 50,
                                'employeeIdInCompany' : 160,
                                'reportsTo': {
                                    'email': 'somebodyelse@nomail.com'
                                }
                            },
                            'Link': 'https://theaxolotlapi.netlify.app/',
                            'Category': 'Animals'
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
            options.RootKey = "entries";
            options.Mappings = new Dictionary<string, string>
            {
                { "DescriptionVariation", "Description" },
                { "AuthVariation", "Auth" },
                { "CategoryVariation", "Category" },
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
                'employees': [
                    { 'API': 'ServiceA', 'Description': 'First', 'Auth': 'apiKey', 'HTTPS': true, 'Cors': 'yes', 'Link': 'https://a.com', 'Category': 'Test' },
                    { 'API': 'ServiceB', 'Description': 'Second', 'Auth': 'none', 'HTTPS': false, 'Cors': 'no', 'Link': 'https://b.com', 'Category': 'Test' }
                ]
            }
        }";

        var destinationCollection = new List<TestObject>();
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "data.employees";
        });

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.First().API, Is.EqualTo("ServiceA"));
    }

    [Test]
    public void TestMapperSupportsThreeLevelNestedRootKeyWithDotNotation()
    {
        var json = @"
        {
            'response': {
                'data': {
                    'employees': [
                        { 'API': 'ServiceA', 'Description': 'First', 'Auth': 'apiKey', 'HTTPS': true, 'Cors': 'yes', 'Link': 'https://a.com', 'Category': 'Test' }
                    ]
                }
            }
        }";

        var destinationCollection = new List<TestObject>();
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "response.data.employees";
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().API, Is.EqualTo("ServiceA"));
    }

    [Test]
    public void TestMapperDoesNotSetMappedPropertyWhenSourceFieldAbsentFromJson()
    {
        var json = @"{'entries': [{'API': 'ServiceA', 'Description': 'First', 'Auth': 'none', 'HTTPS': true, 'Cors': 'yes', 'Link': 'https://a.com', 'Category': 'Test'}]}";

        var destinationCollection = new List<TestObjectWithVariation>();
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "entries";
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
        var json = @"{'entries': [{'API': 'ServiceA', 'Description': 'First', 'Auth': 'apiKey', 'HTTPS': true, 'Cors': 'yes', 'Link': 'https://a.com', 'Category': 'Test'}]}";

        var destinationCollection = new List<TestObject> { new TestObject { API = "NotInFeed" } };
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "entries";
            options.ItemKey = "API";
            options.Mappings = new Dictionary<string, string>
            {
                { "Description", "Description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().API, Is.EqualTo("NotInFeed"));
        Assert.That(result.First().Description, Is.Null);
    }

    [Test]
    public void TestSeedKnownCollectionUsesRemappedJsonKeyWhenItemKeyIsMapped()
    {
        var json = @"{'entries': [{'api_name': 'ServiceA', 'Description': 'First', 'Auth': 'apiKey', 'HTTPS': true, 'Cors': 'yes', 'Link': 'https://a.com', 'Category': 'Test'}]}";

        var destinationCollection = new List<TestObject> { new TestObject { API = "ServiceA" } };
        var result = json.MapCollection(destinationCollection, options =>
        {
            options.RootKey = "entries";
            options.ItemKey = "API";
            options.Mappings = new Dictionary<string, string>
            {
                { "API", "api_name" },
                { "Description", "Description" }
            };
        });

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().Description, Is.EqualTo("First"));
    }
}