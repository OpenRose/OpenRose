// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;

namespace ItemzApp.API.Profiles
{
    public class BaselineProfile : Profile
    {
        public BaselineProfile()
        {
            CreateMap<Entities.Baseline, Models.GetBaselineDTO>(); // Used for creating GetBaselineDTO based on Baseline object.
            CreateMap<Models.CreateBaselineDTO, Entities.Baseline>(); // Used for creating Baseline based on CreateBaselineDTO object.
            CreateMap<Models.UpdateBaselineDTO, Entities.Baseline>(); // Used for updating Baseline based on UpdateBaselineDTO object.
            CreateMap<Entities.Baseline, Models.UpdateBaselineDTO>();  // Used for updating UpdateBaselineDTO based on Baseline object.
            CreateMap<Models.CloneBaselineDTO, Entities.NonEntity_CloneBaseline>(); // Used for creating new baseline by cloning existing one.
        }
    }
}
