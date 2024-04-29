using AutoFixture;
using CustomAutoAdapterMapper.Exceptions;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace CustomAutoAdapterMapper.Tests
{
    public class Tests
    {
        private string _result;
        [SetUp]
        public void Setup()
        {
            var client = new HttpClient();
            var result = client.GetAsync("https://api.publicapis.org/entries").Result;
            _result = result.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(_result))
            {
                _result = Fallback();
            }
        }

        public string Fallback()
        {
            return $@"
                {{
                    'count': 1427,
                    'entries': [
                        {{
                            'API': 'AdoptAPet',
                            'Description': 'Resource to help get pets adopted',
                            'Auth': 'apiKey',
                            'HTTPS': true,
                            'Cors': 'yes',
                            'Link': 'https://www.adoptapet.com/public/apis/pet_list.html',
                            'Category': 'Animals'
                        }},
                        {{
                            'API': 'Axolotl',
                            'Description': 'Collection of axolotl pictures and facts',
                            'Auth': '',
                            'HTTPS': true,
                            'Cors': 'no',
                            'Link': 'https://theaxolotlapi.netlify.app/',
                            'Category': 'Animals'
                        }}
                    ]
                }}
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

            Assert.Throws<JsonReaderException>(() => endpointResult.MapCollection(destinationCollection, options =>
            {
                options.RootKey = "SOMETHING";
            }));
        }

        [Test]
        public void TestMapperShouldReturnEmptyWhenEndpointResultIsEmptyOrNull()
        {
            var endpointResult = JsonConvert.SerializeObject(new { Name = "123", State = "Arizona" });
            var destinationCollection = new List<TestObject>();

            Assert.Throws<RootKeyPropertyNullException>(() => endpointResult.MapCollection(destinationCollection, options =>
            {
                options.RootKey = "SOMETHING";
            }));
        }

        [Test]
        public void TestMapperReturnsCollectionWhenRootKeyIsCorrect()
        {
            var destinationCollection = new List<TestObject>();
            var result = _result.MapCollection(destinationCollection, options =>
            {
                options.RootKey = "entries";
            });
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
                    { "CategoryVariation", "Category" },
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
            var publicAPIResult = JsonConvert.DeserializeObject<PublicAPIViewModelWithVariation>(_result);

            var destinationCollection = publicAPIResult.Entries;

            Assert.Throws<ItemKeyOptionNullException>(() => _result.MapCollection(destinationCollection, options =>
            {
                options.RootKey = "entries";
                options.Mappings = new Dictionary<string, string>
                {
                    { "DescriptionVariation", "Description" },
                    { "AuthVariation", "Auth" },
                    { "CategoryVariation", "Category" },
                };
            }));
        }

        [Test]
        public void TestMapperReturnsCorrectCollectionWithMappingConfigurationWithAnEmptyCollectionSupplied()
        {
            var client = new HttpClient();
            var publicAPIResult = JsonConvert.DeserializeObject<PublicAPIViewModel>(_result);

            var destinationCollection = new List<TestObjectWithVariation>();
            var result = _result.MapCollection(destinationCollection, options =>
            {
                options.RootKey = "entries";
                options.Mappings = new Dictionary<string, string>
                {
                    { "DescriptionVariation", "Description" },
                    { "AuthVariation", "Auth" },
                    { "CategoryVariation", "Category" },
                };
            });

            var firstItemFromOriginal = publicAPIResult.Entries.FirstOrDefault();
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
            var publicAPIResult = JsonConvert.DeserializeObject<PublicAPIViewModel>(_result);

            var fixture = new Fixture();
            var destinationCollection = JsonConvert.DeserializeObject<PublicAPIViewModelWithVariation>(_result);

            var result = _result.MapCollection(destinationCollection.Entries, options =>
            {
                options.RootKey = "entries";
                options.ItemKey = "API";
                options.Mappings = new Dictionary<string, string>
                {
                    { "DescriptionVariation", "Description" },
                    { "AuthVariation", "Auth" },
                    { "CategoryVariation", "Category" },
                };
            });

            var firstItemFromOriginal = publicAPIResult.Entries.FirstOrDefault();
            var firstItem = result.FirstOrDefault();

            Assert.That(firstItemFromOriginal, Is.Not.Null);
            Assert.That(firstItem.DescriptionVariation, Is.Not.Null);

            Assert.That(firstItemFromOriginal.Description, Is.EqualTo(firstItem.DescriptionVariation));
            Assert.That(firstItemFromOriginal.Auth, Is.EqualTo(firstItem.AuthVariation));
            Assert.That(firstItemFromOriginal.Category, Is.EqualTo(firstItem.CategoryVariation));
            Assert.That(result, Is.EqualTo(destinationCollection.Entries));
        }
    }
}