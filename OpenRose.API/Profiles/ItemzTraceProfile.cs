// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;

namespace ItemzApp.API.Profiles
{
    public class ItemzTraceProfile : Profile
    {
        public ItemzTraceProfile()
        {
            CreateMap<Entities.ItemzJoinItemzTrace, Models.ItemzTraceDTO>()
                .ForMember(itDTO => itDTO.FromTraceItemzId, ijit => ijit.MapFrom(ijit => ijit.FromItemzId))
                .ForMember(itDTO => itDTO.ToTraceItemzId, ijit => ijit.MapFrom(ijit => ijit.ToItemzId)) // Used for creating ItemzTraceDTO based on ItemzTrace object.
                .ForMember(itDTO => itDTO.TraceLabel, ijit => ijit.MapFrom(ijit => ijit.TraceLabel)); // Map TraceLabel
		    // CreateMap<Models.ItemzTraceDTO, Entities.ItemzJoinItemzTrace>() // Because ItemzJoinItemzTrace contains full Itemz object, I would never expect it to be created from the DTO. 
		}
    }
}
