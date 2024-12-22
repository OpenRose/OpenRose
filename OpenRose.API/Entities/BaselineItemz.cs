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

		[Column(TypeName = "VARCHAR(MAX)")]
		public string? Description { get; set; }

        //[Required]
        [MaxLength(128)]
        public string? CreatedBy { get; set; }

        //[Required]
        public DateTimeOffset? CreatedDate { get; set; }

        //[Required]
        [MaxLength(128)]
        public string? Severity { get; set; }

        public List<BaselineItemzTypeJoinBaselineItemz>? BaselineItemzTypeJoinBaselineItemz { get; set; }

        public virtual List<BaselineItemz>? BaselineFromItemzJoinItemzTrace { get; set; }

        public virtual List<BaselineItemz>? BaselineToItemzJoinItemzTrace { get; set; }

        public Guid IgnoreMeBaselineItemzTypeId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public bool isIncluded { get; set; } = true;

    }
}
