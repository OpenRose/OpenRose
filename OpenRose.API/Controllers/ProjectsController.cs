// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ItemzApp.API.Models;
using ItemzApp.API.ResourceParameters;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ItemzApp.API.BusinessRules.Project;
using ItemzApp.API.Helper;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    //[Route("api/Project")]
    [Route("api/[controller]")] // e.g. http://HOST:PORT/api/itemzs/Projects
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
		private readonly IHierarchyRepository _hierarchyRepository;
		private readonly IMapper _mapper;
        // private readonly IPropertyMappingService _propertyMappingService;
        private readonly ILogger<ProjectsController> _logger;
        private readonly IProjectRules _projectRules;
        public ProjectsController(IProjectRepository projectRepository,
                                 IHierarchyRepository hierarchyRepository,
                                 IMapper mapper,
                                 //IPropertyMappingService propertyMappingService,
                                 ILogger<ProjectsController> logger,
                                 IProjectRules projectRules)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
			_mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            //_propertyMappingService = propertyMappingService ??
            //    throw new ArgumentNullException(nameof(propertyMappingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectRules = projectRules ?? throw new ArgumentNullException(nameof(projectRules));


        }

        /// <summary>
        /// Get a Project by ID (represented by a GUID)
        /// </summary>
        /// <param name="ProjectId">GUID representing an unique ID of the Project that you want to get</param>
        /// <returns>A single Project record based on provided ID (GUID) </returns>
        /// <response code="200">Returns the requested Project</response>
        /// <response code="404">Requested Project not found</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetProjectDTO))]
        [HttpGet("{ProjectId:Guid}",
            Name = "__Single_Project_By_GUID_ID__")] // e.g. http://HOST:PORT/api/Projects/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("{ProjectId:Guid}", Name = "__HEAD_Project_By_GUID_ID__")]
        public async Task<ActionResult<GetProjectDTO>> GetProjectAsync(Guid ProjectId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get Project for ID {ProjectId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                ProjectId);
            var projectFromRepo = await _projectRepository.GetProjectAsync(ProjectId);

            if (projectFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Project for ID {ProjectId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    ProjectId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Found Project for ID {ProjectId} and now returning results",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                ProjectId);
            return Ok(_mapper.Map<GetProjectDTO>(projectFromRepo));
        }



        /// <summary>
        /// Gets collection of Projects
        /// </summary>
        /// <returns>Collection of Projects based on expectated sorting order.</returns>
        /// <response code="200">Returns collection of Projects based on sorting order</response>
        /// <response code="404">No Projects were found</response>
        [HttpGet(Name = "__GET_Projects__")]
        [HttpHead(Name = "__HEAD_Projects_Collection__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetProjectDTO>>> GetProjectsAsync(
            //[FromQuery] ItemzResourceParameter itemzResourceParameter
            )
        {
            //if (!_propertyMappingService.ValidMappingExistsFor<GetItemzDTO, Itemz>
            //    (itemzResourceParameter.OrderBy))
            //{
            //    _logger.LogWarning("Requested Order By Field {OrderByFieldName} is not found. Property Validation Failed!", itemzResourceParameter.OrderBy);
            //    return BadRequest();
            //}

            var projectsFromRepo = await _projectRepository.GetProjectsAsync();
            if (projectsFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Projects found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return NotFound();
            }
            // _logger.LogDebug("In total {ProjecftsNumbers} Itemz found in the repository", projectsFromRepo.Count());
            //var previousPageLink = projectsFromRepo.HasPrevious ?
            //    CreateItemzResourceUri(itemzResourceParameter,
            //    ResourceUriType.PreviousPage) : null;

            //var nextPageLink = projectsFromRepo.HasNext ?
            //    CreateItemzResourceUri(itemzResourceParameter,
            //    ResourceUriType.NextPage) : null;

            //var paginationMetadata = new
            //{
            //    totalCount = projectsFromRepo.TotalCount,
            //    pageSize = projectsFromRepo.PageSize,
            //    currentPage = projectsFromRepo.CurrentPage,
            //    totalPages = projectsFromRepo.TotalPages,
            //    previousPageLink,
            //    nextPageLink
            //};

            // EXPLANATION : it's possible to send customer headers in the response.
            // So, before we hit 'return Ok...' statement, we can build our
            // own response header as you can see in following example.

            // TODO: Check if just passsing the header is good enough. How can we
            // document it so that consumers can use it effectively. Also, 
            // how to implement versioning of headers so that we don't break
            // existing applications using the headers after performing upgrade
            // in the future.

            //Response.Headers.Add("X-Pagination",
            //    JsonConvert.SerializeObject(paginationMetadata));

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {ProjectNumbers} Projects to the caller",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                projectsFromRepo.Count());
            return Ok(_mapper.Map<IEnumerable<GetProjectDTO>>(projectsFromRepo));
        }



        /// <summary>
        /// Used for creating new Project record in the database
        /// </summary>
        /// <param name="createProjectDTO">Used for populating information in the newly created Project in the database</param>
        /// <returns>Newly created Project property details</returns>
        /// <response code="201">Returns newly created Projects property details</response>
        /// <response code="409">Project with the same name already exists in the repository</response>

        [HttpPost(Name = "__POST_Create_Project__")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<GetProjectDTO>> CreateProjectAsync(CreateProjectDTO createProjectDTO)
        {
            var projectEntity = _mapper.Map<Entities.Project>(createProjectDTO);

            if (await _projectRules.UniqueProjectNameRuleAsync(createProjectDTO.Name!))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Project with name {createProjectDTO_Name} already exists in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    createProjectDTO.Name);
                return Conflict($"Project with name '{createProjectDTO.Name}' already exists in the repository");

            }
            //if ((_projectRepository.HasProjectWithName(createProjectDTO.Name.Trim().ToLower())))
            //{
            //    _logger.LogDebug("Project with name \"{projectEntityName}\" already exists in the repository", projectEntity.Name);
            //    return Conflict($"Project with name '{projectEntity.Name}' already exists in the repository");
            //}

            try
            {
                _projectRepository.AddProject(projectEntity);
                await _projectRepository.SaveAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add new project:" + dbUpdateException.InnerException,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"Project with name '{projectEntity.Name}' already exists in the repository");
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Created new Project with ID {ProjectId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                projectEntity.Id);

            // TODO: Try and Catch logic here is not clear and it might add project
            // in the DB even if adding hierarchy record fails. In such cases 
            // we need both this steps to be included in one single transaction. 
            // If there is an issue to add Project into hierarchy table then we will not be
            // able to work with it's ItemzType and Itemz which are expected to be childrens.

            try
            {
                await _projectRepository.AddNewProjectHierarchyAsync(projectEntity);
                await _projectRepository.SaveAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add new project hierarchy:" + dbUpdateException.InnerException,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"Could not add hierarchy for newly created project '{projectEntity.Name}' ");
            }


            return CreatedAtRoute("__Single_Project_By_GUID_ID__", new { ProjectId = projectEntity.Id },
                _mapper.Map<GetProjectDTO>(projectEntity) // Converting to DTO as this is going out to the consumer
                );
        }

        /// <summary>
        /// Updating exsting Project based on Project Id (GUID)
        /// </summary>
        /// <param name="projectId">GUID representing an unique ID of the Project that you want to get</param>
        /// <param name="projectToBeUpdated">required Project properties to be updated</param>
        /// <returns>No contents are returned but only Status 204 indicating that Project was updated successfully </returns>
        /// <response code="204">No content are returned but status of 204 indicated that Project was successfully updated</response>
        /// <response code="404">Project based on projectId was not found</response>
        /// <response code="409">Project with updated name already exists in the repository</response>

        [HttpPut("{projectId}", Name = "__PUT_Update_Project_By_GUID_ID")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> UpdateProjectPutAsync(Guid projectId, UpdateProjectDTO projectToBeUpdated)
        {
            if (!(await _projectRepository.ProjectExistsAsync(projectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Update request for Project for ID {ProjectId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    projectId);
                return NotFound();
            }

            var projectFromRepo = await _projectRepository.GetProjectForUpdateAsync(projectId);

            if (projectFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Update request for Project for ID {ProjectId} could not be found in the Repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    projectId);
                return NotFound();
            }

            if (await _projectRules.UniqueProjectNameRuleAsync(projectToBeUpdated.Name!, projectFromRepo.Name))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Project with name {projectToBeUpdated_Name} already exists in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    projectToBeUpdated.Name);
                return Conflict($"Project with name '{projectToBeUpdated.Name}' already exists in the repository");
            }

            _mapper.Map(projectToBeUpdated, projectFromRepo);
            try 
            { 
            _projectRepository.UpdateProject(projectFromRepo);
            await _projectRepository.SaveAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add new project:" + dbUpdateException.InnerException,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"Project with name '{projectToBeUpdated.Name}' already exists in the repository");
            }

            // EXPLANATION :: as part of updating Project record, we are making sure that project name is updated in two places.
            // First in the project record itself and secondly within ItemzHierarchy record as well. We are not going to update
            // BaselineItemzHierarchy record with updated project name as it's a snapshot of data from a given point in time.
            
            // TODO :: We should update Project and ItemzHierarchy together rather then two separate transactions

            try
            {
               var _discard = _hierarchyRepository.UpdateHierarchyRecordNameByID(projectFromRepo.Id, projectFromRepo.Name ?? "");
			}
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to update project name in ItemzHierarchy :" + dbUpdateException.InnerException,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"Name of ItemzHierarchy record for Project with ID {projectFromRepo.Id} could not be updated.");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Project for ID {ProjectId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				projectId);

			return NoContent(); // This indicates that update was successfully saved in the DB.
        }

        /// <summary>
        /// Partially updating a single **Project**
        /// </summary>
        /// <param name="projectId">Id of the Project representated by a GUID.</param>
        /// <param name="projectPatchDocument">The set of operations to apply to the Project via JsonPatchDocument</param>
        /// <returns>an ActionResult of type Project</returns>
        /// <response code="204">No content are returned but status of 204 indicated that Project was successfully updated</response>
        /// <response code="404">Project based on projectId was not found</response>
        /// <response code="409">Project with updated name already exists in the repository</response>
        /// <response code="422">Validation problems occured during analyzing validation rules for the JsonPatchDocument </response>
        /// <remarks> Sample request (this request updates an **Project's name**)   
        /// Documentation regarding JSON Patch can be found at 
        /// *[ASP.NET Core - JSON Patch Operations](https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-3.1#operations)* 
        /// 
        ///     PATCH /api/Projects/{id}  
        ///     [  
        ///	        {   
        ///             "op": "replace",   
        ///             "path": "/name",   
        ///             "value": "PATCH Updated Name field"  
        ///	        }   
        ///     ]
        /// </remarks>

        [HttpPatch("{projectId}", Name = "__PATCH_Update_Project_By_GUID_ID")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> UpdateProjectPatchAsync(Guid projectId, JsonPatchDocument<UpdateProjectDTO> projectPatchDocument)
        {
            if (!(await _projectRepository.ProjectExistsAsync(projectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Update request for Project for ID {ProjectId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    projectId);
                return NotFound();
            }

            var projectFromRepo = await _projectRepository.GetProjectForUpdateAsync(projectId);

            if (projectFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Update request for Project for ID {ProjectId} could not be found in the Repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    projectId);
                return NotFound();
            }

            var projectToPatch = _mapper.Map<UpdateProjectDTO>(projectFromRepo);

            projectPatchDocument.ApplyTo(projectToPatch, ModelState);

            // Validating Project patch document and verifying that it meets all the 
            // validation rules as expected. This will check if the data passed in the Patch Document
            // is ready to be saved in the db.

            if (!TryValidateModel(projectToPatch))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Project Properties did not pass defined Validation Rules for ID {ProjectId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    projectId);
                return ValidationProblem(ModelState);
            }
            if (await _projectRules.UniqueProjectNameRuleAsync(projectToPatch.Name!, projectFromRepo.Name))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Project with name {projectToPatch_Name} already exists in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    projectToPatch.Name);
                return Conflict($"Project with name '{projectToPatch.Name}' already exists in the repository");
            }

            _mapper.Map(projectToPatch, projectFromRepo);
            try
            {
                _projectRepository.UpdateProject(projectFromRepo);
                await _projectRepository.SaveAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add new project:" + dbUpdateException.InnerException,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"Project with name '{projectToPatch.Name}' already exists in the repository");
            }

			// EXPLANATION :: as part of updating Project record, we are making sure that project name is updated in two places.
			// First in the project record itself and secondly within ItemzHierarchy record as well. We are not going to update
			// BaselineItemzHierarchy record with updated project name as it's a snapshot of data from a given point in time.

			// TODO :: We should update Project and ItemzHierarchy together rather then two separate transactions

			try
			{
				var _discard = _hierarchyRepository.UpdateHierarchyRecordNameByID(projectFromRepo.Id, projectFromRepo.Name ?? "");
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to update project name in ItemzHierarchy :" + dbUpdateException.InnerException,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"Name of ItemzHierarchy record for Project with ID {projectFromRepo.Id} could not be updated.");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Project for ID {ProjectId} processed successfully",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                projectId);
            return NoContent();
        }

        // We have configured in startup class our own custom implementation of 
        // problem Details. Now we are overriding ValidationProblem method that is defined in ControllerBase
        // class to make sure that we use that custom problem details builder. 
        // Instead of passing 400 it will pass back 422 code with more details.
        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }


        /// <summary>
        /// Deleting a specific Project
        /// </summary>
        /// <param name="projectId">GUID representing an unique ID of the Project that you want to get</param>
        /// <returns>Status code 204 is returned without any content indicating that deletion of the specified Project was successful</returns>
        /// <response code="404">Project based on projectId was not found</response>
        [HttpDelete("{projectId}", Name = "__DELETE_Project_By_GUID_ID__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteProjectAsync(Guid projectId)
        {
            if (!(await _projectRepository.ProjectExistsAsync(projectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot Delete Project with ID {ProjectId} as it could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    projectId);
                return NotFound();
            }

            var projectFromRepo = await _projectRepository.GetProjectForUpdateAsync(projectId);

            if (projectFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot Delete Project with ID {ProjectId} as it could not be found in the Repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    projectId);
                return NotFound();
            }

            _projectRepository.DeleteProject(projectFromRepo);
            await _projectRepository.SaveAsync();

            _logger.LogDebug("{FormattedControllerAndActionNames}Delete request for Project with ID {ProjectId} processed successfully",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                projectId);

            var projectItemzHierarchyDeletionSuccessStatus = await _projectRepository.DeleteProjectItemzHierarchyAsync(projectId);

            if (!projectItemzHierarchyDeletionSuccessStatus)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Delete ItemzHierarchy records for Project with ID {ProjectId} process failed",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    projectId);
            }

            await _projectRepository.DeleteOrphanedBaselineItemzAsync();

            return NoContent();
        }

        /// <summary>
        /// Get total number of Itemz by Project
        /// </summary>
        /// <param name="projectId">Provide ProjectID representated in GUID form</param>
        /// <returns>Number of Itemz found for the given ProjectID. Zero if none found.</returns>
        /// <response code="200">Returns number of Itemz count that were associated with a given Project</response>
        /// <response code="404">Project based on projectId was not found</response>
        [HttpGet("GetItemzCount/{ProjectId:Guid}", Name = "__GET_Itemz_Count_By_Project__")]
        [HttpHead("GetItemzCount/{ProjectId:Guid}", Name = "__HEAD_Itemz_Count_By_Project__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetItemzCountByProjectAsync(Guid projectId)
        {
            if (!(await _projectRepository.ProjectExistsAsync(projectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find count of Itemz as Project with ID {ProjectId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    projectId);
                return NotFound();
            }

            var foundItemzCountByProjectId = await _projectRepository.GetItemzCountByProjectAsync(projectId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundItemzCountByProjectId} Itemz records for Project with ID {projectId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundItemzCountByProjectId,
                projectId);
            return foundItemzCountByProjectId;
        }


        /// <summary>
        /// Gets collection of ItemzTypes for the given ProjectID
        /// </summary>
        /// <returns>Collection of ItemzTypes based on sorting order for the given ProjectID</returns>
        /// <response code="200">Returns collection of ItemzTypes based on sorting order for the given ProjectID</response>
        /// <response code="404">No ItemzTypes were found for the given ProjectID</response>

        [HttpGet("GetItemzTypes/{ProjectId:Guid}", Name = "__GET_ItemzTypes_By_Project__")] // e.g. http://HOST:PORT/api/Projects/GetItemzTypes/42f62a6c-fcda-4dac-a06c-406ac1c17770

        [HttpHead("GetItemzTypes/{ProjectId:Guid}", Name = "__HEAD_ItemzTypes_By_Project__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetItemzTypeDTO>>> GetItemzTypesByProjectIdAsync(Guid ProjectId)
        {
            var projectItemzTypesFromRepo = await _projectRepository.GetItemzTypesAsync(ProjectId);
            if (projectItemzTypesFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No ItemzTypes found for ProjectID {ProjectID}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ProjectId);
                return NotFound();
            }

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {ItemzTypeNumbers} ItemzTypes based on ProjectID {ProjectID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                projectItemzTypesFromRepo.Count(),
                ProjectId);
            return Ok(_mapper.Map<IEnumerable<GetItemzTypeDTO>>(projectItemzTypesFromRepo));
        }

        /// <summary>
        /// Gets last project hierarchy number
        /// </summary>
        /// <returns>string representing highest most last project hierarchy id</returns>
        /// <response code="200">string representing highest most last project hierarchy id</response>
        /// <response code="404">No project hierarchy records found in the system</response>
        /// 
        [HttpGet("GetLastProjectHierarchyID/", Name = "__GET_Last_Project_HierarchyID__")] // e.g. http://HOST:PORT/api/Projects/GetLastProjectHierarchyID/
        [HttpHead("GetLastProjectHierarchyID/", Name = "__HEAD_Last_Project_HierarchyID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<string>> GetLastProjectHierarchyIDAsync()
        {
            var lastProjectHierarchyID = await _projectRepository.GetLastProjectHierarchyID();
            if (lastProjectHierarchyID == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Seems like there are no hierarchy record found for projects " +
                    "in this repository. " +
                    "If there are projects in the repository but you are not able to find last project hierarchyid " +
                    "then please contact your system administrator.",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results of {LastProjectHierarchyID} as highest most and last project hierarchy id in the repository",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                lastProjectHierarchyID);
            return Ok(lastProjectHierarchyID);
        }

        /// <summary>
        /// Get list of supported HTTP Options for the Projects controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions(Name = "__OPTIONS_for_Projects_Controller__")]
        public IActionResult GetProjectsOptions()
        {
            Response.Headers.Add("Allow", "GET,HEAD,OPTIONS,POST,PUT,PATCH,DELETE");
            return Ok();
        }
    }
}
