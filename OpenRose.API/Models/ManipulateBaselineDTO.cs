// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// ManipulateBaselineDTO is used for updating Baseline
    /// </summary>
  
    public class ManipulateBaselineDTO
    {
        /// <summary>
        /// Name or Title of the Baseline
        /// </summary>
        [Required] // Such attributes are important as they are used for Validating incoming API calls.
        [MaxLength(128)]
        public string? Name { get; set; }
        /// <summary>
        /// Description of the Baseline
        /// </summary>
        [MaxLength(10280)]
        public string? Description { get; set; }
    }
}
