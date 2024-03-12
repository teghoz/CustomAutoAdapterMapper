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
        }

        [Test]
        public void TestMapperThrowsExceptionWhenRootKeyOptionIsNotPassed()
        {
            var endpointResult = "";
            var destinationCollection = new List<TestObject>();
            Assert.Throws<RootKeyOptionNullException>(() => endpointResult.MapCollection(destinationCollection, null));
        }

        [Test]
        public void TestMapperThrowsExceptionWhenEndpointResultIsEmptyOrNull()
        {
            var endpointResult = "";
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
            var httpResponseMessage = client.GetAsync("https://api.publicapis.org/entries").Result;
            var actualResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;
            var publicAPIResult = JsonConvert.DeserializeObject<PublicAPIViewModelWithVariation>(actualResponse);

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
            var httpResponseMessage = client.GetAsync("https://api.publicapis.org/entries").Result;
            var actualResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;
            var publicAPIResult = JsonConvert.DeserializeObject<PublicAPIViewModel>(actualResponse);

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
            var httpResponseMessage = client.GetAsync("https://api.publicapis.org/entries").Result;
            var actualResponse = httpResponseMessage.Content.ReadAsStringAsync().Result;
            var publicAPIResult = JsonConvert.DeserializeObject<PublicAPIViewModel>(actualResponse);

            var fixture = new Fixture();
            var destinationCollection = JsonConvert.DeserializeObject<PublicAPIViewModelWithVariation>(actualResponse);

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