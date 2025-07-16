// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0.
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace ItemzApp.API.Models
{
	/// <summary>
	/// Form data for importing OpenRose JSON along with placement information.
	/// Used to bind multipart/form-data content for file upload + metadata.
	/// </summary>
	public class ImportFormDTO
	{
		/// <summary>
		/// JSON file exported from OpenRose to be imported.
		/// </summary>
		[Required]
		public IFormFile File { get; set; }

		/// <summary>
		/// The parent record under which the entity/entities will be imported.
		/// It's a required property to identify parent record.
		/// </summary>
		public Guid? TargetParentId { get; set; }

		/// <summary>
		/// If true, import occurs at the bottom of the parent's child list.
		/// Mutually exclusive with placement between two entities.
		/// </summary>
		public bool AtBottomOfChildNodes { get; set; } = true;

		/// <summary>
		/// For importing ItemzType: The ID of the first existing ItemzType between which to place the import.
		/// Must be paired with SecondItemzTypeId. Mutually exclusive with AtBottomOfChildNodes.
		/// </summary>
		public Guid? FirstItemzTypeId { get; set; }

		/// <summary>
		/// For importing ItemzType: The ID of the second existing ItemzType between which to place the import.
		/// Must be paired with FirstItemzTypeId. Mutually exclusive with AtBottomOfChildNodes.
		/// </summary>
		public Guid? SecondItemzTypeId { get; set; }

		/// <summary>
		/// For importing Itemz/SubItemz: The ID of the first existing Itemz between which to place the import.
		/// Must be paired with SecondItemzId. Mutually exclusive with AtBottomOfChildNodes.
		/// </summary>
		public Guid? FirstItemzId { get; set; }

		/// <summary>
		/// For importing Itemz/SubItemz: The ID of the second existing Itemz between which to place the import.
		/// Must be paired with FirstItemzId. Mutually exclusive with AtBottomOfChildNodes.
		/// </summary>
		public Guid? SecondItemzId { get; set; }
	}
}
