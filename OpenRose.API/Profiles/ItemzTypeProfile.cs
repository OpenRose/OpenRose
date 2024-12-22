// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;

namespace ItemzApp.API.Profiles
{
    public class ItemzTypeProfile : Profile
    {
        public ItemzTypeProfile()
        {
            CreateMap<Entities.ItemzType, Models.GetItemzTypeDTO>(); // Used for creating GetItemzTypeDTO based on ItemzType object.
            CreateMap<Models.CreateItemzTypeDTO, Entities.ItemzType>(); // Used for creating ItemzType based on CreateItemzTypeDTO object.
            CreateMap<Models.UpdateItemzTypeDTO, Entities.ItemzType>(); // Used for updating ItemzType based on UpdateItemzTypeDTO object.
            CreateMap<Entities.ItemzType, Models.UpdateItemzTypeDTO>();  // Used for updating UpdateItemzTypeDTO based on ItemzType object.
        }
    }
}
