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
    }
}
