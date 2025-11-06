// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemzApp.API.Entities
{
    public class Baseline
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid(); // If Database Guid is not provided as ID then a new one will be created by default.

        [Required]
        [MaxLength(128)]
        public string? Name { get; set; }

		// TODO :: For the following Description field with type VARCHAR(MAX) supports ASCII and not unicode.
		// To support unicode we have to change it to NVARCHAR(MAX) or NVARCHAR(n) with specified length. 

		[Column(TypeName = "NVARCHAR(MAX)")]
		public string? Description { get; set; }

        [Required]
        [MaxLength(128)]
        public string CreatedBy { get; set; } = "Some User";

        [Required]
        public DateTimeOffset CreatedDate { get; set; } = DateTime.Now;

        public Project? Project { get; set; }

        [Required]
        public Guid ProjectId { get; set; }
        
        [NotMapped]
        public Guid ItemzTypeId { get; set; }

        public List<BaselineItemzType>? BaselineItemzTypes { get; set; }
    }
}
