// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemzApp.API.Entities
{
    public class Itemz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid(); // If Database Guid is not provided as ID then a new one will be created by default.

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; }

        //[Required]
        //[MaxLength(64)]
        //public string Status { get; set; } = "New";



		// EXPLANATION: First we have introduced internal _itemzStatus field
		// which is internal private backing field for public Status Field.
		// Using Enum.Parse, we initialize default value for _itemzStatus.
		// Ref : https://www.csharp-examples.net/string-to-enum/

		private ItemzStatus _itemzStatus = (ItemzStatus)Enum.Parse(
			typeof(ItemzStatus),
			EntityPropertyDefaultValues.ItemzStatusDefaultValue,
			true);

		//[JsonConverter(typeof(StringEnumConverter))]
		[EnumDataType(typeof(ItemzStatus))]
		public ItemzStatus Status
		{
			get
			{
				return _itemzStatus;
			}
			// EXPLANATION: Status field is desgined to check if the passed in value is
			// present in the target ItemzStatus enum or not. If it's not then
			// it will throw ArgumentOutOfRangeException that is caught by 
			// automapper. It's being implemented based on following answer.
			// https://stackoverflow.com/a/35480269
			set
			{
				if (Enum.IsDefined(typeof(ItemzStatus), value))
					_itemzStatus = value;
				else
					// TODO: Below error that is thrown manually is not captured globally
					// by exception handling code. In the future, we have to take care of
					// making sure that we capture the error at global level.
					throw new ArgumentOutOfRangeException($"{value} not supported for field Status");
			}
		}



		//[MaxLength(64)]
		//public string? Priority { get; set; } = "Medium";




		// EXPLANATION: First we have introduced internal _itemzPriority field
		// which is internal private backing field for public Priority Field.
		// Using Enum.Parse, we initialize default value for _itemzPriority.
		// Ref : https://www.csharp-examples.net/string-to-enum/

		private ItemzPriority _itemzPriority = (ItemzPriority)Enum.Parse(
			typeof(ItemzPriority),
			EntityPropertyDefaultValues.ItemzPriorityDefaultValue,
			true);

		//[JsonConverter(typeof(StringEnumConverter))]
		[EnumDataType(typeof(ItemzPriority))]
		public ItemzPriority Priority
		{
			get
			{
				return _itemzPriority;
			}
			// EXPLANATION: Priority field is desgined to check if the passed in value is
			// present in the target ItemzPriority enum or not. If it's not then
			// it will throw ArgumentOutOfRangeException that is caught by 
			// automapper. It's being implemented based on following answer.
			// https://stackoverflow.com/a/35480269
			set
			{
				if (Enum.IsDefined(typeof(ItemzPriority), value))
					_itemzPriority = value;
				else
					// TODO: Below error that is thrown manually is not captured globally
					// by exception handling code. In the future, we have to take care of
					// making sure that we capture the error at global level.
					throw new ArgumentOutOfRangeException($"{value} not supported for field Priority");
			}
		}

		[Column(TypeName = "VARCHAR(MAX)")]
		public string? Description { get; set; }

        [Required]
        [MaxLength(128)]
        public string CreatedBy { get; set; } = "Some User";

        [Required]
        public DateTimeOffset CreatedDate { get; set; } = DateTime.Now ;

        // EXPLANATION: First we have introduced internal _itemzSeverity field
        // which is internal private backing field for public Severity Field.
        // Using Enum.Parse, we initialize default value for _itemzSeverity.
        // Ref : https://www.csharp-examples.net/string-to-enum/

        private ItemzSeverity _itemzSeverity = (ItemzSeverity)Enum.Parse(
            typeof(ItemzSeverity),
            EntityPropertyDefaultValues.ItemzSeverityDefaultValue,
            true);

        //[JsonConverter(typeof(StringEnumConverter))]
        [EnumDataType(typeof(ItemzSeverity))]
        public ItemzSeverity Severity
        {
            get
            {
                return _itemzSeverity;
            }
            // EXPLANATION: Severity field is desgined to check if the passed in value is
            // present in the target ItemzSeverity enum or not. If it's not then
            // it will throw ArgumentOutOfRangeException that is caught by 
            // automapper. It's being implemented based on following answer.
            // https://stackoverflow.com/a/35480269
            set
            {
                if (Enum.IsDefined(typeof(ItemzSeverity), value))
                    _itemzSeverity = value;
                else
                    // TODO: Below error that is thrown manually is not captured globally
                    // by exception handling code. In the future, we have to take care of
                    // making sure that we capture the error at global level.
                    throw new ArgumentOutOfRangeException($"{value} not supported for field Severity");
            }
        }

        // TODO: We are now replacing ProjeectJoinItemz with ItemzTypeJoinItemz.
        // So at some point we will have to remove ProjectJoinItemz from below.
        //public List<ProjectJoinItemz> ProjectJoinItemz { get; set; }

        public List<ItemzTypeJoinItemz>? ItemzTypeJoinItemz { get; set; }

        public virtual List<Itemz>? FromItemzJoinItemzTrace { get; set; }

        public virtual List<Itemz>? ToItemzJoinItemzTrace { get; set; }

    }
}
