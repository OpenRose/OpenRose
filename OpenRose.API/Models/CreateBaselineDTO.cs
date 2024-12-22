// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// CreateBaselineDTO shall be used for sending in request for creating new Baseline.
    /// It will expose necessary properties to allow successful creation of the Baseline.
    /// </summary>
    public class CreateBaselineDTO : ManipulateBaselineDTO
    {
        /// <summary>
        /// ProjectId is used to create new baseline under identified project.
        /// </summary>
        [Required]
        public Guid ProjectId { get; set; }
        /// <summary>
        /// ItemzTypeId is an optional property for which value shall be supplied 
        /// only when Baseline has to be created for a given ItemzType within 
        /// an identified Project. 
        /// </summary>
        public Guid ItemzTypeId { get; set; } = Guid.Empty;
    }
}
