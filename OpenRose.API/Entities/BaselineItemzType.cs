// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemzApp.API.Entities
{
    public class BaselineItemzType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid(); // If Database Guid is not provided as ID then a new one will be created by default.

        public Guid ItemzTypeId { get; set; }

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; }

        //[Required]
        [MaxLength(64)]
        public string? Status { get; set; }

		[Column(TypeName = "VARCHAR(MAX)")]
		public string? Description { get; set; }

        //[Required]
        [MaxLength(128)]
        public string? CreatedBy { get; set; } 

        //[Required]
        public DateTimeOffset? CreatedDate { get; set; }

        public List<BaselineItemzTypeJoinBaselineItemz>? BaselineItemzTypeJoinBaselineItemz { get; set; }

        public Baseline? Baseline { get; set; }

        [Required]
        public Guid BaselineId { get; set; }

        [Required]
        public bool IsSystem { get; set; }

    }
}
