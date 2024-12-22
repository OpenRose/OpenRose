// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;

namespace ItemzApp.API.Profiles
{
    public class BaselineItemzTypeProfile : Profile
    {
        public BaselineItemzTypeProfile()
        {
            CreateMap<Entities.BaselineItemzType, Models.GetBaselineItemzTypeDTO>(); // Used for creating GetBaselineItemzTypeDTO based on BaselineItemzType object.
        }
    }
}
