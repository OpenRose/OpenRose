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
    public class ItemzChangeHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required]
        //[MaxLength(128)]
        //public string CreatedBy { get; set; } = "Some User";

        public Itemz? Itemz{ get; set; }

        [Required]
        public Guid ItemzId { get; set; }

        [Required]
        public DateTimeOffset CreatedDate { get; set; } = DateTime.Now;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        [Required]
        [MaxLength(128)]
        public string? ChangeEvent { get; set; }
    }
}
