// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ItemzApp.API.Models;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ItemzApp.API.BusinessRules.Baseline;
using ItemzApp.API.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    //[Route("api/Baselines")]
    [Route("api/[controller]")] // e.g. http://HOST:PORT/api/Baselines
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BaselinesController : ControllerBase
    {
        private readonly IBaselineRepository _baselineRepository;
        private readonly IBaselineHierarchyRepository _baselineHierarchyRepository;
		private readonly IMapper _mapper;
        private readonly ILogger<BaselinesController> _logger;
        private readonly IBaselineRules _baselineRules;
        public BaselinesController( IBaselineRepository baselineRepository,
                                 IBaselineHierarchyRepository baselineHierarchyRepository,
                                 IMapper mapper,
                                 ILogger<BaselinesController> logger,
                                 IBaselineRules baselineRules
            )
        {
            _baselineRepository = baselineRepository ?? throw new ArgumentNullException(nameof(baselineRepository));
			_baselineHierarchyRepository = baselineHierarchyRepository ?? throw new ArgumentNullException(nameof(baselineHierarchyRepository));

			_mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
   
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baselineRules = baselineRules ?? throw new ArgumentNullException(nameof(baselineRules));
        }

        /// <summary>
        /// Get a Baseline by ID (represented by a GUID)
        /// </summary>
        /// <param name="BaselineId">GUID representing an unique ID of the Baseline that you want to get</param>
        /// <returns>A single Baseline record based on provided ID (GUID) </returns>
        /// <response code="200">Returns the requested Baseline</response>
        /// <response code="404">Requested Baseline not found</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetBaselineDTO))]
        [HttpGet("{BaselineId:Guid}",
            Name = "__Single_Baseline_By_GUID_ID__")] // e.g. http://HOST:PORT/api/Baselines/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("{BaselineId:Guid}", Name = "__HEAD_Baseline_By_GUID_ID__")]
        public async Task<ActionResult<GetBaselineDTO>> GetBaselineAsync(Guid BaselineId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get Baseline for ID {BaselineId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                BaselineId);
            var baselineFromRepo = await _baselineRepository.GetBaselineAsync(BaselineId);

            if (baselineFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline for ID {BaselineId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    BaselineId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Found Baseline for ID {BaselineId} and now returning results",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                BaselineId);
            return Ok(_mapper.Map<GetBaselineDTO>(baselineFromRepo));
        }


        /// <summary>
        /// Gets collection of Baselines
        /// </summary>
        /// <returns>Collection of Baselines based on expectated sorting order.</returns>
        /// <response code="200">Returns collection of Baselines based on sorting order</response>
        /// <response code="404">No Baselines were found</response>
        [HttpGet(Name = "__GET_Baselines_Collection__")]
        [HttpHead(Name = "__HEAD_Baselines_Collection__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetBaselineDTO>>> GetBaselinesAsync()
        {
            var baselinesFromRepo = await _baselineRepository.GetBaselinesAsync();
            if (baselinesFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Baselines found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return NotFound();
            }

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {BaselineNumbers} Baselines to the caller",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                baselinesFromRepo.Count());
            return Ok(_mapper.Map<IEnumerable<GetBaselineDTO>>(baselinesFromRepo));
        }

        /// <summary>
        /// Used for creating new Baseline record in the database
        /// </summary>
        /// <param name="createBaselineDTO">Used for populating information in the newly created Baseline in the database</param>
        /// <returns>Newly created Baseline property details</returns>
        /// <response code="201">Returns newly created Baselines property details</response>
        /// <response code="404">Expected Project with ID was not found in the repository</response>
        /// <response code="409">Baseline with the same name already exists in the repository</response>

        [HttpPost(Name = "__POST_Create_Baseline__")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<GetBaselineDTO>> CreateBaselineAsync(CreateBaselineDTO createBaselineDTO)
        {
            if (!(await _baselineRepository.ProjectExistsAsync(createBaselineDTO.ProjectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Project with {ProjectId} could not be found while creating new Baseline in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    createBaselineDTO.ProjectId);
                return NotFound();
            }

            var baselineEntity = _mapper.Map<Entities.Baseline>(createBaselineDTO);

            if (await _baselineRules.UniqueBaselineNameRuleAsync(createBaselineDTO.ProjectId, createBaselineDTO.Name!))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline with name {createBaselineDTO_Name} already exists in the project with Id {createBaselineDTO_ProjectId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    createBaselineDTO.Name,
                    createBaselineDTO.ProjectId);
                return Conflict($"Baseline with name '{createBaselineDTO.Name}' already exists in the project with Id '{createBaselineDTO.ProjectId}'");

            }

            if (baselineEntity.ItemzTypeId != Guid.Empty)
            {
                var foundItemzCountByItemzType = 
                        await _baselineRepository.GetItemzCountByItemzTypeAsync(
                            baselineEntity.ItemzTypeId);

                if (foundItemzCountByItemzType == 0)
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames}Zero Itemz found in ItemzType with ID {ItemzTypeId}",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                        baselineEntity.ItemzTypeId);
                    return Conflict($"Zero Itemz found in ItemzType with ID '{baselineEntity.ItemzTypeId}'");
                }
            }
            else
            {
                var foundItemzCountByProject =
                        await _baselineRepository.GetItemzCountByProjectAsync(
                            baselineEntity.ProjectId);

                if (foundItemzCountByProject == 0)
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames}Zero Itemz found in Project with ID {ProjectId}",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                        baselineEntity.ProjectId);
                    return Conflict($"Zero Itemz found in Project with ID '{baselineEntity.ProjectId}'");
                }

            }

            try
            {
                // EXPLANATION: Because baseline is created via User Defined Stored Procedure,
                // We therefor do not call SaveAsync() method on the _baselineRepository. 
                // Also notice that because we are returning newly created baseline GUID from 
                // AddBaselineAsync method, that value is directly getting assigned to 
                // baselineEntry.Id as we use it subsequently to send this GUID back to the 
                // calling application so that they get correct GUID returned to them.

                baselineEntity.Id = await _baselineRepository.AddBaselineAsync(baselineEntity);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add new baseline:" + dbUpdateException.InnerException,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"Baseline with name '{baselineEntity.Name}' already exists in the repository. DB Error reported, check the log file.");
            }
            //catch (Microsoft.Data.SqlClient.SqlException sqlException)
            //{
            //}
            catch (Exception ex)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add new baseline:" + ex.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"Exception Occured while trying to add new baseline: {ex.Message}");
            }


            _logger.LogDebug("{FormattedControllerAndActionNames}Created new Baseline with ID {BaselineId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineEntity.Id);
            return CreatedAtRoute("__Single_Baseline_By_GUID_ID__", new { BaselineId = baselineEntity.Id },
                _mapper.Map<GetBaselineDTO>(baselineEntity) // Converting to DTO as this is going out to the consumer
                );
        }

        /// <summary>
        /// Used for creating new Baseline record by cloning existing baseline
        /// </summary>
        /// <param name="cloneBaselineDTO">Used for cloning existing baseline by BaselineId</param>
        /// <returns>Newly created Baseline property details by cloning existing baseline</returns>
        /// <response code="201">Returns newly created Baseline property details</response>
        /// <response code="404">Expected Baseline with ID was not found in the repository</response>
        /// <response code="409">Conflicts encountered while creating new Baseline from existing Baseline</response>

        [HttpPost("CloneBaseline", Name = "__POST_Clone_Baseline__")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<GetBaselineDTO>> CloneBaselineAsync(CloneBaselineDTO cloneBaselineDTO)
        {
            var sourceBaselineFromRepo = await _baselineRepository.GetBaselineAsync(cloneBaselineDTO.BaselineId);

            if (sourceBaselineFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Source Baseline with {BaselineId} could not be found while cloning Baseline in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    cloneBaselineDTO.BaselineId);
                return NotFound();
            }

            var baselineEntity = _mapper.Map<Entities.NonEntity_CloneBaseline>(cloneBaselineDTO);

            if (await _baselineRules.UniqueBaselineNameRuleAsync(sourceBaselineFromRepo.ProjectId, cloneBaselineDTO.Name!))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline with name {cloneBaselineDTO_Name} already exists in the project with Id {sourceBaselineFromRepo_ProjectId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    cloneBaselineDTO.Name,
                    sourceBaselineFromRepo.ProjectId);
                return Conflict($"Baseline with name '{cloneBaselineDTO.Name}' already exists in the project with Id '{sourceBaselineFromRepo.ProjectId}'");

            }

            var foundBaselineItemzCountByBaselineId = 
                        await _baselineRepository.GetBaselineItemzCountByBaselineAsync(
                            cloneBaselineDTO.BaselineId);

            if (foundBaselineItemzCountByBaselineId == 0)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Zero BaselineItemz found in Baseline with ID {cloneBaselineDTO_BaselineId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    cloneBaselineDTO.BaselineId);
                return Conflict($"Zero BaselineItemz found in Baseline with ID '{ cloneBaselineDTO.BaselineId}'");
            }
            Guid newlyClonedBaselineId;

            try
            {
                // EXPLANATION: Because baseline is created via User Defined Stored Procedure,
                // We therefor do not call SaveAsync() method on the _baselineRepository. 

                newlyClonedBaselineId = await _baselineRepository.CloneBaselineAsync(baselineEntity);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to clone existing baseline:" + dbUpdateException.InnerException,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"Baseline with name '{baselineEntity.Name}' already exists in the repository. DB Error reported, check the log file.");
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Created new Baseline with ID {newlyClonedBaselineId} by cloning from Baseline ID {cloneBaselineDTO_BaselineId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                newlyClonedBaselineId,
                cloneBaselineDTO.BaselineId);

            // EXPLANATION: Because we are creating new baseline from existing baseline by cloning it using custom user defined stored procedure
            // we do not have access to the underlying Entity. That's why we have to call _baselineRepository.GetBaselineAsync
            // and then use Automapper to convert it into DTO that is returned back to the user of the API.

            return CreatedAtRoute("__Single_Baseline_By_GUID_ID__", new { BaselineId = newlyClonedBaselineId },
                 _mapper.Map<GetBaselineDTO>(await _baselineRepository.GetBaselineAsync(newlyClonedBaselineId)) // Converting to DTO as this is going out to the consumer 
                );
        }

		/// <summary>
		/// Updating exsting Baseline based on Baseline Id (GUID)
		/// </summary>
		/// <param name="baselineId">GUID representing an unique ID of the Baseline that you want to update</param>
		/// <param name="baselineToBeUpdated">required Baseline properties to be updated</param>
		/// <returns>No content are returned but only Status 204 indicating that Baseline was updated successfully </returns>
		/// <response code="204">No content are returned but status of 204 indicated that Baseline was successfully updated</response>
		/// <response code="404">Baseline based on baselineId was not found</response>
		/// <response code="409">Baseline with updated name already exists in the repository</response>
		[HttpPut("{baselineId}", Name = "__PUT_Update_Baseline_By_GUID_ID")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> UpdateBaselinePutAsync(Guid baselineId, UpdateBaselineDTO baselineToBeUpdated)
		{
			if (!(await _baselineRepository.BaselineExistsAsync(baselineId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Baseline for ID {BaselineId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					baselineId);
				return NotFound();
			}

			var baselineFromRepo = await _baselineRepository.GetBaselineForUpdateAsync(baselineId);

			if (baselineFromRepo == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Baseline for ID {BaselineId} could not be found in the Repository",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					baselineId);
				return NotFound();
			}

			if (await _baselineRules.UniqueBaselineNameRuleAsync(baselineFromRepo.ProjectId, baselineToBeUpdated.Name!, baselineFromRepo.Name))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Baseline with name {baselineToBeUpdated_Name} already exists in the project with Id {baselineFromRepo_ProjectId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					baselineToBeUpdated.Name,
					baselineFromRepo.ProjectId);
				return Conflict($"Baseline with name '{baselineToBeUpdated.Name}' already exists in the project with Id '{baselineFromRepo.ProjectId}'");
			}

			// Track whether Name actually changed to decide if we need to update BaselineItemzHierarchy.
			// NOTE :: We use StringComparison.Ordinal to ensure case-sensitive comparison.
			// This means "BaselineX" vs "baselinex" will be treated as different and trigger hierarchy update.
			var nameChanged = !string.Equals(baselineFromRepo.Name, baselineToBeUpdated.Name, StringComparison.Ordinal);

			// Map incoming DTO to tracked entity
			_mapper.Map(baselineToBeUpdated, baselineFromRepo);

			// EXPLANATION :: as part of updating Baseline record, we are making sure that baseline name is updated in two places.
			// First in the Baseline record itself and secondly within BaselineItemzHierarchy record as well. 

			try
			{
				_baselineRepository.UpdateBaseline(baselineFromRepo);

				if (nameChanged)
				{
					// Atomic update: update BaselineItemzHierarchy name in the same DbContext before saving
					var hierarchyRecord = await _baselineRepository.GetBaselineHierarchyRecordForUpdateAsync(baselineFromRepo.Id);
					if (hierarchyRecord == null)
					{
						return Conflict($"Name of BaselineItemzHierarchy record for Baseline with ID {baselineFromRepo.Id} could not be updated.");
					}
					hierarchyRecord.Name = baselineFromRepo.Name ?? "";
				}

				// Single SaveChangesAsync — commits Baseline and BaselineItemzHierarchy together
				await _baselineRepository.SaveAsync();
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to update baseline and BaselineItemzHierarchy :" + dbUpdateException.InnerException,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"Baseline with name '{baselineToBeUpdated.Name}' already exists in the project with Id '{baselineFromRepo.ProjectId}'. DB Error reported, check the log file.");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Baseline for ID {BaselineId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				baselineId);
			return NoContent(); // This indicates that update was successfully saved in the DB.
		}



		/// <summary>
		/// Partially updating a single **Baseline**
		/// </summary>
		/// <param name="baselineId">Id of the Baseline representated by a GUID.</param>
		/// <param name="baselinePatchDocument">The set of operations to apply to the Baseline via JsonPatchDocument</param>
		/// <returns>an ActionResult of type Baseline</returns>
		/// <response code="204">No content are returned but status of 204 indicated that Baseline was successfully updated</response>
		/// <response code="404">Baseline based on baselineId was not found</response>
		/// <response code="409">Baseline with updated name already exists in the repository</response>
		/// <response code="422">Validation problems occured during analyzing validation rules for the JsonPatchDocument </response>
		/// <remarks> Sample request (this request updates an **Baseline's name**)   
		/// Documentation regarding JSON Patch can be found at 
		/// *[ASP.NET Core - JSON Patch Operations](https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-3.1#operations)* 
		/// 
		///     PATCH /api/Baselines/{id}  
		///     [  
		///         {   
		///             "op": "replace",   
		///             "path": "/name",   
		///             "value": "PATCH Updated Name field"  
		///         }   
		///     ]
		/// </remarks>

		[HttpPatch("{baselineId}", Name = "__PATCH_Update_Baseline_By_GUID_ID")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> UpdateBaselinePatchAsync(Guid baselineId, JsonPatchDocument<UpdateBaselineDTO> baselinePatchDocument)
		{
			if (!(await _baselineRepository.BaselineExistsAsync(baselineId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Baseline for ID {BaselineId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					baselineId);
				return NotFound();
			}

			var baselineFromRepo = await _baselineRepository.GetBaselineForUpdateAsync(baselineId);

			if (baselineFromRepo == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Baseline for ID {BaselineId} could not be found in the Repository",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					baselineId);
				return NotFound();
			}

			var baselineToPatch = _mapper.Map<UpdateBaselineDTO>(baselineFromRepo);

			baselinePatchDocument.ApplyTo(baselineToPatch, ModelState);

			// Validating Baseline patch document and verifying that it meets all the 
			// validation rules as expected. This will check if the data passed in the Patch Document
			// is ready to be saved in the db.

			if (!TryValidateModel(baselineToPatch))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Baseline Properties did not pass defined Validation Rules for ID {BaselineId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					baselineId);
				return ValidationProblem(ModelState);
			}

			// Track whether Name actually changed to decide if we need to update BaselineItemzHierarchy.
			var nameChanged = !string.Equals(baselineFromRepo.Name, baselineToPatch.Name, StringComparison.Ordinal);

			if (nameChanged)
			{
				if (await _baselineRules.UniqueBaselineNameRuleAsync(baselineFromRepo.ProjectId, baselineToPatch.Name!, baselineFromRepo.Name))
				{
					_logger.LogDebug("{FormattedControllerAndActionNames}Baseline with name {baselineToPatch_Name} already exists in the project with Id {BaselineFromRepo_ProjectId}",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						baselineToPatch.Name,
						baselineFromRepo.ProjectId);
					return Conflict($"Baseline with name '{baselineToPatch.Name}' already exists in the project with Id '{baselineFromRepo.ProjectId}'");
				}
			}

			_mapper.Map(baselineToPatch, baselineFromRepo);

			// EXPLANATION :: as part of updating Baseline record, we are making sure that baseline name is updated in two places.
			// First in the Baseline record itself and secondly within BaselineItemzHierarchy record as well. 

			// TODO :: We should update Baseline and BaselineItemzHierarchy together rather then two separate transactions

			try
			{
				_baselineRepository.UpdateBaseline(baselineFromRepo);

				if (nameChanged)
				{
					var hierarchyRecord = await _baselineRepository.GetBaselineHierarchyRecordForUpdateAsync(baselineFromRepo.Id);
					if (hierarchyRecord == null)
					{
						return Conflict($"Name of BaselineItemzHierarchy record for Baseline with ID {baselineFromRepo.Id} could not be updated.");
					}
					hierarchyRecord.Name = baselineFromRepo.Name ?? "";
				}

				// Single SaveChangesAsync — commits Baseline and BaselineItemzHierarchy together
				await _baselineRepository.SaveAsync();

			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to update baseline and BaselineItemzHierarchy :" + dbUpdateException.InnerException,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"Baseline with name '{baselineToPatch.Name}' already exists in the project with Id '{baselineFromRepo.ProjectId}'. DB Error reported, check the log file.");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Baseline for ID {BaselineId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				baselineId);
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
        /// Deleting a specific Baseline
        /// </summary>
        /// <param name="baselineId">GUID representing an unique ID of the Baseline that you want to get</param>
        /// <returns>Status code 204 is returned without any content indicating that deletion of the specified Baseline was successful</returns>
        /// <response code="404">Baseline based on baselineId was not found</response>
        [HttpDelete("{baselineId}", Name = "__DELETE_Baseline_By_GUID_ID__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteBaselineAsync(Guid baselineId)
        {
            if (!(await _baselineRepository.BaselineExistsAsync(baselineId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot Delete Baseline with ID {BaselineId} as it could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    baselineId);
                return NotFound($"Cannot Delete Baseline with ID {baselineId} as it could not be found");
            }

            var baselineFromRepo = await _baselineRepository.GetBaselineForUpdateAsync(baselineId);

            if (baselineFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot Delete Baseline with ID {BaselineId} as it could not be found in the Repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    baselineId);
                return NotFound($"Cannot Delete Baseline with ID {baselineId} as it could not be found in the Repository");
            }

            _baselineRepository.DeleteBaseline(baselineFromRepo);
            await _baselineRepository.SaveAsync();

            _logger.LogDebug("{FormattedControllerAndActionNames}Delete request for Baseline with ID {BaselineId} processed successfully",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                baselineId);

            await _baselineRepository.DeleteOrphanedBaselineItemzAsync();
            return NoContent();
        }

        /// <summary>
        /// Get total number of BaselineItemz by Baseline
        /// </summary>
        /// <param name="baselineId">Provide BaselineID representated in GUID form</param>
        /// <returns>Number of BaselineItemz found for the given BaselineID. Zero if none found.</returns>
        /// <response code="200">Returns number of BaselineItemz count that were associated with a given Baseline</response>
        /// <response code="404">Baseline based on baselineId was not found</response>
        [HttpGet("GetBaselineItemzCount/{BaselineId:Guid}", Name = "__GET_BaselineItemz_Count_By_Baseline__")]
        [HttpHead("GetBaselineItemzCount/{BaselineId:Guid}", Name = "__HEAD_BaselineItemz_Count_By_Baseline__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetBaselineItemzCountByBaselineAsync(Guid baselineId)
        {
            if (!(await _baselineRepository.BaselineExistsAsync(baselineId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find count of BaselineItemz as Baseline with ID {BaselineId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineId);
                return NotFound();
            }

            var foundBaselineItemzCountByBaselineId = await _baselineRepository.GetBaselineItemzCountByBaselineAsync(baselineId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundBaselineItemzCountByBaselineId} BaselineItemz records for Baseline with ID {baselineId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundBaselineItemzCountByBaselineId,
                baselineId);
            return foundBaselineItemzCountByBaselineId;
        }



        /// <summary>
        /// Get total number of BaselineItemz Traces by Baseline
        /// </summary>
        /// <param name="BaselineId">Provide BaselineID representated in GUID form</param>
        /// <returns>Number of BaselineItemz Traces found for the given BaselineID. Zero if none found.</returns>
        /// <response code="200">Returns number of BaselineItemz Traces count that are associated with a given Baseline</response>
        /// <response code="404">Baseline based on baselineId was not found</response>
        [HttpGet("GetBaselineItemzTraceCount/{BaselineId:Guid}", Name = "__GET_BaselineItemz_Trace_Count_By_Baseline__")]
        [HttpHead("GetBaselineItemzTraceCount/{BaselineId:Guid}", Name = "__HEAD_BaselineItemz_Trace_Count_By_Baseline__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetBaselineItemzTraceCountByBaselineAsync(Guid BaselineId)
        {
            if (!(await _baselineRepository.BaselineExistsAsync(BaselineId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find count of BaselineItemz Traces as Baseline with ID {BaselineId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    BaselineId);
                return NotFound();
            }

            var foundBaselineItemzTraceCountByBaselineId = await _baselineRepository.GetBaselineItemzTraceCountByBaselineAsync(BaselineId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundBaselineItemzTraceCountByBaselineId} BaselineItemz Trace records for Baseline with ID {baselineId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundBaselineItemzTraceCountByBaselineId,
                BaselineId);
            return foundBaselineItemzTraceCountByBaselineId;
        }


        /// <summary>
        /// Get total number of Included BaselineItemz by Baseline
        /// </summary>
        /// <param name="baselineId">Provide BaselineID representated in GUID form</param>
        /// <returns>Number of Included BaselineItemz found for the given BaselineID. Zero if none found.</returns>
        /// <response code="200">Returns number of Included BaselineItemz count that were associated with a given Baseline</response>
        /// <response code="404">Baseline based on baselineId was not found</response>
        [HttpGet("GetIncludedBaselineItemzCount/{BaselineId:Guid}", Name = "__GET_Included_BaselineItemz_Count_By_Baseline__")]
        [HttpHead("GetIncludedBaselineItemzCount/{BaselineId:Guid}", Name = "__HEAD_Included_BaselineItemz_Count_By_Baseline__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetIncludedBaselineItemzCountByBaselineAsync(Guid baselineId)
        {
            if (!(await _baselineRepository.BaselineExistsAsync(baselineId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find count of Included BaselineItemz as Baseline with ID {BaselineId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineId);
                return NotFound();
            }

            var foundIncludedBaselineItemzCountByBaselineId = await _baselineRepository.GetIncludedBaselineItemzCountByBaselineAsync(baselineId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundBaselineItemzCountByBaselineId} Included BaselineItemz records for Baseline with ID {baselineId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundIncludedBaselineItemzCountByBaselineId,
                baselineId);
            return foundIncludedBaselineItemzCountByBaselineId;
        }

        /// <summary>
        /// Get total number of Baseline by Project ID
        /// </summary>
        /// <param name="ProjectId">Provide ProjectID representated in GUID form</param>
        /// <returns>Number of Baseline found for the given ProjectID. Zero if none found.</returns>
        /// <response code="200">Returns number of Baseline count that were associated with a given ProjectID</response>
        /// <response code="404">Project based on projectID was not found</response>
        [HttpGet("GetBaselineCount/{ProjectId:Guid}", Name = "__GET_Baseline_Count_By_Project__")]
        [HttpHead("GetBaselineCount/{ProjectId:Guid}", Name = "__HEAD_Baseline_Count_By_Project__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetBaselineCountByProjectIDAsync(Guid ProjectId)
        {
            if (!(await _baselineRepository.ProjectExistsAsync(ProjectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find count of Baseline as Project with ID {ProjectId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ProjectId);
                return NotFound();
            }

            var foundBaselineCountByProjectId = await _baselineRepository.GetBaselineCountByProjectIdAsync(ProjectId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundBaselineCountByProjectId} Included BaselineItemz records for Project with ID {ProjectId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundBaselineCountByProjectId,
                ProjectId);
            return foundBaselineCountByProjectId < 0 ? 0 : foundBaselineCountByProjectId;
        }

        /// <summary>
        /// Get Baselines that are associated with given Project ID
        /// </summary>
        /// <param name="ProjectId">Provide ProjectID representated in GUID form</param>
        /// <returns>All Baselines associated with the given ProjectID. Zero if none found.</returns>
        /// <response code="200">Returns all Baselines that are associated with a given ProjectID</response>
        /// <response code="404">Project based on projectID was not found</response>
        [HttpGet("GetBaselines/{ProjectId:Guid}", Name = "__GET_Baselines_By_Project_Id__")]
        [HttpHead("GetBaselines/{ProjectId:Guid}", Name = "__HEAD_Baselines_By_Project_Id__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetBaselineDTO>>> GetBaselinesByProjectIDAsync(Guid ProjectId)
        {
            if (!(await _baselineRepository.ProjectExistsAsync(ProjectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Project with ID {ProjectId} were found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ProjectId);
                return NotFound();
            }

            var foundBaselinesByProjectId = await _baselineRepository.GetBaselinesByProjectIdAsync(ProjectId);
            if (foundBaselinesByProjectId == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Baselines association found for Project with ID {ProjectID}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ProjectId);
                return NotFound();
            }

            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundBaselinesCountByProjectId} Baselines associated to Project with ID {ProjectId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundBaselinesByProjectId.Count(),
                ProjectId);
            return Ok(_mapper.Map<IEnumerable<GetBaselineDTO>>(foundBaselinesByProjectId));
        }

        /// <summary>
        /// Get total number of Excluded BaselineItemz by Baseline
        /// </summary>
        /// <param name="baselineId">Provide BaselineID representated in GUID form</param>
        /// <returns>Number of Excluded BaselineItemz found for the given BaselineID. Zero if none found.</returns>
        /// <response code="200">Returns number of Excluded BaselineItemz count that were associated with a given Baseline</response>
        /// <response code="404">Baseline based on baselineId was not found</response>
        [HttpGet("GetExcludedBaselineItemzCount/{BaselineId:Guid}", Name = "__GET_Excluded_BaselineItemz_Count_By_Baseline__")]
        [HttpHead("GetExcludedBaselineItemzCount/{BaselineId:Guid}", Name = "__HEAD_Excluded_BaselineItemz_Count_By_Baseline__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetExcludedBaselineItemzCountByBaselineAsync(Guid baselineId)
        {
            if (!(await _baselineRepository.BaselineExistsAsync(baselineId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find count of Excluded BaselineItemz as Baseline with ID {BaselineId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineId);
                return NotFound();
            }

            var foundExcludedBaselineItemzCountByBaselineId = await _baselineRepository.GetExcludedBaselineItemzCountByBaselineAsync(baselineId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundBaselineItemzCountByBaselineId} Excluded BaselineItemz records for Baseline with ID {baselineId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundExcludedBaselineItemzCountByBaselineId,
                baselineId);
            return foundExcludedBaselineItemzCountByBaselineId;
        }

        /// <summary>
        /// Get total number of Orphaned BaselineItemz in Repository
        /// </summary>
        /// <returns>Total Number of Orphaned BaselineItemz found in entire Repository. Zero if none found.</returns>
        /// <response code="200">Returns total number of Orphaned BaselineItemz count that are present in the Repository.</response>
        [HttpGet("GetOrphanedBaselineItemzCount", Name = "__GET_Orphaned_BaselineItemz_Count__")]
        [HttpHead("GetOrphanedBaselineItemzCount", Name = "__HEAD_Orphaned_BaselineItemz_Count__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetOrphanedBaselineItemzCountInRepository()
        {
            var foundOrphanedBaselineItemzCountInRepository = await _baselineRepository.GetOrphanedBaselineItemzCount();
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundTotalBaselineItemzCountInRepository} Orphaned BaselineItemz records in current Repository",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundOrphanedBaselineItemzCountInRepository);
            return foundOrphanedBaselineItemzCountInRepository;
        }

        /// <summary>
        /// Get total number of BaselineItemz in Repository
        /// </summary>
        /// <returns>Total Number of BaselineItemz found in entire Repository. Zero if none found.</returns>
        /// <response code="200">Returns total number of BaselineItemz count that are present in the Repository.</response>
        [HttpGet("GetTotalBaselineItemzCount", Name = "__GET_Total_BaselineItemz_Count__")]
        [HttpHead("GetTotalBaselineItemzCount", Name = "__HEAD_Total_BaselineItemz_Count__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetTotalBaselineItemzCountInRepository()
        {
            var foundTotalBaselineItemzCountInRepository = await _baselineRepository.GetTotalBaselineItemzCountAsync();
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundTotalBaselineItemzCountInRepository} BaselineItemz records in current Repository",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundTotalBaselineItemzCountInRepository);
            return foundTotalBaselineItemzCountInRepository;
        }

        /// <summary>
        /// Gets collection of BaselineItemzTypes for the given BaselineID
        /// </summary>
        /// <returns>Collection of BaselineItemzTypes based on sorting order for the given BaselineID</returns>
        /// <response code="200">Returns collection of BaselineItemzTypes based on sorting order for the given BaselineID</response>
        /// <response code="404">No BaselineItemzTypes were found for the given BaselineID</response>

        [HttpGet("GetBaselineItemzTypes/{BaselineId:Guid}", Name = "__GET_BaselineItemzTypes_By_Baseline__")] // e.g. http://HOST:PORT/api/Baselines/GetItemzTypes/42f62a6c-fcda-4dac-a06c-406ac1c17770

        [HttpHead("GetBaselineItemzTypes/{BaselineId:Guid}", Name = "__HEAD_BaselineItemzTypes_By_Baseline__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetBaselineItemzTypeDTO>>> GetBaselineItemzTypesByBaselineIdAsync(Guid BaselineId)
        {
            var baselineItemzTypesFromRepo = await _baselineRepository.GetBaselineItemzTypesAsync(BaselineId);
            if (baselineItemzTypesFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No BaselineItemzTypes found for BaselineID {BaselineID}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    BaselineId);
                return NotFound();
            }

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {BaselineItemzTypeNumbers} BaselineItemzTypes based on BaselineID {BaselineID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineItemzTypesFromRepo.Count(),
                BaselineId);
            return Ok(_mapper.Map<IEnumerable<GetBaselineItemzTypeDTO>>(baselineItemzTypesFromRepo));
        }

        /// <summary>
        /// Get list of supported HTTP Options for the Baselines controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions(Name = "__OPTIONS_for_Baselines_Controller__")]
        public IActionResult GetBaselinesOptions()
        {
            Response.Headers.Add("Allow", "GET,HEAD,OPTIONS,POST,PUT,PATCH,DELETE");
            return Ok();
        }
    }
}
