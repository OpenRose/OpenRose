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
    public class ItemzProfile : Profile
    {
        public ItemzProfile ()
        {
            CreateMap<Entities.Itemz, Models.GetItemzDTO>()
                .ForMember(dto => dto.Severity, i => i.MapFrom(o => o.Severity));    // Used for creating GetItemzDTO based on Itemz object.
            CreateMap<Models.CreateItemzDTO, Entities.Itemz>()
                .ForMember(i => i.Severity , dto => dto.MapFrom(d => d.Severity)); // Used for creating Itemz based on CreateItemzDTO object.
            CreateMap<Models.UpdateItemzDTO, Entities.Itemz>()
                .ForMember(i => i.Severity, dto => dto.MapFrom(d => d.Severity));// Used for updating Itemz based on UpdateItemzDTO object.
            CreateMap<Entities.Itemz,Models.UpdateItemzDTO>()
                .ForMember(dto => dto.Severity, i => i.MapFrom(o => o.Severity));// Used for updating UpdateItemzDTO based on Itemz object.
            CreateMap<Models.GetItemzWithBasePropertiesDTO, Models.GetItemzWithBasePropertiesDTO>()
                            .ForMember(dto => dto.Severity, i => i.MapFrom(o => o.Severity));    // Used for creating GetItemzWithBasePropertiesDTO based on Custom Itemz object mainly used for Sorting Paginated Orphand Itemz.
        }
    }
}
