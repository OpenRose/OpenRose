// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using ItemzApp.API.Entities;
using ItemzApp.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// ManipulateItemzDTO is used for updating Itemz
    /// </summary>
    [ItemzNameMustNotStartWithABC]
   
    public class ManipulateItemzDTO
    {
        /// <summary>
        /// Name or Title of the Itemz
        /// </summary>
        [Required] // Such attributes are important as they are used for Validating incoming API calls.
        [MaxLength(128)]
        public string? Name { get; set; }
        /// <summary>
        /// Status of the itemz
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? Status { get; set; }
        /// <summary>
        /// Priority of the Itemz
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? Priority { get; set; }
        /// <summary>
        /// Description of the Itemz
        /// </summary>
        [MaxLength(10280)]
        public string? Description { get; set; }
        /// <summary>
        /// Severity of the Itemz
        /// </summary>
        [MaxLength(128)]
        public string Severity { get; set; } = EntityPropertyDefaultValues.ItemzSeverityDefaultValue;

    	/// <summary>
		/// Tags property allows multiple tags to be applied to a requirement.
		/// Tags are provided as a delimited string using pipe (|) as separator.
		/// 
		/// Examples:
		/// - "April2026|A108|New York"
		/// - "Monday & Tuesday|Delivery_Completed|01-03-2026"
		/// 
		/// The property is marked as optional to maintain backward compatibility.
		/// Existing code that doesn't use tags can continue to work without modification.
		/// Tags are marked as nullable and optional in validation.
		/// </summary>
		[MaxLength(512)]
		public string? Tags { get; set; }
	}
}
