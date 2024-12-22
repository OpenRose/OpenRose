﻿// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Models
{
    /// <summary>
    /// UpdateItemzDTO shall be used for sending in request for updating
    /// existing Itemz. It will expose necessary properties to allow existing Itemz 
    /// to be updated with new values for those properties.
    /// </summary>
    public class UpdateItemzDTO : ManipulateItemzDTO
    {

    }
}
