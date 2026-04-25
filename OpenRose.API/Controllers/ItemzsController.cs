// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using ItemzApp.API.Entities;
using AutoMapper;
using ItemzApp.API.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ItemzApp.API.ResourceParameters;
using ItemzApp.API.Helper;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Types;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    [Route("api/Itemzs")] // e.g. http://HOST:PORT/api/itemzs
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzsController : ControllerBase
    {
        private readonly IItemzRepository _itemzRepository;
		private readonly IHierarchyRepository _hierarchyRepository;
		private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ILogger<ItemzsController> _logger;

        public ItemzsController(IItemzRepository itemzRepository,
			IHierarchyRepository hierarchyRepository,
			IMapper mapper,
            IPropertyMappingService propertyMappingService,
            ILogger<ItemzsController> logger)
        {
            _itemzRepository = itemzRepository ?? throw new ArgumentNullException(nameof(itemzRepository));
			_hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
			_mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Get an Itemz by ID (represented by a GUID)
        /// </summary>
        /// <param name="ItemzId">GUID representing an unique ID of the Itemz that you want to get</param>
        /// <returns>A single Itemz record based on provided ID (GUID) </returns>
        /// <response code="200">Returns the requested Itemz</response>
        /// <response code="404">Requested Itemz not found</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetItemzDTO))]
        [HttpGet("{ItemzId:Guid}",
            Name = "__Single_Itemz_By_GUID_ID__")] // e.g. http://HOST:PORT/api/Itemzs/9153a516-d69e-4364-b17e-03b87442e21c
        [HttpHead("{ItemzId:Guid}",Name ="__HEAD_Itemz_By_GUID_ID__")]
        public async Task<ActionResult<GetItemzDTO>> GetItemzAsync(Guid ItemzId)
        {

            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get Itemz for ID {ItemzId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzId);
            var itemzFromRepo = await _itemzRepository.GetItemzAsync(ItemzId);

            if (itemzFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz for ID {ItemzId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Found Itemz for ID {ItemzId} and now returning results",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzId);
            return Ok(_mapper.Map<GetItemzDTO>(itemzFromRepo));
        }

        /// <summary>
        /// Gets collection of Itemzs
        /// </summary>
        /// <param name="itemzResourceParameter">Pass in information related to Pagination and Sorting Order via this parameter</param>
        /// <returns>Collection of Itemz based on expectated pagination and sorting order.</returns>
        /// <response code="200">Returns collection of Itemzs based on pagination</response>
        /// <response code="404">No Itemzs were found</response>
        [HttpGet(Name = "__GET_Itemzs_Collection__")]
        [HttpHead (Name ="__HEAD_Itemzs_Collection__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<GetItemzDTO>> GetItemzs(
            [FromQuery] ItemzResourceParameter itemzResourceParameter)
        {
            if(!_propertyMappingService.ValidMappingExistsFor<GetItemzDTO, Itemz>
                (itemzResourceParameter.OrderBy))
            {
                _logger.LogWarning("{FormattedControllerAndActionNames}Requested Order By Field {OrderByFieldName} is not found. Property Validation Failed!",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzResourceParameter.OrderBy);
                return BadRequest();
            }

            var itemzsFromRepo = _itemzRepository.GetItemzs(itemzResourceParameter);

            // EXPLANATION : Check if list is IsNullOrEmpty
            // By default we don't have option baked in the .NET to check
            // for null or empty for List type. In the following code we are first checking
            // for nullable itemzsFromRepo? and then for count great then zero via Any()
            // If any of above is true then we return true. This way we log that no itemz were
            // found in the database.
            // Ref: https://stackoverflow.com/a/54549818
            if (!itemzsFromRepo?.Any() ?? true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Items found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ItemzNumbers} Itemz found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzsFromRepo!.TotalCount);
            var previousPageLink = itemzsFromRepo.HasPrevious ?
                CreateItemzResourceUri(itemzResourceParameter,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = itemzsFromRepo.HasNext ?
                CreateItemzResourceUri(itemzResourceParameter,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = itemzsFromRepo.TotalCount,
                pageSize = itemzsFromRepo.PageSize,
                currentPage = itemzsFromRepo.CurrentPage,
                totalPages = itemzsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };

            // EXPLANATION : it's possible to send customer headers in the response.
            // So, before we hit 'return Ok...' statement, we can build our
            // own response header as you can see in following example.

            // TODO: Check if just passsing the header is good enough. How can we
            // document it so that consumers can use it effectively. Also, 
            // how to implement versioning of headers so that we don't break
            // existing applications using the headers after performing upgrade
            // in the future.

            Response.Headers.Add("X-Pagination",
                JsonConvert.SerializeObject(paginationMetadata));

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {ItemzNumbers} Itemzs to the caller",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzsFromRepo.TotalCount);
            return Ok(_mapper.Map<IEnumerable<GetItemzDTO>>(itemzsFromRepo));
        }

        /// <summary>
        /// Gets collection of Orphaned Itemzs
        /// </summary>
        /// <param name="itemzResourceParameter">Pass in information related to Pagination and Sorting Order via this parameter</param>
        /// <returns>Collection of orphaned Itemz based on expectated pagination and sorting order.</returns>
        /// <response code="200">Returns collection of orphaned Itemzs based on pagination</response>
        /// <response code="404">No Itemzs were found</response>
        [HttpGet("GetOrphan/", Name = "__GET_Orphan_Itemzs_Collection__")]
        [HttpHead("GetOrphan/", Name = "__HEAD_Orphan_Itemzs_Collection__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<GetItemzWithBasePropertiesDTO>> GetOrphanItemzs(
            [FromQuery] ItemzResourceParameter itemzResourceParameter)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<GetItemzWithBasePropertiesDTO, GetItemzWithBasePropertiesDTO>
                (itemzResourceParameter.OrderBy))
            {
                _logger.LogWarning("{FormattedControllerAndActionNames}Requested Order By Field {OrderByFieldName} is not found. Property Validation Failed!",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzResourceParameter.OrderBy);
                return BadRequest();
            }

            var itemzsFromRepo = _itemzRepository.GetOrphanItemzs(itemzResourceParameter);
        
            if (!itemzsFromRepo?.Any() ?? true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No orphan Itemz found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ItemzNumbers} orphan Itemz found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzsFromRepo!.TotalCount);
            var previousPageLink = itemzsFromRepo.HasPrevious ?
                CreateItemzResourceUri(itemzResourceParameter,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = itemzsFromRepo.HasNext ?
                CreateItemzResourceUri(itemzResourceParameter,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = itemzsFromRepo.TotalCount,
                pageSize = itemzsFromRepo.PageSize,
                currentPage = itemzsFromRepo.CurrentPage,
                totalPages = itemzsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };

            // EXPLANATION : it's possible to send customer headers in the response.
            // So, before we hit 'return Ok...' statement, we can build our
            // own response header as you can see in following example.

            // TODO: Check if just passsing the header is good enough. How can we
            // document it so that consumers can use it effectively. Also, 
            // how to implement versioning of headers so that we don't break
            // existing applications using the headers after performing upgrade
            // in the future.

            Response.Headers.Add("X-Pagination",
                JsonConvert.SerializeObject(paginationMetadata));

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {ItemzNumbers} orphan Itemzs",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzsFromRepo.TotalCount);
            return Ok(itemzsFromRepo);
        }

        /// <summary>
        /// Gets count of Orphaned Itemzs in the repository
        /// </summary>
        /// <returns>Number of Orphaned itemzs found in the repository</returns>
        /// <response code="200">Returns collection of orphaned Itemzs based on pagination</response>
        [HttpGet("GetOrphanCount/", Name = "__GET_Orphan_Itemzs_Count__")]
        [HttpHead("GetOrphanCount/", Name = "__HEAD_Orphan_Itemzs_Count__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetOrphanItemzCount()
        {
            var foundNumberOfOrphanItemz = await _itemzRepository.GetOrphanItemzsCount();
            if (foundNumberOfOrphanItemz > 0)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Found {foundNumberOfOrphanItemz} records of orphan Itemz",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    foundNumberOfOrphanItemz);
            }
            else
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No orphan Itemz records found.",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
            }
            return Ok(foundNumberOfOrphanItemz);
        }

		/// <summary>
		/// Used for creating new Itemz record in the database
		/// </summary>
		/// <param name="createItemzDTO">Used for populating information in the newly created itemz in the database</param>
		/// <param name="parentId">Used as parent for adding new Itemz as children</param>
		/// <param name="AtBottomOfChildNodes">Indicates if we should add new Itemz at TOP or BOTTOM of child Itemz nodes list</param>
		/// <returns>Newly created Itemz property details</returns>
		/// <response code="201">Returns newly created itemzs property details</response>
		/// <response code="404">Expected Parent Itemz not found</response>
		[HttpPost(Name = "__POST_Create_Itemz__")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult<GetItemzDTO>> CreateItemzAsync(
					[FromBody] CreateItemzDTO createItemzDTO
					, [FromQuery] Guid parentId
					, [FromQuery] bool AtBottomOfChildNodes = true)
		{
			Itemz itemzEntity;
			try
			{
				itemzEntity = _mapper.Map<Entities.Itemz>(createItemzDTO);

				// Normalize tags
				itemzEntity.Tags = TagParsingUtility.NormalizeAndRemoveDuplicates(itemzEntity.Tags);
				if (!TagParsingUtility.ValidateTagsLength(itemzEntity.Tags))
					return BadRequest("Tags exceed the maximum allowed length of 512 characters after normalization.");

			}
			catch (AutoMapper.AutoMapperMappingException amm_ex)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Could not create new Itemz due to issue with value provided for {fieldname}",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						amm_ex.MemberMap.DestinationName);
				return ValidationProblem();
			}
			_itemzRepository.AddItemz(itemzEntity);

			// Track the hierarchy record ID for trigger events
			Guid createdHierarchyRecordId = Guid.Empty;

			if (!(parentId.Equals(Guid.Empty)))
			{
				if (!((await _itemzRepository.ItemzExistsAsync(parentId)) || (await _itemzRepository.ItemzTypeExistsAsync(parentId))))
				{
					_logger.LogDebug("{FormattedControllerAndActionNames}Target ID {parentId} could not be found for either 'ItemzType' or 'Itemz'",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						parentId);
					return NotFound();
				}

				// Capture the hierarchy record ID returned from MoveItemzHierarchyAsync
				createdHierarchyRecordId = await _itemzRepository.MoveItemzHierarchyAsync(itemzEntity.Id, parentId
					, atBottomOfChildNodes: AtBottomOfChildNodes
					, movingItemzName: itemzEntity.Name);
			}

			await _itemzRepository.SaveAsync();

			// Add EstimationUnit from parent to newly created Itemz
			if (!parentId.Equals(Guid.Empty))
			{
				_logger.LogDebug(
					"{FormattedControllerAndActionNames}Adding EstimationUnit to newly created Itemz {ItemzId} from parent {parentId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzEntity.Id,
					parentId);

				try
				{
					bool estimationUnitAddSuccess = await _hierarchyRepository.AddHierarchyRecordEstimationUnitAsync(itemzEntity.Id);

					if (!estimationUnitAddSuccess)
					{
						_logger.LogWarning(
							"{FormattedControllerAndActionNames}Failed to add EstimationUnit for newly created Itemz {ItemzId}",
							ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
							itemzEntity.Id);
						// Non-fatal error - Itemz was created successfully, but EstimationUnit sync failed
					}
					else
					{
						_logger.LogDebug(
							"{FormattedControllerAndActionNames}Successfully added EstimationUnit to newly created Itemz {ItemzId}",
							ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
							itemzEntity.Id);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(
						"{FormattedControllerAndActionNames}Exception occurred while adding EstimationUnit for newly created Itemz {ItemzId}: {ExceptionMessage}",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						itemzEntity.Id,
						ex.Message);
					// Non-fatal error - continue
				}
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Created new Itemz with ID {ItemzId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzEntity.Id);
			return CreatedAtRoute("__Single_Itemz_By_GUID_ID__", new { ItemzId = itemzEntity.Id },
				_mapper.Map<GetItemzDTO>(itemzEntity) // Converting to DTO as this is going out to the consumer
				);
		}
		/// <summary>
		/// Used for creating new Itemz record in the database between two existing Itemz
		/// </summary>
		/// <param name="createItemzDTO">Used for populating information in the newly created itemz in the database</param>
		/// <param name="firstItemzId">Used as first Itemz for adding new Itemz between existing two Itemz</param>
		/// <param name="secondItemzId">Used as second Itemz for adding new Itemz between existing two Itemz</param>
		/// <returns>Newly created Itemz property details</returns>
		/// <response code="201">Returns newly created itemzs property details</response>
		/// <response code="404">Expected first or second from between Itemz not found</response>
		[HttpPost("CreateItemzBetweenExistingItemz/", Name = "__POST_Create_Itemz_Between_Existing_Itemz__")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult<GetItemzDTO>> CreateItemzBetweenExistingItemzAsync([FromBody] CreateItemzDTO createItemzDTO, [FromQuery] Guid firstItemzId, [FromQuery] Guid secondItemzId)
		{
			if (firstItemzId == Guid.Empty)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}First ItemzID from between two Itemz is an empty ID.",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
				return NotFound();
			}
			if (secondItemzId == Guid.Empty)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Second ItemzID from between two Itemz is an empty ID.",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
				return NotFound();
			}

			Itemz itemzEntity;

			try
			{
				itemzEntity = _mapper.Map<Entities.Itemz>(createItemzDTO);

				// Normalize tags
				itemzEntity.Tags = TagParsingUtility.NormalizeAndRemoveDuplicates(itemzEntity.Tags);
				if (!TagParsingUtility.ValidateTagsLength(itemzEntity.Tags))
					return BadRequest("Tags exceed the maximum allowed length of 512 characters after normalization.");

			}
			catch (AutoMapper.AutoMapperMappingException amm_ex)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Could not create new Itemz due to issue with value provided for {fieldname}",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						amm_ex.MemberMap.DestinationName);
				return ValidationProblem();
			}
			_itemzRepository.AddItemz(itemzEntity);

			var firstItemz = await _itemzRepository.GetItemzAsync(firstItemzId);

			if (firstItemz == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Expected first Itemz with ID {firstItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					firstItemzId);
				return NotFound();
			}

			var secondItemz = await _itemzRepository.GetItemzAsync(secondItemzId);

			if (secondItemz == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Expected second Itemz with ID {secondItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					secondItemzId);
				return NotFound();
			}

			try
			{
				await _itemzRepository.AddOrMoveItemzBetweenTwoHierarchyRecordsAsync(firstItemzId, secondItemzId, itemzEntity.Id, itemzEntity.Name!);
				await _itemzRepository.SaveAsync();
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to add Itemz between two existing Itemz :" + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				var tempMessage = $"Could not create new Itemz between Itemz '{firstItemzId}' " +
					$"and '{secondItemzId}'. " +
					$":: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}DBUpdateException Occured while trying to add Itemz between two existing Itemz :" + dbUpdateException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"DBUpdateException : Could not create new Itemz between Itemz " +
					$"'{firstItemzId}' and '{secondItemzId}'. DB Error reported, check the log file. " +
					$":: InnerException :: '{dbUpdateException.Message}' ");
			}
			catch (Microsoft.SqlServer.Types.HierarchyIdException hierarchyIDException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}HierarchyIdException Occured while trying to add Itemz between two existing Itemz :" + hierarchyIDException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return Conflict($"HierarchyIdException : Could not create new Itemz between Itemz " +
					$"'{firstItemzId}' and '{secondItemzId}'. DB Error reported, check the log file. " +
					$":: InnerException :: '{hierarchyIDException.Message}' ");
			}

			// Add EstimationUnit from parent to newly created Itemz
			_logger.LogDebug(
				"{FormattedControllerAndActionNames}Adding EstimationUnit to newly created Itemz {ItemzId} from its parent",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzEntity.Id);

			try
			{
				bool estimationUnitAddSuccess = await _hierarchyRepository.AddHierarchyRecordEstimationUnitAsync(itemzEntity.Id);

				if (!estimationUnitAddSuccess)
				{
					_logger.LogWarning(
						"{FormattedControllerAndActionNames}Failed to add EstimationUnit for newly created Itemz {ItemzId}",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						itemzEntity.Id);
					// Non-fatal error - Itemz was created successfully, but EstimationUnit sync failed
				}
				else
				{
					_logger.LogDebug(
						"{FormattedControllerAndActionNames}Successfully added EstimationUnit to newly created Itemz {ItemzId}",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						itemzEntity.Id);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(
					"{FormattedControllerAndActionNames}Exception occurred while adding EstimationUnit for newly created Itemz {ItemzId}: {ExceptionMessage}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzEntity.Id,
					ex.Message);
				// Non-fatal error - continue
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Created new Itemz with ID {ItemzId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzEntity.Id);
			return CreatedAtRoute("__Single_Itemz_By_GUID_ID__", new { ItemzId = itemzEntity.Id },
				_mapper.Map<GetItemzDTO>(itemzEntity) // Converting to DTO as this is going out to the consumer
				);
		}
		/// <summary>
		/// Used for moving Itemz record between two existing Itemz records
		/// </summary>
		/// <param name="movingItemzId">Source moving Itemz ID that will be moved to new location</param>
		/// <param name="firstItemzId">Used as first Itemz for moving Itemz between existing two Itemz</param>
		/// <param name="secondItemzId">Used as second Itemz for moving Itemz between existing two Itemz</param>
		/// <param name="estimationRollupService">Service for updating roll-up estimations after move</param>
		/// <returns>No Content</returns>
		/// <response code="204">No Content</response>
		/// <response code="404">Expected moving OR target between Itemz could not found</response>
		[HttpPost("MoveItemzBetweenExistingItemz/", Name = "__POST_Move_Itemz_Between_Existing_Itemz__")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> MoveItemzBetweenExistingItemzAsync(
			[FromQuery, BindRequired] Guid movingItemzId,
			[FromQuery, BindRequired] Guid firstItemzId,
			[FromQuery, BindRequired] Guid secondItemzId,
			[FromServices] EstimationRollupService estimationRollupService = null)
		{
			if (movingItemzId == Guid.Empty)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Source moving ItemzID is an empty ID.",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
				return NotFound();
			}

			if (firstItemzId == Guid.Empty)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}First ItemzID from between two Itemz is an empty ID.",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
				return NotFound();
			}

			if (secondItemzId == Guid.Empty)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Second ItemzID from between two Itemz is an empty ID.",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
				return NotFound();
			}

			// Validate moving Itemz exists
			var movingItemz = await _itemzRepository.GetItemzAsync(movingItemzId);
			if (movingItemz == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Expected moving Itemz with ID {movingItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					movingItemzId);
				return NotFound();
			}

			// Validate first Itemz exists
			var firstItemz = await _itemzRepository.GetItemzAsync(firstItemzId);
			if (firstItemz == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Expected first Itemz with ID {firstItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					firstItemzId);
				return NotFound();
			}

			// Validate second Itemz exists
			var secondItemz = await _itemzRepository.GetItemzAsync(secondItemzId);
			if (secondItemz == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Expected second Itemz with ID {secondItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					secondItemzId);
				return NotFound();
			}

			try
			{
				// Capture the previous parent hierarchy record ID before the move
				// The parent of firstItemzId IS the parent of the entire group (firstItemzId, movingItemzId, secondItemzId)
				Guid previousParentHierarchyId = Guid.Empty;
				var movingHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(movingItemzId);

				if (movingHierarchyRecord != null && movingHierarchyRecord.ItemzHierarchyId != null)
				{
					// Get the parent HierarchyId by going up one level
					var previousParentHierarchyIdPath = movingHierarchyRecord.ItemzHierarchyId.GetAncestor(1);

					if (previousParentHierarchyIdPath != null)
					{
						// Use repository method to get parent record details
						var previousParentRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByHierarchyIdPath(previousParentHierarchyIdPath);
						if (previousParentRecord != null)
						{
							previousParentHierarchyId = previousParentRecord.RecordId;
						}
					}
				}

				// Perform the actual move between existing Itemz records
				await _itemzRepository.AddOrMoveItemzBetweenTwoHierarchyRecordsAsync(
					firstItemzId,
					secondItemzId,
					movingItemzId,
					movingItemz.Name!);

				await _itemzRepository.SaveAsync();

				_logger.LogDebug(
					"{FormattedControllerAndActionNames}Moving Itemz ID {movingItemzId} moved between {firstItemzId} and {secondItemzId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					movingItemzId,
					firstItemzId,
					secondItemzId);

				// Get the hierarchy record ID of the moved Itemz for trigger events
				var movedHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(movingItemzId);
				Guid movedItemzHierarchyRecordId = movedHierarchyRecord?.Id ?? Guid.Empty;

				// Determine the current (new) parent after the move
				// When moving between existing Itemz records, the parent is the parent of those Itemz
				Guid currentParentHierarchyId = previousParentHierarchyId; // Same parent when moving between siblings
				if (movedHierarchyRecord != null && movedHierarchyRecord.ItemzHierarchyId != null)
				{
					// Verify current parent by getting it from the moved record's hierarchy path
					var currentParentHierarchyIdPath = movedHierarchyRecord.ItemzHierarchyId.GetAncestor(1);

					if (currentParentHierarchyIdPath != null)
					{
						var currentParentRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByHierarchyIdPath(currentParentHierarchyIdPath);
						if (currentParentRecord != null)
						{
							currentParentHierarchyId = currentParentRecord.RecordId;
						}
					}
				}

				// After successful move, trigger roll-up estimation update for moved Itemz
				if (movedItemzHierarchyRecordId != Guid.Empty && estimationRollupService != null)
				{
					_logger.LogInformation(
						"Invoking roll-up estimation update after Itemz move between existing Itemz. " +
						"MovingItemz: {movingItemzId}, PreviousParent: {previousParentHierarchyId}, CurrentParent: {currentParentHierarchyId}",
						movingItemzId,
						previousParentHierarchyId,
						currentParentHierarchyId);

					var rollupResult = await estimationRollupService.UpdateRollUpEstimationsForRecordMoveAsync(
						movedItemzHierarchyRecordId,
						previousParentHierarchyId,
						currentParentHierarchyId);

					if (!rollupResult)
					{
						_logger.LogWarning(
							"Roll-up estimation update failed for moved Itemz ID {movingItemzId} " +
							"between existing Itemz {firstItemzId} and {secondItemzId}",
							movingItemzId,
							firstItemzId,
							secondItemzId);
					}
				}

				_logger.LogDebug(
					"{FormattedControllerAndActionNames}Itemz ID {movingItemzId} successfully moved between {firstItemzId} and {secondItemzId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					movingItemzId,
					firstItemzId,
					secondItemzId);
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug(
					"{FormattedControllerAndActionNames}ApplicationException occurred while trying to move Itemz between two existing Itemz: {Exception}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					appException.Message);

				var tempMessage = $"Could not move Itemz between Itemz '{firstItemzId}' and '{secondItemzId}'. " +
					$":: InnerException :: {appException.Message}";
				return BadRequest(tempMessage);
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug(
					"{FormattedControllerAndActionNames}DBUpdateException occurred while trying to move Itemz between two existing Itemz: {Exception}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					dbUpdateException.Message);

				return Conflict($"DBUpdateException: Could not move Itemz between Itemz " +
					$"'{firstItemzId}' and '{secondItemzId}'. DB Error reported, check the log file. " +
					$":: InnerException :: '{dbUpdateException.Message}'");
			}
			catch (Microsoft.SqlServer.Types.HierarchyIdException hierarchyIDException)
			{
				_logger.LogDebug(
					"{FormattedControllerAndActionNames}HierarchyIdException occurred while trying to move Itemz between two existing Itemz: {Exception}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					hierarchyIDException.Message);

				return Conflict($"HierarchyIdException: Could not move Itemz between Itemz " +
					$"'{firstItemzId}' and '{secondItemzId}'. DB Error reported, check the log file. " +
					$":: InnerException :: '{hierarchyIDException.Message}'");
			}
			catch (Exception ex)
			{
				_logger.LogError(
					"Unexpected exception occurred during MoveItemzBetweenExistingItemzAsync: {Exception}",
					ex.Message);
				throw;
			}

			return NoContent();
		}


		/// <summary>
		/// Updating existing Itemz based on Itemz Id (GUID)
		/// </summary>
		/// <param name="itemzId">GUID representing an unique ID of the Itemz that you want to get</param>
		/// <param name="itemzToBeUpdated">required Itemz properties to be updated</param>
		/// <returns>No contents are returned but only Status 204 indicating that Item was updated successfully </returns>
		/// <response code="204">No content are returned but status of 204 indicated that item was successfully updated</response>
		/// <response code="404">Itemz based on itemzId was not found</response>
		[HttpPut("{itemzId}", Name = "__PUT_Update_Itemz_By_GUID_ID__ ")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> UpdateItemzPutAsync(Guid itemzId, UpdateItemzDTO itemzToBeUpdated)
		{
			// Check if Itemz exists
			if (!(await _itemzRepository.ItemzExistsAsync(itemzId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Itemz for ID {ItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId);
				return NotFound();
			}

			// Load Itemz for update
			var itemzFromRepo = await _itemzRepository.GetItemzForUpdatingAsync(itemzId);

			if (itemzFromRepo == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Itemz for ID {ItemzId} could not be found in the Repository",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId);
				return NotFound();
			}

			// Track whether Name actually changed to decide if we need to update ItemzHierarchy.
			// NOTE :: We use StringComparison.Ordinal to ensure case-sensitive comparison.
			// This means "ItemA" vs "itema" will be treated as different and trigger hierarchy update.
			var nameChanged = !string.Equals(itemzFromRepo.Name, itemzToBeUpdated.Name, StringComparison.Ordinal);

			// Map incoming DTO to tracked entity
			try
			{
				_mapper.Map(itemzToBeUpdated, itemzFromRepo);

				// 🔧 Normalize tags
				itemzFromRepo.Tags = TagParsingUtility.NormalizeAndRemoveDuplicates(itemzFromRepo.Tags);
				if (!TagParsingUtility.ValidateTagsLength(itemzFromRepo.Tags))
					return BadRequest("Tags exceed the maximum allowed length of 512 characters after normalization.");

			}
			catch (AutoMapper.AutoMapperMappingException amm_ex)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Could not update Itemz for ID {ItemzId} due to issue with value provided for {fieldname}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId, amm_ex.MemberMap.DestinationName);
				return ValidationProblem();
			}

			// EXPLANATION :: as part of updating Itemz record, we are making sure that Itemz name is updated in two places.
			// First in the Itemz record itself and secondly within ItemzHierarchy record as well. We are not going to update
			// BaselineItemzHierarchy record with updated Itemz name as it's a snapshot of data from a given point in time.

			// ✅ FIXED: Update ItemzHierarchy using the SAME DbContext and SAME transaction
			try
			{
				_itemzRepository.UpdateItemz(itemzFromRepo);

				if (nameChanged)
				{
					// Atomic update: update ItemzHierarchy name in the same DbContext before saving
					var hierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(itemzId);
					if (hierarchyRecord == null)
					{
						return Conflict($"Hierarchy record for Itemz with ID {itemzId} could not be found.");
					}
					hierarchyRecord.Name = itemzFromRepo.Name ?? "";
				}

				// ✅ ONE SaveChangesAsync() — atomic update
				await _itemzRepository.SaveAsync();
			}
			catch (Exception ex)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occurred while preparing ItemzHierarchy update: {Exception}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					ex.Message);
				return Conflict($"Name of ItemzHierarchy record for Itemz with ID {itemzId} could not be updated.");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Itemz for ID {ItemzId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzId);

			return NoContent(); // This indicates that update was successfully saved in the DB.
		}



		/// <summary>
		/// Partially updating a single **Itemz**
		/// </summary>
		/// <param name="itemzId">Id of the Itemz representated by a GUID.</param>
		/// <param name="itemzPatchDocument">The set of operations to apply to the Itemz via JsonPatchDocument</param>
		/// <returns>an ActionResult of type Itemz</returns>
		/// <response code="204">No content are returned but status of 204 indicated that itemz was successfully updated</response>
		/// <response code="404">Itemz based on itemzId was not found</response>
		/// <response code="422">Validation problems occured during analyzing validation rules for the JsonPatchDocument </response>
		/// <remarks> Sample request (this request updates an **Itemz's name**)   
		/// Documentation regarding JSON Patch can be found at 
		/// *[ASP.NET Core - JSON Patch Operations](https://docs.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-3.1#operations)* 
		/// 
		///     PATCH /api/Itemzs/{id}  
		///     [  
		///         {   
		///             "op": "replace",   
		///             "path": "/name",   
		///             "value": "PATCH Updated Name field"  
		///         }   
		///     ]
		/// </remarks>
		[HttpPatch("{itemzId}", Name = "__PATCH_Update_Itemz_By_GUID_ID")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> UpdateItemzPatchAsync(Guid itemzId, JsonPatchDocument<UpdateItemzDTO> itemzPatchDocument)
		{
			if (!(await _itemzRepository.ItemzExistsAsync(itemzId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Itemz for ID {ItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId);
				return NotFound();
			}

			var itemzFromRepo = await _itemzRepository.GetItemzForUpdatingAsync(itemzId);

			if (itemzFromRepo == null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Itemz for ID {ItemzId} could not be found in the Repository",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId);
				return NotFound();
			}

			var itemzToPatch = _mapper.Map<UpdateItemzDTO>(itemzFromRepo);

			itemzPatchDocument.ApplyTo(itemzToPatch, ModelState);

			// Validating Itemz patch document and verifying that it meets all the 
			// validation rules as expected. This will check if the data passed in the Patch Document
			// is ready to be saved in the db.

			if (!TryValidateModel(itemzToPatch))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Itemz Properties did not pass defined Validation Rules for ID {ItemzId}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId);
				return ValidationProblem(ModelState);
			}

			// Track whether Name actually changed to decide if we need to update ItemzHierarchy.
			var nameChanged = !string.Equals(itemzFromRepo.Name, itemzToPatch.Name, StringComparison.Ordinal);

			try
			{
				_mapper.Map(itemzToPatch, itemzFromRepo);

				// Normalize tags
				itemzFromRepo.Tags = TagParsingUtility.NormalizeAndRemoveDuplicates(itemzFromRepo.Tags);
				if (!TagParsingUtility.ValidateTagsLength(itemzFromRepo.Tags))
					return BadRequest("Tags exceed the maximum allowed length of 512 characters after normalization.");

			}
			catch (AutoMapper.AutoMapperMappingException amm_ex)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Could not update Itemz for ID {ItemzId} due to issue with value provided for {fieldname}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId, amm_ex.MemberMap.DestinationName);
				return ValidationProblem();
			}

			_itemzRepository.UpdateItemz(itemzFromRepo);

			// EXPLANATION :: as part of updating Itemz record, we are making sure that Itemz name is updated in two places.
			// First in the Itemz record itself and secondly within ItemzHierarchy record as well. We are not going to update
			// BaselineItemzHierarchy record with updated Itemz name as it's a snapshot of data from a given point in time.

			// TODO :: We should update Itemz and ItemzHierarchy together rather then two separate transactions
			// Atomic update: only touch ItemzHierarchy when Name actually changed; save both in one transaction.
			if (nameChanged)
			{
				try
				{
					var hierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(itemzFromRepo.Id);
					if (hierarchyRecord == null)
					{
						return Conflict($"Name of ItemzHierarchy record for Itemz with ID {itemzFromRepo.Id} could not be updated.");
					}
					hierarchyRecord.Name = itemzFromRepo.Name ?? "";
				}
				catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
				{
					_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to update Itemz name in ItemzHierarchy :" + dbUpdateException.InnerException,
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
						);
					return Conflict($"Name of ItemzHierarchy record for Itemz with ID {itemzFromRepo.Id} could not be updated.");
				}
				catch (Exception)
				{
					_logger.LogDebug("{FormattedControllerAndActionNames}Exception Occured while trying to update Itemz name in ItemzHierarchy :",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
						);
					return Conflict($"Name of ItemzHierarchy record for Itemz with ID {itemzFromRepo.Id} could not be updated.");
				}
			}

			// Single SaveChangesAsync — commits Itemz and (if applicable) ItemzHierarchy together.
			await _itemzRepository.SaveAsync();

			_logger.LogDebug("{FormattedControllerAndActionNames}Update request for Itemz for ID {ItemzId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzId);
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
		/// Move Itemz and it's sub-Itemz to a new location in the repository
		/// </summary>
		[HttpPost("{MovingItemzId}", Name = "__POST_Move_Itemz__")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> MoveItemzAsync(
			[FromRoute] Guid MovingItemzId,
			[FromQuery, BindRequired] Guid TargetId,
			[FromQuery] bool AtBottomOfChildNodes = true,
			[FromServices] EstimationRollupService estimationRollupService = null)
		{
			if (!(await _itemzRepository.ItemzExistsAsync(MovingItemzId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Itemz for ID {MovingItemzId} could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					MovingItemzId);
				return NotFound();
			}

			if (!((await _itemzRepository.ItemzTypeExistsAsync(TargetId)) || (await _itemzRepository.ItemzExistsAsync(TargetId))))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Target ID {TargetId} could not be found for either 'ItemzType' or 'Itemz'",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					TargetId);
				return NotFound();
			}

			try
			{
				// Capture the previous parent hierarchy record ID before the move
				Guid previousParentHierarchyId = Guid.Empty;
				var movingHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(MovingItemzId);

				if (movingHierarchyRecord != null && movingHierarchyRecord.ItemzHierarchyId != null)
				{
					var previousParentHierarchyIdPath = movingHierarchyRecord.ItemzHierarchyId.GetAncestor(1);
					if (previousParentHierarchyIdPath != null)
					{
						var previousParentRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByHierarchyIdPath(previousParentHierarchyIdPath);
						if (previousParentRecord != null)
						{
							previousParentHierarchyId = previousParentRecord.RecordId;
						}
					}
				}

				// Perform the actual move
				var movingItemzHierarchyRecordId = await _itemzRepository.MoveItemzHierarchyAsync(
					MovingItemzId,
					TargetId,
					atBottomOfChildNodes: AtBottomOfChildNodes);

				await _itemzRepository.SaveAsync();

				// Get the current (new) parent after move
				Guid currentParentHierarchyId = Guid.Empty;
				if (movingItemzHierarchyRecordId != Guid.Empty)
				{
					var movedHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(MovingItemzId);
					if (movedHierarchyRecord != null && movedHierarchyRecord.ItemzHierarchyId != null)
					{
						var currentParentHierarchyIdPath = movedHierarchyRecord.ItemzHierarchyId.GetAncestor(1);
						if (currentParentHierarchyIdPath != null)
						{
							var currentParentRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByHierarchyIdPath(currentParentHierarchyIdPath);
							if (currentParentRecord != null)
							{
								currentParentHierarchyId = currentParentRecord.RecordId;
							}
						}
					}
				}

				// After successful save, trigger roll-up estimation update
				if (movingItemzHierarchyRecordId != Guid.Empty && estimationRollupService != null)
				{
					_logger.LogInformation("Invoking roll-up estimation update after Itemz move");
					var rollupResult = await estimationRollupService.UpdateRollUpEstimationsForRecordMoveAsync(
						movingItemzHierarchyRecordId,
						previousParentHierarchyId,
						currentParentHierarchyId);

					if (!rollupResult)
					{
						_logger.LogWarning("Roll-up estimation update failed for moved Itemz ID {MovingItemzId}",
							MovingItemzId);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception occurred during ItemzMove: {Exception}", ex.Message);
				throw;
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Itemz ID {MovingItemzId} successfully moved under Target ID {TargetId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				MovingItemzId,
				TargetId);

			return RedirectToRoute("__Single_Itemz_By_GUID_ID__", new { Controller = "Itemzs", ItemzId = MovingItemzId });
		}
		/// <summary>
		/// Deleting a specific Itemz
		/// </summary>
		[HttpDelete("{itemzId}", Name = "__DELETE_Itemz_By_GUID_ID__")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult> DeleteItemzAsync(
			Guid itemzId,
			[FromServices] EstimationRollupService estimationRollupService = null)
		{
			if (!(await _itemzRepository.ItemzExistsAsync(itemzId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Cannot Delete Itemz with ID {ItemzId} as it could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					itemzId);
				return NotFound($"Cannot Delete Itemz with ID {itemzId} as it could not be found");
			}

			try
			{
				// Capture deleted record's rolled-up estimation and parent before deletion
				decimal deletedRecordRolledUpEstimation = 0;
				Guid parentHierarchyRecordId = Guid.Empty;

				var hierarchyRecordToDelete = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(itemzId);

				if (hierarchyRecordToDelete != null && hierarchyRecordToDelete.ItemzHierarchyId != null)
				{
					deletedRecordRolledUpEstimation = hierarchyRecordToDelete.RolledUpEstimation;

					var parentHierarchyIdPath = hierarchyRecordToDelete.ItemzHierarchyId.GetAncestor(1);
					if (parentHierarchyIdPath != null)
					{
						var parentRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByHierarchyIdPath(parentHierarchyIdPath);
						if (parentRecord != null)
						{
							parentHierarchyRecordId = parentRecord.RecordId;
						}
					}
				}

				// Proceed with deletion
				await _itemzRepository.DeleteItemzAsync(itemzId);

				// After successful deletion, trigger roll-up estimation update
				if (parentHierarchyRecordId != Guid.Empty && estimationRollupService != null)
				{
					_logger.LogInformation("Invoking roll-up estimation update after Itemz deletion");
					var rollupResult = await estimationRollupService.UpdateRollUpEstimationsForRecordDeletionAsync(
						parentHierarchyRecordId,
						deletedRecordRolledUpEstimation);

					if (!rollupResult)
					{
						_logger.LogWarning("Roll-up estimation update failed after Itemz deletion");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception occurred during ItemzDelete: {Exception}", ex.Message);
				throw;
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Delete request for Itemz with ID {ItemzId} processed successfully",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzId);
			return NoContent();
		}

		/// <summary>
		/// Delete All Orphan Itemz
		/// </summary>
		/// <response code="204">Returns status as successful</response>
		/// <response code="404">Requested Itemz not found</response>
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[HttpDelete("DeleteAllOrphanItemz",
			Name = "__Delete_All_Orphan_Itemz__")] // e.g. http://HOST:PORT/api/Itemzs/DeleteAllOrphanItemz
		public async Task<ActionResult> DeleteAllOrphanItemzAsync()
		{

			_logger.LogDebug("{FormattedControllerAndActionNames}Processing request to Delete All Orphan Itemz!",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
            try
            {
                await _itemzRepository.DeleteAllOrphanItemz();
            }
            catch (ApplicationException ex)
            {
				return NotFound(ex.Message);
			}
			return NoContent();
		}

		/// <summary>
		/// Copy Itemz including all it's child Itemz
		/// </summary>
		[HttpPost("CopyItemz", Name = "__Copy_Itemz_By_GUID_ID__")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult<GetItemzDTO>> CopyItemzAsync(
			CopyItemzDTO copyItemzDTO,
			CancellationToken cancellationToken,
			[FromServices] EstimationRollupService estimationRollupService = null)
		{
			if (!(await _itemzRepository.ItemzExistsAsync(copyItemzDTO.ItemzId)))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Cannot Copy Itemz with ID {ItemzId} as it could not be found",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					copyItemzDTO.ItemzId);
				return NotFound($"Cannot Copy Itemz with ID {copyItemzDTO.ItemzId} as it could not be found");
			}

			Guid newlyCopiedItemzId = Guid.Empty;
			Guid copiedItemzHierarchyRecordId = Guid.Empty;

			try
			{
				// EXPLANATION : cancellationToken is automatically populated by ASP.NET Core from
				// HttpContext.RequestAborted. When the HTTP client closes or cancels the request,
				// this token becomes cancelled, which propagates down through the repository to the
				// Entity Framework ExecuteSqlRawAsync call, aborting the SQL Server command.
				newlyCopiedItemzId = await _itemzRepository.CopyItemzAsync(copyItemzDTO.ItemzId, cancellationToken);

				// After copy is created, get the hierarchy record ID of the newly copied Itemz
				if (newlyCopiedItemzId != Guid.Empty)
				{
					var copiedHierarchyRecord = await _hierarchyRepository.GetHierarchyRecordForUpdateAsync(newlyCopiedItemzId);
					if (copiedHierarchyRecord != null)
					{
						copiedItemzHierarchyRecordId = copiedHierarchyRecord.Id;
					}
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("{FormattedControllerAndActionNames}CopyItemz request for ID {ItemzId} was cancelled by the client",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					copyItemzDTO.ItemzId.ToString());
				return StatusCode(StatusCodes.Status499ClientClosedRequest);
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}DbUpdateException occurred while trying to copy existing Itemz: {Exception}",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					dbUpdateException.InnerException);
				return Conflict($"Failed to create Itemz Copy for '{copyItemzDTO.ItemzId}'. DB Error reported, check the log file.");
			}
			catch (Exception ex)
			{
				_logger.LogError("Exception occurred during ItemzCopy: {Exception}", ex.Message);
				throw;
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Created new Itemz with ID {newlyCopiedItemzId} by copying from Itemz ID {SourceItemzId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				newlyCopiedItemzId,
				copyItemzDTO.ItemzId);

			// After successful copy, trigger roll-up estimation update
			if (copiedItemzHierarchyRecordId != Guid.Empty && estimationRollupService != null)
			{
				_logger.LogInformation("Invoking roll-up estimation update after Itemz copy");
				var rollupResult = await estimationRollupService.UpdateRollUpEstimationsForRecordCopyAsync(copiedItemzHierarchyRecordId);

				if (!rollupResult)
				{
					_logger.LogWarning("Roll-up estimation update failed for copied Itemz ID {copiedItemzHierarchyRecordId}",
						copiedItemzHierarchyRecordId);
				}
			}

			return CreatedAtRoute("__Single_Itemz_By_GUID_ID__", new { ItemzId = newlyCopiedItemzId },
				 _mapper.Map<GetItemzDTO>(await _itemzRepository.GetItemzAsync(newlyCopiedItemzId))
				);
		}

		/// <summary>
		/// Get list of supported HTTP Options for the Itemz controller.
		/// </summary>
		/// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
		/// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

		[HttpOptions (Name ="__OPTIONS_for_Itemz_Controller__")]
        public IActionResult GetItemzOptions()
        {
            Response.Headers.Add("Allow","GET,HEAD,OPTIONS,POST,PUT,PATCH,DELETE");
            return Ok();
        }

        private string CreateItemzResourceUri(
            ItemzResourceParameter itemzResourceParameter,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("__GET_Itemzs__",
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber - 1,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
                case ResourceUriType.NextPage:
                    return Url.Link("__GET_Itemzs__",
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber + 1,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
                default:
                    return Url.Link("__GET_Itemzs__",
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
            }
        }
    }
}
