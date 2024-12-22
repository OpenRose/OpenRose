// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;

namespace ItemzApp.API.Profiles
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<Entities.Project, Models.GetProjectDTO>(); // Used for creating GetProjectDTO based on Project object.
            CreateMap<Models.CreateProjectDTO, Entities.Project>(); // Used for creating Project based on CreateProjectDTO object.
            CreateMap<Models.UpdateProjectDTO, Entities.Project>(); // Used for updating Project based on UpdateProjectDTO object.
            CreateMap<Entities.Project, Models.UpdateProjectDTO>();  // Used for updating UpdateProjectDTO based on Project object.
        }
    }
}
