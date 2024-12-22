// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Models;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ItemzApp.API.ValidationAttributes
{
    public class ItemzNameMustNotStartWithABCAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value,
            ValidationContext validationContext)
        {
            var itemz = value as ManipulateItemzDTO;

            // TODO: THIS validation rule was configured just to learn and test if it's working as expected.
            // it should be removed later on.

            if(itemz?.Name is null)
            {
                return ValidationResult.Success;
            }

            if (itemz.Name.ToUpper().StartsWith("ABC"))
            {
                return new ValidationResult("Name should never start with keyword like ABC! Issue found with item having name - '" + itemz.Name + "'",
                    new[] { nameof(ManipulateItemzDTO) });
            }
            return ValidationResult.Success;
        }
    }
}
