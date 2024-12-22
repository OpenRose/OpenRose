// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// Itemz class containing several properties that represents it.
    /// This Itemz class is returned by the "ItemzApp.API" in most of the cases when
    /// user sends request to read an Itemz record.
    /// </summary>
    public class GetItemzWithBasePropertiesDTO
    {
        /// <summary>
        /// Id of the Itemz representated by a GUID.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name or Title of the Itemz
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Status of the itemz
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Priority of the Itemz
        /// </summary>
        public string? Priority { get; set; }
        /// <summary>
        /// Description of the Itemz
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
        /// <summary>
        /// Severity of the Itemz
        /// </summary>
        public string? Severity { get; set; }
    }
}
