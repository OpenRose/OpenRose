// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Threading.Tasks;
using System.Collections.Generic;
using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.Project
{
    public interface IProjectService
    {
        public Task<GetProjectDTO> __Single_Project_By_GUID_ID__Async(Guid projectId);

        public Task<ICollection<GetProjectDTO>> __GET_Projects__Async();

        public Task<GetProjectDTO?> __POST_Create_Project__Async(GetProjectDTO updateProjectDTO);

        public Task<GetProjectDTO?> __PUT_Update_Project_By_GUID_ID__Async(Guid projectId, GetProjectDTO updateProjectDTO);

        public Task<int?> __GET_Itemz_Count_By_Project__Async(Guid projectId);

        public Task<ICollection<GetItemzTypeDTO>?> __GET_ItemzTypes_By_Project__Async(Guid projectId);

        public Task<string?> __GET_Last_Project_HierarchyID__Async();

        public Task __DELETE_Project_By_GUID_ID__Async(Guid projectId);
        ///// <summary>
        ///// Gets collection of Projects
        ///// </summary>
        ///// <returns>Returns collection of Projects based on sorting order</returns>
        ///// <exception cref="ApiException">A server side error occurred.</exception>
        //System.Threading.Tasks.Task<System.Collections.Generic.ICollection<GetProjectDTO>> __GET_Projects__Async();

        ///// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        ///// <summary>
        ///// Gets collection of Projects
        ///// </summary>
        ///// <returns>Returns collection of Projects based on sorting order</returns>
        ///// <exception cref="ApiException">A server side error occurred.</exception>
        //System.Threading.Tasks.Task<System.Collections.Generic.ICollection<GetProjectDTO>> __GET_Projects__Async(System.Threading.CancellationToken cancellationToken);

    }
}
