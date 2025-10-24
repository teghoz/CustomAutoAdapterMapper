using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomAutoAdapterMapper.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomAutoAdapterMapper
{
    public static class Mapper
    {
        public static List<T> MapCollection<T>(this string jsonResponse, List<T> destination, Action<Option> options)
        {
            var mapperOptions = new Option();
            options?.Invoke(mapperOptions);
            JObject jsonObject = null;

            var rootProperty = Validate(jsonResponse, jsonObject, mapperOptions);
            var entries = rootProperty.ToList();

            if (MapperShouldIterateThroughEntireIncomingCollection(destination, mapperOptions))
                foreach (var entry in entries)
                {
                    var collectionItem = Activator.CreateInstance<T>();

                    SeedCollectionItem(entry, collectionItem);
                    SeedMappedPropertiesOfItem(entry, collectionItem, mapperOptions);
                    destination.Add(collectionItem);
                }
            else
                foreach (var entry in destination)
                    SeedKnownCollectionOfItem(entries, entry, mapperOptions);

            return destination;
        }

        private static bool JsonStringIsValid(string jsonString)
        {
            try
            {
                JToken.Parse(jsonString);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static bool MapperShouldIterateThroughEntireIncomingCollection<T>(List<T> destination, Option options)
        {
            var itemKeyIdentifierIsEmpty = destination != null &&
                                           !string.IsNullOrEmpty(options.ItemKey) &&
                                           destination.All(x =>
                                               string.IsNullOrEmpty(x.GetType()?.GetProperty(options.ItemKey)
                                                   ?.GetValue(x)?.ToString() ?? string.Empty));

            var result = destination == null || destination.Count == 0 || itemKeyIdentifierIsEmpty;
            return result;
        }

        private static void SeedCollectionItem<T>(JToken entry, T collectionItem)
        {
            var collectionItemProperties = collectionItem
                .GetType()
                .GetProperties()
                .ToList();

            foreach (var property in collectionItemProperties)
            {
                var mappedValue = entry.SelectToken(property.Name)?.ToObject(property.PropertyType) ?? null;
                SetPropertyValue(collectionItem, property.Name, mappedValue);
            }
        }

        private static void SeedMappedPropertiesOfItem<T>(JToken entry, T collectionItem, Option mapperOptions)
        {
            var matchedProperties = collectionItem
                .GetType()
                .GetProperties()
                .Where(i => mapperOptions.MappingKeys.Contains(i.Name))
                .ToList();

            foreach (var property in matchedProperties)
            {
                var incomingProperty = mapperOptions.Mappings[property.Name];

                var mappedPropertyValue =
                    entry.SelectToken(incomingProperty)?.ToObject(property.PropertyType) ?? null;

                if (mappedPropertyValue != null)
                    SetPropertyValue(collectionItem, property.Name, mappedPropertyValue);
            }
        }

        private static void SeedKnownCollectionOfItem<T>(List<JToken> entries, T entry, Option mapperOptions)
        {
            if (string.IsNullOrEmpty(mapperOptions.ItemKey))
                throw new ItemKeyOptionNullException("Item Key Option Not Set!!!!!");

            var keyItemExist = entry.GetType().GetProperty(mapperOptions.ItemKey);

            if (keyItemExist == null) return;
            var propertyKeyItemValue =
                entry.GetType()?.GetProperty(mapperOptions.ItemKey)?.GetValue(entry)?.ToString() ?? null;
            if (propertyKeyItemValue == null) return;
            var incomingRecord = entries
                .Where(e => e.Values().Any(ee => ee.ToString() == propertyKeyItemValue))
                .FirstOrDefault();

            var matchedProperties = entry
                .GetType()
                .GetProperties()
                .Where(i => mapperOptions.MappingKeys.Contains(i.Name))
                .ToList();

            foreach (var property in matchedProperties)
            {
                var incomingProperty = mapperOptions.Mappings[property.Name];

                if (incomingRecord[incomingProperty] != null)
                {
                    var mappedValue = incomingRecord[incomingProperty].ToString();

                    if (!string.IsNullOrEmpty(mappedValue)) SetPropertyValue(entry, property.Name, mappedValue);
                }
            }
        }

        private static JToken Validate(string jsonResponse, JObject jsonObject, Option option)
        {
            if (!JsonStringIsValid(jsonResponse)) throw new JsonContentException("Json Content Supplied Is Invalid");

            if (string.IsNullOrEmpty(option.RootKey))
                throw new RootKeyOptionNullException("Root Key Is Required to map");

            jsonObject = JObject.Parse(jsonResponse);

            var rootProperty = jsonObject[option.RootKey];

            if (rootProperty == null)
                throw new RootKeyPropertyNullException("Root Property Does Not Exist In Object!!!!!");

            return rootProperty;
        }

        private static void SetPropertyValue<T>(T destinationObject, string propertyName, object value)
        {
            var prop = destinationObject
                .GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (prop != null && prop.CanWrite) prop.SetValue(destinationObject, value, null);
        }
    }
}