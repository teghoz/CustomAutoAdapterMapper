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
            Assert.Throws<ArgumentNullException>(() => endpointResult.MapCollection(destinationCollection, null));
        }

        [Test]
        public void TestMapperReturnEmptyListWhenEndpointResultIsEmptyOrNull()
        {
            var endpointResult = "";
            var destinationCollection = new List<TestObject>();
            var result = endpointResult.MapCollection(destinationCollection, options =>
            {
                options.RootKey = "SOMETHING";
            });
            Assert.That(result, Is.EqualTo(destinationCollection));
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
        public void TestMapperReturnsCollectionWithMappingConfigurationCorrect2()
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
    }
}