using CustomAutoAdapterMapper.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomAutoAdapterMapper
{
    public static class Mapper
    {
        public static List<T> MapCollection<T>(this string jsonResponse, List<T> destination, Action<Option> options)
        {
            var mapperOptions = new Option();
            options?.Invoke(mapperOptions);
            List<T> result = destination;
            JObject jsonObject = null;

            Validate(mapperOptions);

            try
            {
                jsonObject = JObject.Parse(jsonResponse);
            }
            catch(Exception)
            {
                throw;
            }

            var rootProperty = jsonObject[mapperOptions.RootKey];

            if (rootProperty == null)
            {
                throw new RootKeyPropertyNullException("Root Property Does Not Exist In Object!!!!!");
            }

            var entries = rootProperty.ToList();

            if (string.IsNullOrEmpty(jsonResponse))
            {
                return new List<T>();
            }

            if(MapperShouldIterateThroughEntireCollection(result, mapperOptions))
            {
                foreach (var entry in entries)
                {
                    T collectionItem = Activator.CreateInstance<T>();

                    SeedCollectionItem(entry, collectionItem);
                    SeedMappedPropertiesOfItem(entry, collectionItem, mapperOptions);
                    result.Add(collectionItem);
                }
            }
            else
            {
                foreach (var entry in result)
                {
                    SeedknownCollectionItem(entries, entry, mapperOptions);
                }
            }

            return result;
        }
        private static bool MapperShouldIterateThroughEntireCollection<T>(List<T> destination, Option options)
        {
            var result = false;
            var itemKeyIdentifierIsEmpty = destination != null &&
                !string.IsNullOrEmpty(options.ItemKey) &&
                destination.All(x => string.IsNullOrEmpty(x.GetType().GetProperty(options.ItemKey).GetValue(x).ToString()));

            result = destination == null || destination.Count == 0 || itemKeyIdentifierIsEmpty;
            return result;
        }
        private static void SeedCollectionItem<T>(JToken entry, T collectionItem)
        {
            var collectionItemProperties = collectionItem.GetType().GetProperties().ToList();

            foreach (var property in collectionItemProperties)
            {
                var mappedValue = entry[property.Name]?.ToString() ?? null;
                SetPropertyValue(collectionItem, property.Name, mappedValue);
            }
        }
        private static void SeedMappedPropertiesOfItem<T>(JToken entry, T collectionItem, Option mapperOptions)
        {
            var matchedProperties = collectionItem.GetType().GetProperties().Where(i => mapperOptions.MappingKeys.Contains(i.Name)).ToList();

            foreach (var property in matchedProperties)
            {
                var incomingProperty = mapperOptions.Mappings[property.Name];
                if (entry[incomingProperty] != null)
                {
                    var mappedPropertyValue = entry[incomingProperty].ToString() ?? null;

                    if (!string.IsNullOrEmpty(mappedPropertyValue))
                    {
                        SetPropertyValue(collectionItem, property.Name, mappedPropertyValue);
                    }
                }
            }
        }
        private static void SeedknownCollectionItem<T>(List<JToken> entries, T entry, Option mapperOptions)
        {
            if (string.IsNullOrEmpty(mapperOptions.ItemKey))
            {
                throw new ItemKeyOptionNullException("Item Key Option Not Set!!!!!");
            }

            var keyItemExist = entry.GetType().GetProperty(mapperOptions.ItemKey);

            if (keyItemExist != null)
            {
                var propertyKeyItemValue = entry.GetType()?.GetProperty(mapperOptions.ItemKey)?.GetValue(entry)?.ToString() ?? null;
                if (propertyKeyItemValue != null)
                {
                    var incomingRecord = entries.Where(e => e.Values().Any(ee => ee.ToString() == propertyKeyItemValue)).FirstOrDefault();
                    var matchedProperties = entry.GetType().GetProperties().Where(i => mapperOptions.MappingKeys.Contains(i.Name)).ToList();

                    foreach (var property in matchedProperties)
                    {
                        var incomingProperty = mapperOptions.Mappings[property.Name];

                        if (incomingRecord[incomingProperty] != null)
                        {
                            var mappedValue = incomingRecord[incomingProperty].ToString();

                            if (!string.IsNullOrEmpty(mappedValue))
                            {
                                SetPropertyValue(entry, property.Name, mappedValue);
                            }
                        }
                    }
                }
            }
        }
        private static void Validate(Option option)
        {
            if (string.IsNullOrEmpty(option.RootKey))
            {
                throw new RootKeyOptionNullException("Root Key Is Required to map");
            }
        }
        private static void SetPropertyValue<T>(T desinationObject, string propertyName, object value)
        {
            PropertyInfo prop = desinationObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(desinationObject, value, null);
            }
        }
    }
}
