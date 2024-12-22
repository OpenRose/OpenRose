// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Entities;
using ItemzApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _itemzPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                // TODO: We are manually entering this information in the code to map
                // different properties between source and target objects. We should also 
                // consider implementing this via some sort of Dynamic method. This information
                // should either come from Database Table or from some sort of configuration file.

                {"Id", new PropertyMappingValue(new List<string>() {"Id" } )},
                {"Name", new PropertyMappingValue(new List<string>(){"Name"} )},
                {"Status", new PropertyMappingValue(new List<string>(){ "Status" } )},
                {"Priority", new PropertyMappingValue(new List<string>(){ "Priority" } )},
                {"Description", new PropertyMappingValue(new List<string>(){ "Description" } )},
                {"CreatedBy", new PropertyMappingValue(new List<string>(){ "CreatedBy" } )},
                {"CreatedDate", new PropertyMappingValue(new List<string>(){ "CreatedDate" } )}
            };

        private Dictionary<string, PropertyMappingValue> _itemzWithBasePropertiesPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                // TODO: We are manually entering this information in the code to map
                // different properties between source and target objects. We should also 
                // consider implementing this via some sort of Dynamic method. This information
                // should either come from Database Table or from some sort of configuration file.

                {"Id", new PropertyMappingValue(new List<string>() {"Id" } )},
                {"Name", new PropertyMappingValue(new List<string>(){"Name"} )},
                {"Status", new PropertyMappingValue(new List<string>(){ "Status" } )},
                {"Priority", new PropertyMappingValue(new List<string>(){ "Priority" } )},
                {"CreatedDate", new PropertyMappingValue(new List<string>(){ "CreatedDate" } )}
            };

        private Dictionary<string, PropertyMappingValue> _baselineItemzPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                // TODO: We are manually entering this information in the code to map
                // different properties between source and target objects. We should also 
                // consider implementing this via some sort of Dynamic method. This information
                // should either come from Database Table or from some sort of configuration file.

                {"Id", new PropertyMappingValue(new List<string>() {"Id" } )},
                {"Name", new PropertyMappingValue(new List<string>(){"Name"} )},
                {"Status", new PropertyMappingValue(new List<string>(){ "Status" } )},
                {"Priority", new PropertyMappingValue(new List<string>(){ "Priority" } )},
                {"Description", new PropertyMappingValue(new List<string>(){ "Description" } )},
                {"CreatedBy", new PropertyMappingValue(new List<string>(){ "CreatedBy" } )},
                {"CreatedDate", new PropertyMappingValue(new List<string>(){ "CreatedDate" } )}
            };
        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<GetItemzDTO, Itemz>(_itemzPropertyMapping));
            _propertyMappings.Add(new PropertyMapping<GetItemzWithBasePropertiesDTO, GetItemzWithBasePropertiesDTO>(_itemzWithBasePropertiesPropertyMapping));
            _propertyMappings.Add(new PropertyMapping<GetBaselineItemzDTO, BaselineItemz>(_baselineItemzPropertyMapping));
        }

        public bool ValidMappingExistsFor<TSource,TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();

                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);
                if(!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }
        public Dictionary<string, PropertyMappingValue> GetPropertyMapping
            <TSource, TDestination>()
        {
            var matchingMapping = _propertyMappings
                .OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }
            throw new Exception($"Cannot find exact property mapping instance" +
                $"for <{typeof(TSource)}, {typeof(TDestination)}");
        }
    }
}
