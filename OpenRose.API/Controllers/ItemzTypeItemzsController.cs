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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ItemzApp.API.ResourceParameters;
using ItemzApp.API.Helper;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    [Route("api/ItemzTypeItemzs")] // e.g. http://HOST:PORT/api/ItemzTypeItemzs
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzTypeItemzsController : ControllerBase
    {
        private readonly IItemzRepository _itemzRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ILogger<ItemzTypeItemzsController> _logger;

        public ItemzTypeItemzsController(IItemzRepository itemzRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            ILogger<ItemzTypeItemzsController> logger)
        {
            _itemzRepository = itemzRepository ?? throw new ArgumentNullException(nameof(itemzRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Gets collection of Itemzs by ItemzType ID
        /// </summary>
        /// <param name="ItemzTypeId">ItemzType ID for which Itemz are queried</param>
        /// <param name="itemzResourceParameter">Pass in information related to Pagination and Sorting Order via this parameter</param>
        /// <returns>Collection of Itemz based on expectated pagination and sorting order.</returns>
        /// <response code="200">Returns collection of Itemzs based on pagination</response>
        /// <response code="404">No Itemzs were found</response>
        [HttpGet("{ItemzTypeId:Guid}", Name = "__GET_Itemzs_By_ItemzType__")]
        [HttpHead("{ItemzTypeId:Guid}", Name = "__HEAD_Itemzs_By_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetItemzDTO>>> GetItemzsByItemzTypeAsync(Guid ItemzTypeId,
            [FromQuery] ItemzResourceParameter itemzResourceParameter)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<GetItemzDTO, Itemz>
                (itemzResourceParameter.OrderBy))
            {
                _logger.LogWarning("{FormattedControllerAndActionNames}Requested Order By Field {OrderByFieldName} is not found. Property Validation Failed!",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzResourceParameter.OrderBy);
                return BadRequest();
            }

            if(!(await _itemzRepository.ItemzTypeExistsAsync(ItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with ID {ItemzTypeID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    ItemzTypeId);
                return NotFound();
            }

            var itemzsFromRepo = _itemzRepository.GetItemzsByItemzType(ItemzTypeId, itemzResourceParameter);
            // EXPLANATION : Check if list is IsNullOrEmpty
            // By default we don't have option baked in the .NET to check
            // for null or empty for List type. In the following code we are first checking
            // for nullable itemzsFromRepo? and then for count great then zero via Any()
            // If any of above is true then we return true. This way we log that no itemz were
            // found in the database.
            // Ref: https://stackoverflow.com/a/54549818
            if (!itemzsFromRepo?.Any() ?? true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Items found in ItemzType with ID {ItemzTypeID}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    ItemzTypeId);
                // TODO: If no itemz are found in a ItemzType then shall we return an error back to the calling client?
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ItemzNumbers} Itemz found in ItemzType with ID {ItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzsFromRepo?.TotalCount, ItemzTypeId);
            var previousPageLink = itemzsFromRepo!.HasPrevious ?
                CreateItemzTypeItemzResourceUri(itemzResourceParameter,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = itemzsFromRepo.HasNext ?
                CreateItemzTypeItemzResourceUri(itemzResourceParameter,
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
        /// Get count of Itemzs associated with ItemzType
        /// </summary>
        /// <param name="ItemzTypeId">Provide ItemzType Id in GUID form</param>
        /// <returns>Integer representing total number of Itemzs associated with ItemzType</returns>
        /// <response code="200">Count of Itemzs associated with ItemzType. ZERO means no Itemz were found for targeted ItemzType</response>
        /// <response code="404">ItemzType for given ID could not be found</response>
        [HttpGet("GetItemzCount/{ItemzTypeId:Guid}", Name = "__GET_Itemz_Count_By_ItemzType__")]
        [HttpHead("GetItemzCount/{ItemzTypeId:Guid}", Name = "__HEAD_Itemz_Count_By_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<int>> GetItemzCountByItemzType(Guid ItemzTypeId)
        {

            if (!(await _itemzRepository.ItemzTypeExistsAsync(ItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with ID {ItemzTypeID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeId);
                return NotFound();
            }
            var countOfItemzs = -1;
            countOfItemzs = await _itemzRepository.GetItemzsCountByItemzType(ItemzTypeId);
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {countOfItemzs} Itemzs were found associated with ItemzType with ID {ItemzTypeID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                countOfItemzs,
                ItemzTypeId);
            return countOfItemzs;
        }

        /// <summary>
        /// Check if specific ItemzType and Itemz association exists
        /// </summary>
        /// <param name="ItemzTypeId">Provide ItemzType Id</param>
        /// <param name="itemzId">Provide Itemz Id</param>
        /// <returns>GetItemzDTO for the Itemz that has specified ItemzType association</returns>
        /// <response code="200">Returns GetItemzDTO for the Itemz that has specified ItemzType association</response>
        /// <response code="404">No ItemzType and Itemzs association was found</response>
        [HttpGet("CheckExists/", Name = "__GET_Check_ItemzType_Itemz_Association_Exists__")]
        [HttpHead("CheckExists/", Name = "__HEAD_Check_ItemzType_Itemz_Association_Exists__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<GetItemzDTO>> CheckItemzTypeItemzAssociationExistsAsync([FromQuery, BindRequired] Guid ItemzTypeId
            , [FromQuery, BindRequired] Guid itemzId) // TODO: Try from Query.
        {
            var tempItemzTypeItemzDTO = new ItemzTypeItemzDTO();

            tempItemzTypeItemzDTO.ItemzTypeId = ItemzTypeId;
            tempItemzTypeItemzDTO.ItemzId = itemzId;
            if (!(await _itemzRepository.ItemzTypeItemzExistsAsync(tempItemzTypeItemzDTO)))  // Check if ItemzTypeItemz association exists or not
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType ID {ItemzTypeId} and Itemz ID {ItemzId} association could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    tempItemzTypeItemzDTO.ItemzTypeId,
                    tempItemzTypeItemzDTO.ItemzId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType ID {ItemzTypeId} and Itemz ID {ItemzId} association was found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    tempItemzTypeItemzDTO.ItemzTypeId,
                    tempItemzTypeItemzDTO.ItemzId);
            return RedirectToRoute("__Single_Itemz_By_GUID_ID__", new { Controller = "Itemzs", ItemzId = tempItemzTypeItemzDTO.ItemzId });

        }

        /// <summary>
        /// Used for creating single new Itemz record in the repository by ItemzType ID
        /// </summary>
        /// <param name="createItemzDTO">Used for populating information in the newly created itemz in the repository by ItemzType ID</param>
        /// <param name="ItemzTypeId">ItemzType ID in Guid Form. New Itemzs will be associated with provided ItemzType Id</param>
        /// <param name="AtBottomOfChildNodes">New Itemz will be added at the bottom of existing child Itemz records under target ItemzType ID. 
        /// By default new Itemz will be created at the bottom. To create new Itemz at the TOP of the existing child Itemz list then use 'false' </param>
        /// <returns>Newly created Itemzs property details</returns>
        /// <response code="201">Returns newly created itemzs property details</response>
        /// <response code="400">Validation Issue OR Bad Request encountered while creating new Itemz under target ItemzType</response>
        [HttpPost("CreateSingleItemz/{ItemzTypeId:Guid}", Name = "__POST_Create_Single_Itemz_By_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<GetItemzDTO>> CreateItemzByItemzTypeAsync(
                        [FromBody] CreateItemzDTO createItemzDTO
                        , [FromRoute] Guid ItemzTypeId
                        , [FromQuery] bool AtBottomOfChildNodes = true)
        {
            if (!(await _itemzRepository.ItemzTypeExistsAsync(ItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with ID {ItemzTypeID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeId);
                return NotFound();
            }

            Itemz itemzEntity;

            try
            {
                itemzEntity = _mapper.Map<Entities.Itemz>(createItemzDTO);
            }
            catch (AutoMapper.AutoMapperMappingException amm_ex)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Could not create new Itemz due to issue with value provided for {fieldname}",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                        amm_ex.MemberMap.DestinationName);
                return ValidationProblem();
            }

            _itemzRepository.AddItemzByItemzType(itemzEntity, ItemzTypeId);
            // await _itemzRepository.SaveAsync(); // MAY BE THIS WOULD BE NEEDED TO ADD NEXT HIERARCHY RECORD IN THE REPO.

            //await _itemzRepository.AddNewItemzHierarchyByItemzTypeIdAsync(itemzEntity.Id, ItemzTypeId, atBottomOfChildNodes: AtBottomOfChildNodes);
            await _itemzRepository.MoveItemzHierarchyAsync(itemzEntity.Id, ItemzTypeId
                , atBottomOfChildNodes: AtBottomOfChildNodes
                , movingItemzName: itemzEntity.Name);
            await _itemzRepository.SaveAsync();

            _logger.LogDebug("{FormattedControllerAndActionNames}Created new Itemz with ID {ItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzEntity.Id);
            return CreatedAtRoute("__Single_Itemz_By_GUID_ID__", new { ItemzId = itemzEntity.Id },
                _mapper.Map<GetItemzDTO>(itemzEntity) // Converting to DTO as this is going out to the consumer
                );
        }


        /// <summary>
        /// Used for creating new multiple Itemz record in the repository by ItemzType ID
        /// </summary>
        /// <param name="ItemzTypeId">ItemzType ID in Guid Form. New Itemzs will be associated with provided ItemzType Id</param>
        /// <param name="itemzCollection">Used for populating information in the newly created itemz in the repository by ItemzType ID</param>
        /// <returns>Newly created Itemzs property details</returns>
        /// <response code="201">Returns newly created itemzs property details</response>
        [HttpPost("{ItemzTypeId:Guid}", Name = "__POST_Create_Itemz_Collection_By_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetItemzDTO>>> CreateItemzCollectionByItemzTypeAsync(
             Guid ItemzTypeId,
            IEnumerable<CreateItemzDTO> itemzCollection)
        {
            if (!(await _itemzRepository.ItemzTypeExistsAsync(ItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with ID {ItemzTypeID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    ItemzTypeId);
                return NotFound();
            }

            var itemzEntities = _mapper.Map<IEnumerable<Entities.Itemz>>(itemzCollection);
            foreach (var itemz in itemzEntities)
            {
                _itemzRepository.AddItemzByItemzType(itemz, ItemzTypeId);
            }

            // EXPLANATION: WE SAVE ALL ITEMZ IN THE REPOSITORY FIRST BECAUSE WE ARE ADDING
            // ONE HIERARCHY RECORD AT A TIME AND SAVING EACH RECORD INDIVIDUALLY. WE MAY 
            // HAVE TO COME-UP WITH BETTER OPTION IN THE FUTURE BUT FOR NOW WE ARE FIRST SAVING
            // ALL ITEMZ RECORDS BEFORE WE ADD CORRESPONDING ITEMZ HIERARCHY RECORD IN THE REPOSITORY

             await _itemzRepository.SaveAsync(); 

            foreach (var itemz in itemzEntities)
            {
                //await _itemzRepository.AddNewItemzHierarchyByItemzTypeIdAsync(itemz.Id, ItemzTypeId, atBottomOfChildNodes: true);

                await _itemzRepository.MoveItemzHierarchyAsync(itemz.Id, ItemzTypeId
                    , atBottomOfChildNodes: true
                    , movingItemzName: itemz.Name);

                // EXPLANATION: To be able to get next correct HierarchyId, we have to save previous
                // record in the database. Then only we are able to find next available HierarchyID to be
                // used for the next record in the collection. 
                // TODO: Perhaps we can implement better logic here to do bulk import within HierarchyId 
                // in the future. That said, we are not expecting people to keep importing large number
                // of records in the DB. Few seconds to import 100 record is perhaps acceptable.

                await _itemzRepository.SaveAsync();
            }

            var itemzCollectionToReturn = _mapper.Map<IEnumerable<GetItemzDTO>>(itemzEntities);
            var idConvertedToString = string.Join(",", itemzCollectionToReturn.Select(a => a.Id));

            _logger.LogDebug("{FormattedControllerAndActionNames}Created {NumberOfItemzCreated} number of new Itemz and associated to ItemzType Id {ItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzCollectionToReturn.Count(),
                ItemzTypeId);
            return CreatedAtRoute("__GET_Itemz_Collection_By_GUID_IDS__",
                new { Controller = "ItemzCollection", ids = idConvertedToString }, itemzCollectionToReturn);
        }

        /// <summary>
        /// Used for Associating Orphaned Itemz to ItemzType either at the TOP or BOTTOM of the existing child Itemz nodes
        /// </summary>
        /// <param name="ItemzTypeItemzDTO">Used for Associating Itemz to ItemzType through ItemzId and ItemzTypeId Respectively</param>
        /// <param name="AtBottomOfChildNodes">Used to indicate if Itemz should be added at Bottom or Top of existing Child Itemz list</param>
        /// <returns>GetItemzDTO for the Itemz that has specified ItemzType association</returns>
        /// <response code="200">Itemz to ItemzType association was either found or added successfully</response>
        /// <response code="400">Itemz is already associated with another ItemzType and it's not an Orphaned</response>
        /// <response code="404">Either Itemz or ItemzType was not found </response>
        [HttpPost(Name = "__POST_Associate_Itemz_To_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<GetItemzDTO>> AssociateItemzToItemzTypeAsync([FromBody] ItemzTypeItemzDTO ItemzTypeItemzDTO, [FromQuery] bool AtBottomOfChildNodes = true)
        {
            if (!(await _itemzRepository.ItemzTypeExistsAsync(ItemzTypeItemzDTO.ItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType with ID {ItemzTypeID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    ItemzTypeItemzDTO.ItemzTypeId);
                return NotFound();
            }
            if (!(await _itemzRepository.ItemzExistsAsync(ItemzTypeItemzDTO.ItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {itemzID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    ItemzTypeItemzDTO.ItemzId);
                return NotFound();
            }

            if((await _itemzRepository.ItemzTypeItemzExistsAsync(ItemzTypeItemzDTO)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType Itemz Association already existis for ItemzType ID {ItemzTypeID}" +
                    " and Itemz Id {itemzId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeItemzDTO.ItemzTypeId, ItemzTypeItemzDTO.ItemzId);
                return RedirectToRoute("__Single_Itemz_By_GUID_ID__", new { Controller = "Itemzs", ItemzId = ItemzTypeItemzDTO.ItemzId });
            }

            if (!(await _itemzRepository.IsOrphanedItemzAsync(ItemzTypeItemzDTO.ItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {itemzID} is not an Orphaned Itemz.",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeItemzDTO.ItemzId);
                return BadRequest();
            }

            // _itemzRepository.AssociateItemzToItemzType(ItemzTypeItemzDTO, AtBottomOfChildNodes);
            await _itemzRepository.MoveItemzHierarchyAsync(ItemzTypeItemzDTO.ItemzId
                ,ItemzTypeItemzDTO.ItemzTypeId
                , AtBottomOfChildNodes);
            await _itemzRepository.SaveAsync();
            _logger.LogDebug("{FormattedControllerAndActionNames}ItemzType Itemz Association was either created or found for ItemzType ID {ItemzTypeID}" +
                " and Itemz Id {itemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                ItemzTypeItemzDTO.ItemzTypeId, ItemzTypeItemzDTO.ItemzId);

            return RedirectToRoute("__Single_Itemz_By_GUID_ID__", new { Controller = "Itemzs", ItemzId = ItemzTypeItemzDTO.ItemzId });
        }

        ///// <summary>
        ///// Move Itemz from one ItemzType to another
        ///// </summary>
        ///// <param name="ItemzTypeId">GUID representing an unique ID of the Source ItemzType from which Itemz has to be moved</param>
        ///// <param name="targetItemzTypeItemzDTO">Details about target ItemzType and Itemz association</param>
        ///// <returns>No contents are returned when expected ItemzType and Itemz association is established</returns>
        ///// <response code="204">No content are returned but status of 204 indicated that expected ItemzType and Itemz association is established</response>
        ///// <response code="404">Either Itemz or ItemzType was not found</response>
        //[HttpPut("{ItemzTypeId}", Name = "__PUT_Move_Itemz_Between_ItemzTypes__")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesDefaultResponseType]
        //public async Task<ActionResult> MoveItemzBetweenItemzTypesAsync(Guid ItemzTypeId, ItemzTypeItemzDTO targetItemzTypeItemzDTO)
        //{
        //    if (!(await _itemzRepository.ItemzExistsAsync(targetItemzTypeItemzDTO.ItemzId)))// Check if Itemz exists

        //    {
        //        _logger.LogDebug("{FormattedControllerAndActionNames}Itemz for ID {ItemzId} could not be found",
        //            ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
        //            targetItemzTypeItemzDTO.ItemzId);
        //        return NotFound();
        //    }
        //    if (!(await _itemzRepository.ItemzTypeExistsAsync(targetItemzTypeItemzDTO.ItemzTypeId)))  // Check if Target ItemzType Exists
        //    {
        //        _logger.LogDebug("{FormattedControllerAndActionNames}Target ItemzType for ID {ItemzTypeId} could not be found",
        //            ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
        //            targetItemzTypeItemzDTO.ItemzTypeId);
        //        return NotFound();
        //    }

        //    var sourceItemzTypeItemzDTO = new ItemzTypeItemzDTO();
        //    sourceItemzTypeItemzDTO.ItemzId = targetItemzTypeItemzDTO.ItemzId;
        //    sourceItemzTypeItemzDTO.ItemzTypeId = ItemzTypeId;

        //    if (!(await _itemzRepository.ItemzTypeItemzExistsAsync(sourceItemzTypeItemzDTO)))  // Check if Source ItemzTypeItemz association exists or not
        //    {
        //        _logger.LogDebug("{FormattedControllerAndActionNames}Source ItemzType ID {ItemzTypeId} and Itemz ID {ItemzId} association could not be found",
        //            ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
        //            sourceItemzTypeItemzDTO.ItemzTypeId,
        //            sourceItemzTypeItemzDTO.ItemzId);

        //    }
        //    //_itemzRepository.MoveItemzFromOneItemzTypeToAnother(sourceItemzTypeItemzDTO, targetItemzTypeItemzDTO, atBottomOfChildNodes: true);
        //    await _itemzRepository.MoveItemzHierarchyAsync(targetItemzTypeItemzDTO.ItemzId, targetItemzTypeItemzDTO.ItemzTypeId, atBottomOfChildNodes: true);
        //                await _itemzRepository.SaveAsync();

        //    _logger.LogDebug("{FormattedControllerAndActionNames}Itemz ID {ItemzId} move from Source ItemzType ID {sourceItemzTypeID} " +
        //        "to Target ItemzType ID {targetItemzTypeID} was successfully completed",
        //        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
        //        sourceItemzTypeItemzDTO.ItemzId,
        //        sourceItemzTypeItemzDTO.ItemzTypeId,
        //        targetItemzTypeItemzDTO.ItemzTypeId);
        //    return NoContent(); // This indicates that update was successfully saved in the DB.

        //}

        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }


        /// <summary>
        /// Deleting a specific Itemz and ItemzType association. This will not delete Itemz or ItemzType from the database,
        /// instead it will only remove their association if found. 
        /// </summary>
        /// <returns>Status code 204 is returned without any content indicating that deletion of the specified ItemzType and Itemz association was successful</returns>
        /// <response code="404">ItemzType and Itemz association not found</response>
        [HttpDelete(Name = "__DELETE_ItemzType_and_Itemz_Association__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteItemzTypeAndItemzAssociationAsync(ItemzTypeItemzDTO ItemzTypeItemzDTO)
        {
            if (!(await _itemzRepository.ItemzTypeItemzExistsAsync(ItemzTypeItemzDTO)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find ItemzType and Itemz asscoaition for ItemzType ID " +
                    "{ItemzTypeId} and Itemz ID {ItemzId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzTypeItemzDTO.ItemzTypeId,
                    ItemzTypeItemzDTO.ItemzId);
                return NotFound($"Cannot find ItemzType and Itemz asscoaition for ItemzType ID {ItemzTypeItemzDTO.ItemzTypeId} and Itemz ID {ItemzTypeItemzDTO.ItemzId}");
            }

            try
            {
				_itemzRepository.RemoveItemzFromItemzType(ItemzTypeItemzDTO);
                await _itemzRepository.SaveAsync();
            }
			catch
			{
				return Conflict($"Issue encountered while trying to delete ItemzType ID '{ItemzTypeItemzDTO.ItemzTypeId}' and Itemz ID '{ItemzTypeItemzDTO.ItemzId}' ");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Delete ItemzType and Itemz asscoaition for ItemzType ID " +
                "{ItemzTypeId} and Itemz ID {ItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                ItemzTypeItemzDTO.ItemzTypeId, 
                ItemzTypeItemzDTO.ItemzId);
            return NoContent();
        }

        /// <summary>
        /// Get list of supported HTTP Options for the Itemz controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions (Name ="__OPTIONS_for_ItemzType_Itemz_Controller__")]
        public IActionResult GetItemzTypeItemzOptions()
        {
            Response.Headers.Add("Allow","GET,HEAD,POST,PUT,DELETE,OPTIONS");
            return Ok();
        }

        private string CreateItemzTypeItemzResourceUri(
            ItemzResourceParameter itemzResourceParameter,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("__GET_Itemzs_By_ItemzType__",
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber - 1,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
                case ResourceUriType.NextPage:
                    return Url.Link("__GET_Itemzs_By_ItemzType__",
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber + 1,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
                default:
                    return Url.Link("__GET_Itemzs_By_ItemzType__",
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
