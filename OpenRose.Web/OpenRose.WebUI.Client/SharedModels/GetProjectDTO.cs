// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace OpenRose.WebUI.Client.SharedModels
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


		//public string IdAsString { get; set; }
		//public Guid Id
		//{
		//	get { return Guid.TryParse(IdAsString, out Guid g) ? g : default; }
		//	set { IdAsString = Convert.ToString(value); }
		//}
		
        
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
