// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Project
{

    public class DummyProjectService : IProjectService
    {
        public Task<string> GetProjectData()
        {
            throw new InvalidOperationException("OpenRose API base URL is not configured. Please provide a valid URL in the configuration file.");
        }

		public Task<GetProjectDTO> __POST_Copy_Project_By_GUID_ID__Async(CopyProjectDTO body)
		{
			throw new NotImplementedException();
		}

		Task IProjectService.__DELETE_Project_By_GUID_ID__Async(Guid projectId)
        {

            throw new NotImplementedException();
        }

        Task<ICollection<GetItemzTypeDTO>?> IProjectService.__GET_ItemzTypes_By_Project__Async(Guid projectId)
        {
            throw new NotImplementedException();
        }

        Task<int?> IProjectService.__GET_Itemz_Count_By_Project__Async(Guid projectId)
        {
            throw new NotImplementedException();
        }

        Task<string?> IProjectService.__GET_Last_Project_HierarchyID__Async()
        {
            throw new NotImplementedException();
        }

        Task<ICollection<GetProjectDTO>> IProjectService.__GET_Projects__Async()
        {
            throw new NotImplementedException();
        }

        Task<GetProjectDTO?> IProjectService.__POST_Create_Project__Async(GetProjectDTO updateProjectDTO)
        {
            throw new NotImplementedException();
        }

        Task<GetProjectDTO?> IProjectService.__PUT_Update_Project_By_GUID_ID__Async(Guid projectId, GetProjectDTO updateProjectDTO)
        {
            throw new NotImplementedException();
        }

        Task<GetProjectDTO> IProjectService.__Single_Project_By_GUID_ID__Async(Guid projectId)
        {
            throw new NotImplementedException();
        }
    }
}
