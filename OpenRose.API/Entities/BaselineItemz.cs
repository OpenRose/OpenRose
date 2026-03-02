// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemzApp.API.Entities
{
    public class BaselineItemz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid(); // If Database Guid is not provided as ID then a new one will be created by default.

        public Guid ItemzId { get; set; }

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; }

        //[Required]
        [MaxLength(64)]
        public string? Status { get; set; }

        [MaxLength(64)]
        public string? Priority { get; set; }


		// TODO :: For the following Description field with type VARCHAR(MAX) supports ASCII and not unicode.
		// To support unicode we have to change it to NVARCHAR(MAX) or NVARCHAR(n) with specified length. 

		[Column(TypeName = "NVARCHAR(MAX)")]
		public string? Description { get; set; }

        //[Required]
        [MaxLength(128)]
        public string? CreatedBy { get; set; }

        //[Required]
        public DateTimeOffset? CreatedDate { get; set; }

        //[Required]
        [MaxLength(128)]
        public string? Severity { get; set; }

		// EXPLANATION: TAGGING SUPPORT FOR BASELINE ITEMZ
		// BaselineItemz inherit tags from the source Itemz they were created from.
		// This allows baselines to capture the tagged state of requirements at a specific point in time.
		// Tags are stored in the same format as Itemz (| delimiter).
		// When a baseline is created, tags from the source Itemz are copied to BaselineItemz.
		[Column(TypeName = "NVARCHAR(512)")]
		[MaxLength(512)]
		public string? Tags { get; set; }

		public List<BaselineItemzTypeJoinBaselineItemz>? BaselineItemzTypeJoinBaselineItemz { get; set; }

        public virtual List<BaselineItemz>? BaselineFromItemzJoinItemzTrace { get; set; }

        public virtual List<BaselineItemz>? BaselineToItemzJoinItemzTrace { get; set; }

        public Guid IgnoreMeBaselineItemzTypeId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public bool isIncluded { get; set; } = true;

    }
}
