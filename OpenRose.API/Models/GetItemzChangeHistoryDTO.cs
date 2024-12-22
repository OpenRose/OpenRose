// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// Itemz Change History class containing details about changes loggged against Itemz.
    /// This Itemz Change History class is identified by Itemz ID against which changes are registered.
    /// </summary>
    public class GetItemzChangeHistoryDTO
    {
        /// <summary>
        /// itemzId of the Itemz representated by a GUID.
        /// </summary>
        public Guid ItemzId { get; set; }
        /// <summary>
        /// Date and Time when Itemz Change History was created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
        /// <summary>
        /// Old value of the Itemz Change History log
        /// </summary>
        public string OldValues { get; set; } = "";
        /// <summary>
        /// New value of the Itemz Change History log
        /// </summary>
        public string NewValues { get; set; } = "";
        /// <summary>
        /// Actual event that triggered registration of Itemz Change History, either Added or Modified.
        /// </summary>
        public string? ChangeEvent { get; set; }
    }
}
