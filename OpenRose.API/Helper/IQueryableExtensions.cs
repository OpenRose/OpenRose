// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ItemzApp.API.Helper
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source,
            string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (mappingDictionary == null)
            {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            var orderByString = string.Empty;

            // Because orderBy string is contains values that are split by comma,
            // we perform following action to Split by comma.

            var orderByAfterSplit = orderBy.Split(',');

            // Apply each orderBy clause in reverse order - otherwise,
            // the IQuaryable will be ordered in the wrong order.

            //            foreach(var orderByClause in orderByAfterSplit.Reverse())
            foreach (var orderByClause in orderByAfterSplit)
            {
                // First Trip the orderByClause.
                var trimmedOrderByClause = orderByClause.Trim();
                var orderDescending = trimmedOrderByClause.ToUpper().EndsWith(" DESC");

                //remove " asc" or " desc" from the orderByclause

                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);
                // lets now find the mapping for the property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                // from the Dictionary of PropertyMaps, get the specific PropertyMappingValue
                // for the property that is included in OrderByClause.
                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException(nameof(propertyMappingValue));
                }

                // There are chances that the target entity might have multiple 
                // properties mapped to a single property in the DTO. For example,
                // one could have "FirstName" and "LastName" in the Entity as two
                // different field which is represented via concatenation on DTO as
                // "Full Name". In such a case, when requesting users pass on OrderBy
                // "Full Name" (as per defined consumer facing DTO), then internally we
                // have to order it by "FirstName" then by "LastName". This is the reason
                // why we have to travel through propertyMappingValue.DestinationProperties.

                foreach (var destinationPropety in propertyMappingValue.DestinationProperties)
                {
                    // Some properties would require reverse order. For example, Age. 
                    // If someone says order by age and we have DateOfBirth field that is used
                    // for computing Age and if we just pass back list of itemz ordered by "DateOfBirth" 
                    // then it will show it in reverse order of age. i.e. it will show Age Descending 
                    // instead of Ascending. In such special cases, we have to use this 
                    // technique where we revert the given Asending OR Descending value passed to us
                    // in OrderByClause.

                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    // In the below statement, you will notice that it is capturing 
                    // "orderByString" which is actually defined
                    // outside of this and it's parent foreach loops. This is then used 
                    // for building the actual OrderByString that will be passed to LINQ statement
                    // automatically at Runtime. This way, we use Dynamic Expressions to 
                    // automatically populate correct OrderByClause that has to be sent out to
                    // the DB via LINQ statement over Entity Framework.

                    orderByString = orderByString +
                        (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                        + destinationPropety
                        + (orderDescending ? " descending" : " ascending");
                }
            }

            // This is the statement in which we implement 
            // System.Linq.Dynamic.Core.DynamicQueryableExtensions
            // which will replace OrderBy clause in the IQueriable<T> i.e. the source. 
            // In our case, "source" is nothing but the collection of itemzs that we
            // are going to receive from the database.

            return source.OrderBy(orderByString);
        }
    }
}
