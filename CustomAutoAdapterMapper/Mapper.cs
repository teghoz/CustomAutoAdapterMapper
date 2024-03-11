using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomAutoAdapterMapper
{
    public static class Mapper
    {
        public static List<T> MapCollection<T>(this string jsonResposne, List<T> destination, Action<Option> options)
        {
            var mapperOptions = new Option();
            options?.Invoke(mapperOptions);
            List<T> result = destination;

            Validate(mapperOptions);

            if (string.IsNullOrEmpty(jsonResposne))
            {
                return new List<T>();
            }

            JObject jsonObject = JObject.Parse(jsonResposne);
            var rootProperty = jsonObject[mapperOptions.RootKey];
            var entries = rootProperty.ToList();

            foreach (var entry in entries)
            {
                T collectionItem = Activator.CreateInstance<T>();

                SeedCollectionItem(entry, collectionItem);
                SeedMappedPropertiesOfItem(entry, collectionItem, mapperOptions);
                result.Add(collectionItem);
            }

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
        private static void Validate(Option option)
        {
            if (string.IsNullOrEmpty(option.RootKey))
            {
                throw new ArgumentNullException(nameof(option.RootKey), "Root Key Is Required to map");
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
