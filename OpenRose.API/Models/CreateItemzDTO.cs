// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// CreateItemzDTO shall be used for sending in request for creating new Itemz. 
    /// It will expose necessary properties to allow successful creation of new Itemz.
    /// </summary>
    public class CreateItemzDTO  : ManipulateItemzDTO // : IValidatableObject // INSTEAD using ItemzNameMustNotStartWithABC attribute
    {


        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    // Instead of inline validation in the class we are now validating via 
        //    // custom attribute called as ItemzNameMustNotStartWithABC 
        //    // again this one is just for learning purposes.
        //    // I had to comment out inheritance of this class from IValidatableObject interface.
        //    // as we are not implementing validation via inline method in the class itself.
        //
        //    if (Name.ToUpper().StartsWith("ABC"))
        //    {
        //        yield return new ValidationResult("Name should never start with keyworkd like ABC!",
        //            new[] { "CreateItemzDTO" });
        //    }
        //}
    }
}
