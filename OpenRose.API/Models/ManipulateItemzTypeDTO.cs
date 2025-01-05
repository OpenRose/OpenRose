// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// ManipulateItemzTypeDTO is used for updating ItemzType
    /// </summary>
    public class ManipulateItemzTypeDTO
    {
        /// <summary>
        /// Name of the ItemzType
        /// </summary>
        [Required] // Such attributes are important as they are used for Validating incoming API calls.
        [MaxLength(128)]
        public string? Name { get; set; }
        /// <summary>
        /// Status of the ItemzType
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? Status { get; set; }
        /// <summary>
        /// Description of the ItemzType
        /// </summary>
        [MaxLength(10280)]
        public string? Description { get; set; }
    }
}
