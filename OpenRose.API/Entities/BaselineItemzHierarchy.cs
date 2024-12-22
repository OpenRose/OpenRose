// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemzApp.API.Entities
{
    public class BaselineItemzHierarchy
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string? RecordType { get; set; }  // TODO: Make it possible to use predefined enum list instead of passing in text. 

		[MaxLength(128)]
		public string? Name { get; set; }
		public HierarchyId? BaselineItemzHierarchyId { get; set; }

        public HierarchyId? SourceItemzHierarchyId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public bool isIncluded { get; set; } = true;
    }
}
