// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ItemzApp.API.Helper;
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
using ItemzApp.API.BusinessRules.ItemzType;
using Microsoft.CodeAnalysis;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    //[Route("api/ItemzType")]
    [Route("api/[controller]")] // e.g. http://HOST:PORT/api/itemzs/ItemzTypes
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzTypesController : ControllerBase
    {
        private readonly IItemzTypeRepository _ItemzTypeRepository;
        private readonly IProjectRepository _projectRepository;
		private readonly IHierarchyRepository _hierarchyRepository;
		private readonly IMapper _mapper;
        // private readonly IPropertyMappingService _propertyMappingService;
        private readonly ILogger<ItemzTypesController> _logger;
        private readonly IItemzTypeRules _itemzTypeRules;

        public ItemzTypesController(IItemzTypeRepository itemzTypeRepository,
                                    IProjectRepository projectRepository,
								    IHierarchyRepository hierarchyRepository,
									IMapper mapper,
                                    //IPropertyMappingService propertyMappingService,
                                    ILogger<ItemzTypesController> logger,
                                    IItemzTypeRules itemzTypeRules)
        {
            _ItemzTypeRepository = itemzTypeRepository ?? throw new ArgumentNullException(nameof(itemzTypeRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
			_hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
			_mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            //_propertyMappingService = propertyMappingService ??
            //    throw new ArgumentNullException(nameof(propertyMappingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _itemzTypeRules = itemzTypeRules ?? throw new ArgumentNullException(nameof(itemzTypeRules));

        }

        /// <summary>
        /// Get a ItemzType by ID (represented by a GUID)
        /// </summary>
        /// <param name="ItemzTypeId">GUID representing an unique ID of the ItemzType that you want to get</param>
        /// <returns>A single ItemzType record based on provided ID (GUID) </returns>
        /// <response code="200">Returns the requested ItemzType</response>
        /// <response code="404">Requested ItemzType not found</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetItemzTypeDTO))]
        [HttpGet("{ItemzTypeId:Guid}",
            Name = "__Single_ItemzType_By_GUID_ID__")] // e.g. http://HOST:PORT/api/ItemzTypes/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("{ItemzTypeId:Guid}", Name = "__HEAD_ItemzType_By_GUID_ID__")]
        public async Task<ActionResult<GetItemzTypeDTO>> GetItemzTypeAsync(Guid ItemzTypeId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get ItemzType for ID {ItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                ItemzTypeId);
            var ItemzTypeFromRepo = await _ItemzTypeRepository.GetItemzTypeAsync(ItemzTypeId);

            if (ItemzTypeFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType for ID {ItemzTypeId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Found ItemzType for ID {ItemzTypeId} and now returning results",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                ItemzTypeId);
            return Ok(_mapper.Map<GetItemzTypeDTO>(ItemzTypeFromRepo));
        }

        /// <summary>
        /// Gets collection of ItemzTypes
        /// </summary>
        /// <returns>Collection of ItemzTypes based on expectated sorting order.</returns>
        /// <response code="200">Returns collection of ItemzTypes based on sorting order</response>
        /// <response code="404">No ItemzTypes were found</response>
        [HttpGet(Name = "__GET_ItemzTypes__")]
        [HttpHead(Name = "__HEAD_ItemzTypes_Collection__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetItemzTypeDTO>>> GetItemzTypesAsync(
            //[FromQuery] ItemzResourceParameter itemzResourceParameter
            )
        {
            //if (!_propertyMappingService.ValidMappingExistsFor<GetItemzDTO, Itemz>
            //    (itemzResourceParameter.OrderBy))
            //{
            //    _logger.LogWarning("Requested Order By Field {OrderByFieldName} is not found. Property Validation Failed!", itemzResourceParameter.OrderBy);
            //    return BadRequest();
            //}

            var ItemzTypesFromRepo = await _ItemzTypeRepository.GetItemzTypesAsync();
            if (ItemzTypesFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No ItemzTypes found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return NotFound();
            }
            // _logger.LogDebug("In total {ProjecftsNumbers} Itemz found in the repository", ItemzTypesFromRepo.Count());
            //var previousPageLink = ItemzTypesFromRepo.HasPrevious ?
            //    CreateItemzResourceUri(itemzResourceParameter,
            //    ResourceUriType.PreviousPage) : null;

            //var nextPageLink = ItemzTypesFromRepo.HasNext ?
            //    CreateItemzResourceUri(itemzResourceParameter,
            //    ResourceUriType.NextPage) : null;

            //var paginationMetadata = new
            //{
            //    totalCount = ItemzTypesFromRepo.TotalCount,
            //    pageSize = ItemzTypesFromRepo.PageSize,
            //    currentPage = ItemzTypesFromRepo.CurrentPage,
            //    totalPages = ItemzTypesFromRepo.TotalPages,
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

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {ItemzTypeNumbers} ItemzTypes to the caller",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                ItemzTypesFromRepo.Count());
            return Ok(_mapper.Map<IEnumerable<GetItemzTypeDTO>>(ItemzTypesFromRepo));
        }



        /// <summary>
        /// Used for creating new ItemzType record in the database
        /// </summary>
        /// <param name="createItemzTypeDTO">Used for populating information in the newly created ItemzType in the database</param>
        /// <returns>Newly created ItemzType property details</returns>
        /// <response code="201">Returns newly created ItemzTypes property details</response>
        /// <response code="404">Expected Project with ID was not found in the repository</response>
        /// <response code="409">ItemzType with the same name already exists in the target Project</response>

        [HttpPost(Name = "__POST_Create_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<GetItemzTypeDTO>> CreateItemzTypeAsync(CreateItemzTypeDTO createItemzTypeDTO)
        {
            if (!(await _projectRepository.ProjectExistsAsync(createItemzTypeDTO.ProjectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Project with {ProjectId} could not be found while creating new ItemzType in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    createItemzTypeDTO.ProjectId);
                return NotFound();
            }    
            var ItemzTypeEntity = _mapper.Map<Entities.ItemzType>(createItemzTypeDTO);

            if (await _itemzTypeRules.UniqueItemzTypeNameRuleAsync(createItemzTypeDTO.ProjectId, createItemzTypeDTO.Name!))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with name {createItemzTypeDTO_Name} already exists in the project with Id {createItemzTypeDTO_ProjectId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    createItemzTypeDTO.Name,
                    createItemzTypeDTO.ProjectId);
                return Conflict($"ItemzType with name '{createItemzTypeDTO.Name}' already exists in the project with Id '{createItemzTypeDTO.ProjectId}'");
            }
            try
            {

                _ItemzTypeRepository.AddItemzType(ItemzTypeEntity);
                await _ItemzTypeRepository.SaveAsync();
            }
            catch(Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to add new itemzType:" + dbUpdateException.InnerException,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"ItemzType with name '{createItemzTypeDTO.Name}' already exists in the project with Id '{createItemzTypeDTO.ProjectId}'");
            }

            _logger.LogDebug("{FormattedControllerAndActionNames}Created new ItemzType with ID {ItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                ItemzTypeEntity.Id);

            // TODO: Try and Catch logic here is not clear and it might add ItemzType
            // in the DB even if adding hierarchy record fails. In such cases 
            // we need both this steps to be included in one single transaction. 
            // If there is an issue to add ItemzType into hierarchy table then we will not be
            // able to work with it's Itemz which are expected to be childrens.

            try
            {
                await _ItemzTypeRepository.AddNewItemzTypeHierarchyAsync(ItemzTypeEntity);
                await _ItemzTypeRepository.SaveAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add new ItemzType Hierarchy:" + dbUpdateException.InnerException,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"Could not add Hierarchy for newly created ItemzType '{ItemzTypeEntity.Name}' ");
            }



            return CreatedAtRoute("__Single_ItemzType_By_GUID_ID__", new { ItemzTypeId = ItemzTypeEntity.Id },
                _mapper.Map<GetItemzTypeDTO>(ItemzTypeEntity) // Converting to DTO as this is going out to the consumer
                );
        }

		/// <summary>
		/// Updating existing ItemzType based on ItemzType Id (GUID)
		/// </summary>
		/// <param name="ItemzTypeId">GUID representing an unique ID of the ItemzType that you want to get</param>
		/// <param name="ItemzTypeToBeUpdated">required ItemzType properties to be updated</param>
		/// <returns>No contents are returned but only Status 204 indicating that ItemzType was updated successfully </returns>
		/// <response code="204">No content are returned but status of 204 indicated that ItemzType was successfully updated</response>
		/// <response code="404">ItemzType based on ItemzTypeId was not found</response>
		/// <response code="405">ItemzType is not allowed to be modified. example, ItemzType is a System one.</response>
		/// <response code="409">ItemzType with the same name already exists in the target Project</response>
		[HttpPut("{ItemzTypeId}", Name = "__PUT_Update_ItemzType_By_GUID_ID")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> UpdateItemzTypePutAsync(Guid ItemzTypeId, UpdateItemzTypeDTO ItemzTypeToBeUpdated)
		{
			if (!(await _ItemzTypeRepository.ItemzTypeExistsAsync(ItemzTypeId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for ItemzType for ID {ItemzTypeId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeId);
				return NotFound();
			}

			var ItemzTypeFromRepo = await _ItemzTypeRepository.GetItemzTypeForUpdateAsync(ItemzTypeId);

			if (ItemzTypeFromRepo == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for ItemzType for ID {ItemzTypeId} could not be found in the Repository",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeId);
				return NotFound();
			}

			if (ItemzTypeFromRepo.IsSystem == true)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}System ItemzType with name {ItemzTypeName} and Id {ItemzTypeId} is NOT ALLOWED to be modified",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeFromRepo.Name, ItemzTypeFromRepo.Id);
				return StatusCode(StatusCodes.Status405MethodNotAllowed,
					$"System ItemzType with name '{ItemzTypeFromRepo.Name}' and Id '{ItemzTypeFromRepo.Id}' is NOT ALLOWED to be modified");
			}

			if (await _itemzTypeRules.UniqueItemzTypeNameRuleAsync(ItemzTypeFromRepo.ProjectId, ItemzTypeToBeUpdated.Name!, ItemzTypeFromRepo.Id))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with name {ItemzTypeToBeUpdated_Name} already exists in the project with Id {ItemzTypeFromRepo_ProjectId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeToBeUpdated.Name,
					ItemzTypeFromRepo.ProjectId);
				return Conflict($"ItemzType with name '{ItemzTypeToBeUpdated.Name}' already exists in the project with Id '{ItemzTypeFromRepo.ProjectId}'");
			}

			// Track whether Name actually changed to decide if we need to update ItemzHierarchy.
			// NOTE :: We use StringComparison.Ordinal to ensure case-sensitive comparison.
			// This means "ItemzTypeX" vs "itemztypex" will be treated as different and trigger hierarchy update.
			var nameChanged = !string.Equals(ItemzTypeFromRepo.Name, ItemzTypeToBeUpdated.Name, StringComparison.Ordinal);

			// Map incoming DTO to tracked entity
			_mapper.Map(ItemzTypeToBeUpdated, ItemzTypeFromRepo);

			// EXPLANATION :: as part of updating ItemzType record, we are making sure that ItemzType name is updated in two places.
			// First in the ItemzType record itself and secondly within ItemzHierarchy record as well. We are not going to update
			// BaselineItemzHierarchy record with updated ItemzType name as it's a snapshot of data from a given point in time.

			// ✅ FIXED: Update ItemzType and ItemzHierarchy together in one transaction
			try
			{
				_ItemzTypeRepository.UpdateItemzType(ItemzTypeFromRepo);

				if (nameChanged)
				{
					var hierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(ItemzTypeFromRepo.Id);
					if (hierarchyRecord == null)
					{
						return Conflict($"Hierarchy record for ItemzType with ID {ItemzTypeFromRepo.Id} could not be found.");
					}
					hierarchyRecord.Name = ItemzTypeFromRepo.Name ?? "";
				}

				// ✅ ONE SaveChangesAsync — atomic update
				await _ItemzTypeRepository.SaveAsync();
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occurred while trying to update ItemzType and ItemzHierarchy: {Exception}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					dbUpdateException.InnerException?.Message);
				return Conflict($"ItemzType with name '{ItemzTypeToBeUpdated.Name}' already exists in the project with Id '{ItemzTypeFromRepo.ProjectId}'");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for ItemzType for ID {ItemzTypeId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				ItemzTypeId);

			return NoContent(); // This indicates that update was successfully saved in the DB.
		}



		/// <summary>
		/// Partially updating a single **ItemzType**
		/// </summary>
		/// <param name="ItemzTypeId">Id of the ItemzType representated by a GUID.</param>
		/// <param name="ItemzTypePatchDocument">The set of operations to apply to the ItemzType via JsonPatchDocument</param>
		/// <returns>an ActionResult of type ItemzType</returns>
		/// <response code="204">No content are returned but status of 204 indicated that ItemzType was successfully updated</response>
		/// <response code="404">ItemzType based on ItemzTypeId was not found</response>
		/// <response code="405">ItemzType is not allowed to be modified. example, ItemzType is a System one.</response>
		/// <response code="409">ItemzType with the same name already exists in the target Project</response>
		/// <response code="422">Validation problems occured during analyzing validation rules for the JsonPatchDocument </response>
		/// <remarks> Sample request (this request updates an **ItemzType's name**)   
		/// Documentation regarding JSON Patch can be found at 
		/// *[ASP.NET Core - JSON Patch Operations](https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-3.1#operations)* 
		/// 
		///     PATCH /api/ItemzTypes/{id}  
		///     [  
		///	        {   
		///             "op": "replace",   
		///             "path": "/name",   
		///             "value": "PATCH Updated Name field"  
		///	        }   
		///     ]
		/// </remarks>
		[HttpPatch("{ItemzTypeId}", Name = "__PATCH_Update_ItemzType_By_GUID_ID")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> UpdateItemzTypePatchAsync(Guid ItemzTypeId, JsonPatchDocument<UpdateItemzTypeDTO> ItemzTypePatchDocument)
		{
			if (!(await _ItemzTypeRepository.ItemzTypeExistsAsync(ItemzTypeId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for ItemzType for ID {ItemzTypeId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeId);
				return NotFound();
			}

			var ItemzTypeFromRepo = await _ItemzTypeRepository.GetItemzTypeForUpdateAsync(ItemzTypeId);

			if (ItemzTypeFromRepo == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for ItemzType for ID {ItemzTypeId} could not be found in the Repository",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeId);
				return NotFound();
			}

			if (ItemzTypeFromRepo.IsSystem == true)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}System ItemzType with name {ItemzTypeName} and Id {ItemzTypeId} is NOT ALLOWED to be modified",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeFromRepo.Name, ItemzTypeFromRepo.Id);
                return StatusCode(
                        Microsoft.AspNetCore.Http.StatusCodes.Status405MethodNotAllowed,
                        $"System ItemzType with name '{ItemzTypeFromRepo.Name}' and Id '{ItemzTypeFromRepo.Id}' is NOT ALLOWED to be modified"
                    );
			}

			// Prepare a DTO snapshot to apply the patch safely.
			var ItemzTypeToPatch = _mapper.Map<UpdateItemzTypeDTO>(ItemzTypeFromRepo);

			// Apply JSON Patch and validate the result.
			ItemzTypePatchDocument.ApplyTo(ItemzTypeToPatch, ModelState);

            // Validating ItemzType patch document and verifying that it meets all the 
            // validation rules as expected. This will check if the data passed in the Patch Document
            // is ready to be saved in the db.

			if (!TryValidateModel(ItemzTypeToPatch))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}ItemzType Properties did not pass defined Validation Rules for ID {ItemzTypeId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ItemzTypeId);
				return ValidationProblem(ModelState);
			}

			// Only run uniqueness rule if Name actually changed.
			var nameChanged = !string.Equals(ItemzTypeFromRepo.Name, ItemzTypeToPatch.Name, StringComparison.Ordinal);
			if (nameChanged)
			{
				if (await _itemzTypeRules.UniqueItemzTypeNameRuleAsync(ItemzTypeFromRepo.ProjectId, ItemzTypeToPatch.Name!, ItemzTypeFromRepo.Id))
				{
					_logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with name {ItemzTypeToPatch_Name} already exists in the project with Id {ItemzTypeFromRepo_ProjectId}",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						ItemzTypeToPatch.Name,
						ItemzTypeFromRepo.ProjectId);
					return Conflict($"ItemzType with name '{ItemzTypeToPatch.Name}' already exists in the project with Id '{ItemzTypeFromRepo.ProjectId}'");
				}
			}

			// Map patched values into the tracked entity.
			_mapper.Map(ItemzTypeToPatch, ItemzTypeFromRepo);

			// EXPLANATION :: as part of updating ItemzType record, we are making sure that ItemzType name is updated in two places.
			// First in the ItemzType record itself and secondly within ItemzHierarchy record as well. We are not going to update
			// BaselineItemzHierarchy record with updated ItemzType name as it's a snapshot of data from a given point in time.

			// ✅ Atomic: update ItemzHierarchy name only if Name actually changed; same DbContext, single SaveChanges.
			if (nameChanged)
			{
				var hierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(ItemzTypeFromRepo.Id);
				if (hierarchyRecord == null)
				{
					return Conflict($"Hierarchy record for ItemzType with ID {ItemzTypeFromRepo.Id} could not be found.");
				}
				hierarchyRecord.Name = ItemzTypeFromRepo.Name ?? "";
			}

			try
			{
				// ✅ ONE SaveChangesAsync — commits ItemzType + (maybe) ItemzHierarchy together
				await _ItemzTypeRepository.SaveAsync();
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occurred while trying to update ItemzType and ItemzHierarchy: {Exception}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					dbUpdateException.InnerException?.Message ?? dbUpdateException.Message);
				// Keep your existing conflict message semantics.
				return Conflict($"ItemzType with name '{ItemzTypeToPatch.Name}' already exists in the project with Id '{ItemzTypeFromRepo.ProjectId}'");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for ItemzType for ID {ItemzTypeId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				ItemzTypeId);

			return NoContent();
		}


		/// <summary>
		/// Move ItemzType to a new project in the repository including all its sub-Itemz
		/// </summary>
		/// <param name="MovingItemzTypeId">GUID representing an unique ID of the moving ItemzType</param>
		/// <param name="TargetProjectId">Details about target Project ID under which ItemzType will be moving</param>
		/// <param name="AtBottomOfChildNodes">Boolean value where by true means at the bottom of the existing nodes and false means at the top</param>
		/// <returns>No contents are returned when ItemzType gets moved to its new desired location</returns>
		/// <response code="204">No content are returned but status of 204 indicating that ItemzType has successfully moved to its desired location</response>
		/// <response code="404">Either moving ItemzType or target Project was not found</response>
		[HttpPost("{MovingItemzTypeId}", Name = "__POST_Move_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> MoveItemzTypeAsync([FromRoute] Guid MovingItemzTypeId, [FromQuery] Guid TargetProjectId, [FromQuery] bool AtBottomOfChildNodes = true)
        {
            if (!(await _ItemzTypeRepository.ItemzTypeExistsAsync(MovingItemzTypeId)))  
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType for ID {MovingItemzTypeId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    MovingItemzTypeId);
                return NotFound();
            }

            // TODO :: WE HAVE TO ALSO CHECK IN THE HIERARCHY TABLE IF TARGET EXISTITS OTHERWISE THERE IS NO
            // POINT IN MOVING ITEMZTYPE.

            if (!(await _projectRepository.ProjectExistsAsync(TargetProjectId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Target Project for ID {TargetProjectId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    TargetProjectId);
                return NotFound();
            }

            // TODO :: ADD NECESSARY EXCEPTION HANDLING CODE FOR MOVING ITEMZTYPE TO ANOTHER PROJECT
            try
            {
                await _ItemzTypeRepository.MoveItemzTypeToAnotherProjectAsync(MovingItemzTypeId, TargetProjectId, atBottomOfChildNodes: AtBottomOfChildNodes);
                await _ItemzTypeRepository.SaveAsync();
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to move ItemzType :" + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not move Itemz Type with ID '{MovingItemzTypeId}' " +
                    $":: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }

            _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType ID {MovingItemzTypeId} successfully moved under Target Project ID {TargetProjectId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                MovingItemzTypeId,
                TargetProjectId);
            return NoContent(); // This indicates that update was successfully saved in the DB.
        }









        /// <summary>
        /// Used for moving ItemzType record between two existing ItemzType records
        /// </summary>
        /// <param name="movingItemzTypeId">Source moving ItemzType ID that will be moved to new location</param>
        /// <param name="firstItemzTypeId">Used as first ItemzType for moving ItemzType between existing two ItemzTypes</param>
        /// <param name="secondItemzTypeId">Used as second ItemzType for moving ItemzType between existing two ItemzTypes</param>
        /// <returns>No Content</returns>
        /// <response code="204">No Content</response>
        /// <response code="404">Expected moveing OR target between ItemzTypes could not found</response>
        [HttpPost("MoveItemzTypeBetweenItemzTypes/", Name = "__POST_Move_ItemzType_Between_ItemzTypes__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> MoveItemzTypeBetweenItemzTypesAsync([FromQuery] Guid movingItemzTypeId, [FromQuery] Guid firstItemzTypeId, [FromQuery] Guid secondItemzTypeId)
        {
            if (movingItemzTypeId == Guid.Empty)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Source moving ItemzTypeID is an empty ID.",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
                return NotFound();
            }
            if (firstItemzTypeId == Guid.Empty)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}First ItemzTypeID from between two ItemzTypes is an empty ID.",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
                return NotFound();
            }
            if (secondItemzTypeId == Guid.Empty)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Second ItemzTypeID from between two ItemzTypes is an empty ID.",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
                return NotFound();
            }

            var movingItemzType = await _ItemzTypeRepository.GetItemzTypeAsync(movingItemzTypeId);

            if (movingItemzType == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Expected moving ItemzType with ID {movingItemzTypeId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    movingItemzTypeId);
                return NotFound();
            }

            var firstItemzType = await _ItemzTypeRepository.GetItemzTypeAsync(firstItemzTypeId);

            if (firstItemzType == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Expected first ItemzType with ID {firstItemzTypeId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    firstItemzTypeId);
                return NotFound();
            }

            var secondItemzType = await _ItemzTypeRepository.GetItemzTypeAsync(secondItemzTypeId);

            if (secondItemzType == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Expected second ItemzType with ID {secondItemzTypeId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    secondItemzTypeId);
                return NotFound();
            }

            try
            {
                await _ItemzTypeRepository.MoveItemzTypeBetweenTwoHierarchyRecordsAsync(firstItemzTypeId, secondItemzTypeId, movingItemzTypeId);
                await _ItemzTypeRepository.SaveAsync();
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to move ItemzType between two existing ItemzTypes :" + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not move ItemzType with Id {movingItemzTypeId} between ItemzType '{firstItemzTypeId}' " +
                    $"and '{secondItemzTypeId}'. " +
                    $":: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}DBUpdateException Occured while trying to move ItemzType between two existing ItemzTypes :" + dbUpdateException.Message,
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"DBUpdateException : Could not move ItemzType with Id {movingItemzTypeId} between ItemzTypes " +
                    $"'{firstItemzTypeId}' and '{secondItemzTypeId}'. DB Error reported, check the log file. " +
                    $":: InnerException :: '{dbUpdateException.Message}' ");
            }
            catch (Microsoft.SqlServer.Types.HierarchyIdException hierarchyIDException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}HierarchyIdException Occured while trying to add Itemz between two existing Itemz :" + hierarchyIDException.Message,
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return Conflict($"HierarchyIdException : Could not move ItemzType with Id {movingItemzTypeId} between ItemzTypes " +
                    $"'{firstItemzTypeId}' and '{secondItemzTypeId}'. DB Error reported, check the log file. " +
                    $":: InnerException :: '{hierarchyIDException.Message}' ");
            }

            return NoContent();
            //_logger.LogDebug("{FormattedControllerAndActionNames}Created new Itemz with ID {ItemzId}",
            //    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
            //    itemzEntity.Id);
            //return CreatedAtRoute("__Single_Itemz_By_GUID_ID__", new { ItemzId = itemzEntity.Id },
            //    _mapper.Map<GetItemzDTO>(itemzEntity) // Converting to DTO as this is going out to the consumer
            //    );
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
        /// Deleting a specific ItemzType
        /// </summary>
        /// <param name="ItemzTypeId">GUID representing an unique ID of the ItemzType that you want to get</param>
        /// <returns>Status code 204 is returned without any content indicating that deletion of the specified ItemzType was successful</returns>
        /// <response code="404">ItemzType based on ItemzTypeId was not found</response>
        /// <response code="405">ItemzType is not allowed to be deleted. example, ItemzType is a System one.</response>
        [HttpDelete("{ItemzTypeId}", Name = "__DELETE_ItemzType_By_GUID_ID__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteItemzTypeAsync(Guid ItemzTypeId)
        {
            if (!(await _ItemzTypeRepository.ItemzTypeExistsAsync(ItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot Delete ItemzType with ID {ItemzTypeId} as it could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeId);
                return NotFound($"Cannot Delete ItemzType with ID {ItemzTypeId} as it could not be found");
            }

            var ItemzTypeFromRepo = await _ItemzTypeRepository.GetItemzTypeForUpdateAsync(ItemzTypeId);

            if (ItemzTypeFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot Delete ItemzType with ID {ItemzTypeId} as it could not be found in the Repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeId);
                return NotFound($"Cannot Delete ItemzType with ID {ItemzTypeId} as it could not be found in the Repository");
            }

            if(ItemzTypeFromRepo.IsSystem == true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}System ItemzType with name {ItemzTypeName} and Id {ItemzTypeId} is NOT ALLOWED to be deleted",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeFromRepo.Name, ItemzTypeFromRepo.Id);
                return StatusCode(
                        Microsoft.AspNetCore.Http.StatusCodes.Status405MethodNotAllowed,
                        $"System ItemzType with name '{ItemzTypeFromRepo.Name}' and Id '{ItemzTypeFromRepo.Id}' is NOT ALLOWED to be deleted"
                    );
            }

            _ItemzTypeRepository.DeleteItemzType(ItemzTypeFromRepo);
            await _ItemzTypeRepository.SaveAsync();

            _logger.LogDebug("{FormattedControllerAndActionNames}Delete request for ItemzType with ID {ItemzTypeId} processed successfully",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
            ItemzTypeId);

            var itemzTypeYtemzHierarchyDeletionSuccessStatus = await 
                    _ItemzTypeRepository.DeleteItemzTypeItemzHierarchyAsync(ItemzTypeId);

            if (!itemzTypeYtemzHierarchyDeletionSuccessStatus)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Delete ItemzHierarchy records for ItemzType with ID {ItemzTypeId} process failed",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeId);
            }

            return NoContent();
        }

		/// <summary>
		/// Copy ItemzType including all it's child Itemz
		/// </summary>
		/// <param name="copyItemzTypeDTO">DTO containing unique ID of the ItemzType that shall be copied</param>
		/// <returns>Newly created ItemzType property details</returns>		
		/// <response code="201">Returns newly created ItemzType property details</response>
		/// <response code="404">Expected source ItemzType with ID was not found in the repository</response>
		/// <response code="409">Conflicts encountered while creating a copy from existing ItemzType</response>

		[HttpPost("CopyItemzType", Name = "__Copy_ItemzType_By_GUID_ID__")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult<GetItemzTypeDTO>> CopyItemzTypeAsync(CopyItemzTypeDTO copyItemzTypeDTO)
		{
			if (!(await _ItemzTypeRepository.ItemzTypeExistsAsync(copyItemzTypeDTO.ItemzTypeId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Cannot Copy ItemzType with ID {ItemzTypeId} as it could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					copyItemzTypeDTO.ItemzTypeId);
				return NotFound($"Cannot Copy ItemzType with ID {copyItemzTypeDTO.ItemzTypeId} as it could not be found");
			}

			Guid newlyCopiedItemzTypeId;

			try
			{
				// EXPLANATION: Because copy is created via User Defined Stored Procedure,
				// We therefor do not call SaveAsync() method on the _ItemzTypeRepository. 

				newlyCopiedItemzTypeId = await _ItemzTypeRepository.CopyItemzTypeAsync(copyItemzTypeDTO.ItemzTypeId);
				// await _ItemzTypeRepository.SaveAsync();
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to copy existing ItemzType:" + dbUpdateException.InnerException,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"Failed to create ItemzType Copy for '{copyItemzTypeDTO.ItemzTypeId}'. DB Error reported, check the log file.");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Created new ItemzType with ID {newlyCopiedItemzTypeId} by copying from ItemzType ID {copyItemzTypeDTO.ItemzTypeId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				newlyCopiedItemzTypeId,
				copyItemzTypeDTO.ItemzTypeId);

			// EXPLANATION: Because we are creating new ItemzType by copying existing ItemzType by using custom user defined stored procedure
			// we do not have access to the underlying Entity. That's why we have to call _ItemzTypeRepository.GetItemzTypeAsync
			// and then use Automapper to convert it into DTO that is returned back to the user of the API.

			return CreatedAtRoute("__Single_ItemzType_By_GUID_ID__", new { ItemzTypeId = newlyCopiedItemzTypeId },
				 _mapper.Map<GetItemzTypeDTO>(await _ItemzTypeRepository.GetItemzTypeAsync(newlyCopiedItemzTypeId)) // Converting to DTO as this is going out to the consumer 
				);

		}

		///// <summary>
		///// Gets expected Top Itemz hierarchy number by ItemzTypeId
		///// </summary>
		///// <returns>string representing highest most last Itemz hierarchy id</returns>
		///// <response code="200">string representing highest most last Itemz hierarchy id</response>
		///// <response code="404">No Itemz hierarchy records found under ItemzTypeID</response>
		///// 
		//[HttpGet("GetTopItemzHierarchyID/{ItemzTypeId}", Name = "__GET_Top_Itemz_HierarchyID__")] // e.g. http://HOST:PORT/api/Projects/GetLastItemzHierarchyID/
		//[HttpHead("GetTopItemzHierarchyID/{ItemzTypeId}", Name = "__HEAD_Top_Itemz_HierarchyID__")]
		//[ProducesResponseType(StatusCodes.Status200OK)]
		//[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesDefaultResponseType]
		//public async Task<ActionResult<string>> GetTopItemzHierarchyIDAsync(Guid ItemzTypeId)
		//{
		//    var topItemzHierarchyID = await _ItemzTypeRepository.GetTopItemzHierarchyID(ItemzTypeId);
		//    if (topItemzHierarchyID == null)
		//    {
		//        _logger.LogDebug("{FormattedControllerAndActionNames}Seems like there are no hierarchy record found for Itemz " +
		//            "under ItemzType ID {ItemzTypeId}. " +
		//            "If there are Itemzs in the repository but you are not able to find top Itemz hierarchyid " +
		//            "then please contact your system administrator.",
		//            ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
		//            , ItemzTypeId);
		//        return NotFound();
		//    }
		//    _logger.LogDebug("{FormattedControllerAndActionNames}Returning results of {topItemzHierarchyID} as expected top Itemz hierarchy id under ItemzTypeID {ItemzTypeID}",
		//        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
		//        topItemzHierarchyID,
		//        ItemzTypeId);
		//    return Ok(topItemzHierarchyID);
		//}

		///// <summary>
		///// Gets expected Bottom Itemz hierarchy number by ItemzTypeId
		///// </summary>
		///// <returns>string representing highest most last Itemz hierarchy id</returns>
		///// <response code="200">string representing highest most last Itemz hierarchy id</response>
		///// <response code="404">No Itemz hierarchy records found under ItemzTypeID</response>
		///// 
		//[HttpGet("GetLastItemzHierarchyID/{ItemzTypeId}", Name = "__GET_Last_Itemz_HierarchyID__")] // e.g. http://HOST:PORT/api/Projects/GetLastItemzHierarchyID/
		//[HttpHead("GetLastItemzHierarchyID/{ItemzTypeId}", Name = "__HEAD_Last_Itemz_HierarchyID__")]
		//[ProducesResponseType(StatusCodes.Status200OK)]
		//[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesDefaultResponseType]
		//public async Task<ActionResult<string>> GetLastItemzHierarchyIDAsync(Guid ItemzTypeId)
		//{

		//    var lastItemzHierarchyID = await _ItemzTypeRepository.GetLastItemzHierarchyID(ItemzTypeId);
		//    if (lastItemzHierarchyID == null)
		//    {
		//        _logger.LogDebug("{FormattedControllerAndActionNames}Seems like there are no hierarchy record found for Itemz " +
		//            "under ItemzType ID {ItemzTypeId}. " +
		//            "If there are Itemzs in the repository but you are not able to find last Itemz hierarchyid " +
		//            "then please contact your system administrator.",
		//            ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
		//            , ItemzTypeId);
		//        return NotFound();
		//    }
		//    _logger.LogDebug("{FormattedControllerAndActionNames}Returning results of {lastItemzHierarchyID} as expected last OR bottom Itemz hierarchy id under ItemzTypeID {ItemzTypeID}",
		//        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
		//        lastItemzHierarchyID,
		//        ItemzTypeId);
		//    return Ok(lastItemzHierarchyID);
		//}

		/// <summary>
		/// Get list of supported HTTP Options for the ItemzTypes controller.
		/// </summary>
		/// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
		/// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

		[HttpOptions(Name = "__OPTIONS_for_ItemzTypes_Controller__")]
        public IActionResult GetItemzTypesOptions()
        {
            Response.Headers.Add("Allow","GET,HEAD,OPTIONS,POST,PUT,PATCH,DELETE");
            return Ok();
        }



    }
}
