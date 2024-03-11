using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomAutoAdapterMapper
{
    public class Mapper
    {
        public List<T> MapCollection<T>(string jsonResposne, List<T> destination, Action<Option> options)
        {
            var mapperOptions = new Option();
            options?.Invoke(mapperOptions);
            List<T> result = destination;

            Validations(mapperOptions);

            if (string.IsNullOrEmpty(jsonResposne))
            {
                return result;
            }

            JObject jsonObject = JObject.Parse(jsonResposne);
            var rootProperty = jsonObject[mapperOptions.RootKey];
            var entries = rootProperty.ToList();

            foreach (var entry in entries)
            {
                T collectionItem = default(T);
                var collectionItemProperties = collectionItem.GetType().GetProperties().ToList();  

                foreach (var property in collectionItemProperties )
                {
                    var mappedValue = entry[property.Name]?.ToString() ?? null;
                    SetPropertyValue(collectionItem, property.Name, mappedValue);
                }

                var matchedProperties = collectionItem.GetType().GetProperties().Where(i => mapperOptions.MappingKeys.Contains(i.Name)).ToList();

                foreach(var property in matchedProperties)
                {
                    var incomingPropertyValue = mapperOptions.Mappings[property.Name];
                    if (entry[incomingPropertyValue] != null)
                    {

                    }
                }

            }

            return result;
        }

        private void Validations(Option option)
        {
            if(option == null)
            {
                throw new ArgumentNullException("Options is required to map");
            }

            if (string.IsNullOrEmpty(option.RootKey))
            {
                throw new ArgumentNullException("Root Key Is Required to map");
            }
        }
        private void SetPropertyValue<T>(T desinationObject, string propertyName, object value)
        {
            PropertyInfo prop = desinationObject.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if(prop != null && prop.CanWrite)
            {
                prop.SetValue(desinationObject, value, null);
            }
        }
    }
}
