// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
	/// <summary>
	/// CopyItemzTypeDTO shall be used for sending in request for creating a copy of 
	/// ItemzType including all it's child Itemz Hierarchy.
	/// </summary>
	public class CopyItemzTypeDTO
	{
		/// <summary>
		/// Source ItemzType's Id for creating a Copy including all it's children.
		/// </summary>
		[Required]
		public Guid ItemzTypeId { get; set; }
	}
}
