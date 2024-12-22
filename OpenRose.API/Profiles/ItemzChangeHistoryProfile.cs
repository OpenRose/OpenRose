// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Profiles
{
    public class ItemzChangeHistoryProfile : Profile
    {
        public ItemzChangeHistoryProfile()
        {
            CreateMap<Entities.ItemzChangeHistory, Models.GetItemzChangeHistoryDTO>(); // Used for creating GetItemzChangeHistoryDTO based on ItemzChangeHistory object.
        }
    }
}
