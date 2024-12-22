// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;

namespace OpenRose.WebUI.Client.SharedModels
{
    public class GetNumberOfChangeHistoryByRepositoryDTO
    {
        /// <summary>
        /// Date and Time upto which Itemz Change History data has to be deleted for given ItemzId.
        /// </summary>
        public DateTimeOffset UptoDateTime { get; set; }
    }
}
