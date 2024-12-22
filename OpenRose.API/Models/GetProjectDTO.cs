// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// ProjectDTO is a POCO used for serving requests like GetProjects or GetProject by ProjectID.
    /// It will carry specified set of data that are exposed when project details are requested throgh "ItemzApp.API"
    /// </summary>
    public class GetProjectDTO
    {
        /// <summary>
        /// Id of the Project representated by a GUID.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Project Name 
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Status of the Project
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Description of the Project
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// User who created the Project
        /// </summary>
        public string? CreatedBy { get; set; }
        /// <summary>
        /// Date and Time when Project was created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
    }
}
