// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;

namespace ItemzApp.API.Profiles
{
    public class BaselineItemzProfile : Profile
    {
        public BaselineItemzProfile()
        {
            CreateMap<Entities.BaselineItemz, Models.GetBaselineItemzDTO>(); // Used for creating GetBaselineItemzDTO based on BaselineItemz object.
            CreateMap<Models.UpdateBaselineItemzDTO, Entities.UpdateBaselineItemz>(); // Used for creating UpdateBaselineItemz based on UpdateBaselineItemzDTO object.
        }
    }
}
