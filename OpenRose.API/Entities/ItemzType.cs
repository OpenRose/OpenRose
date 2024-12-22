// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemzApp.API.Entities
{
    public class ItemzType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid(); // If Database Guid is not provided as ID then a new one will be created by default.

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; }

		//[Required]
		//[MaxLength(64)]
		//public string Status { get; set; } = "Active";


		// EXPLANATION: First we have introduced internal _itemzTypeStatus field
		// which is internal private backing field for public Status Field.
		// Using Enum.Parse, we initialize default value for _itemzTypeStatus.
		// Ref : https://www.csharp-examples.net/string-to-enum/

		private ItemzTypeStatus _itemzTypeStatus = (ItemzTypeStatus)Enum.Parse(
			typeof(ItemzTypeStatus),
			EntityPropertyDefaultValues.ItemzTypeStatusDefaultValue,
			true);

		//[JsonConverter(typeof(StringEnumConverter))]
		[EnumDataType(typeof(ItemzTypeStatus))]
		[Required]
		[MaxLength(64)]
		public ItemzTypeStatus Status
		{
			get
			{
				return _itemzTypeStatus;
			}
			// EXPLANATION: Status field is desgined to check if the passed in value is
			// present in the target ItemzTypeStatus enum or not. If it's not then
			// it will throw ArgumentOutOfRangeException that is caught by 
			// automapper. It's being implemented based on following answer.
			// https://stackoverflow.com/a/35480269
			set
			{
				if (Enum.IsDefined(typeof(ItemzTypeStatus), value))
					_itemzTypeStatus = value;
				else
					// TODO: Below error that is thrown manually is not captured globally
					// by exception handling code. In the future, we have to take care of
					// making sure that we capture the error at global level.
					throw new ArgumentOutOfRangeException($"{value} not supported for field Status");
			}
		}

		[Column(TypeName = "VARCHAR(MAX)")]
		public string? Description { get; set; }

        [Required]
        [MaxLength(128)]
        public string CreatedBy { get; set; } = "Some User";

        [Required]
        public DateTimeOffset CreatedDate { get; set; } = DateTime.Now;

        public List<ItemzTypeJoinItemz>? ItemzTypeJoinItemz { get; set; }

        public Project? Project { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public bool IsSystem { get; set; } = false;

    }
}
