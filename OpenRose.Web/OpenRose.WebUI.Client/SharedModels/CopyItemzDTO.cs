// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.ComponentModel.DataAnnotations;

namespace OpenRose.WebUI.Client.SharedModels
{
	/// <summary>
	/// CopyItemzDTO shall be used for sending in request for creating a copy of 
	/// Requirements i.e. Itemz including all it's child Itemz.
	/// </summary>
	public class CopyItemzDTO
	{
		/// <summary>
		/// Source Itemz's Id for creating a Copy including all it's children.
		/// </summary>
		[Required]
		public Guid ItemzId { get; set; }
	}
}
