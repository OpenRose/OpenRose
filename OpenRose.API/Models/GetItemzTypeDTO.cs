// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// ItemzTypeDTO is a POCO used for serving requests like GetItemzTypes or GetItemzType by ItemzTypeID.
    /// It will carry specified set of data that are exposed when ItemzType details are requested throgh "ItemzApp.API"
    /// </summary>
    public class GetItemzTypeDTO
    {
        /// <summary>
        /// Id of the ItemzType representated by a GUID.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// ItemzType Name 
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Status of the ItemzType
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Description of the ItemzType
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// User who created the ItemzType
        /// </summary>
        public string? CreatedBy { get; set; }
        /// <summary>
        /// Date and Time when ItemzType was created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
        /// <summary>
        /// Returns true if it's system ItemzType otherwise false
        /// </summary>
        public bool IsSystem { get; set; }
    }
}
