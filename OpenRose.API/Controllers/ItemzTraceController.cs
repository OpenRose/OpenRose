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
    [Route("api/ItemzTrace")] // e.g. http://HOST:PORT/api/ItemzTypeItemzs
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzTraceController : ControllerBase
    {
        private readonly IItemzTraceRepository _itemzTraceRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ILogger<ItemzTraceController> _logger;

        public ItemzTraceController(IItemzTraceRepository itemzTraceRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            ILogger<ItemzTraceController> logger)
        {
            _itemzTraceRepository = itemzTraceRepository ?? throw new ArgumentNullException(nameof(itemzTraceRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Check if specific Itemz Trace association exists
        /// </summary>
        /// <param name="fromTraceItemzId">Provide From Trace Itemz Id</param>
        /// <param name="toTraceItemzId">Provide To Trace Itemz Id</param>
        /// <returns>ItemzTraceDTO for the Itemz that has specified Itemz Trace</returns>
        /// <response code="200">Returns ItemzTraceDTO for the From and To Itemz Trace</response>
        /// <response code="404">Itemz Trace was not found</response>
        [HttpGet("CheckExists/", Name = "__GET_Check_Itemz_Trace_Exists__")]
        [HttpHead("CheckExists/", Name = "__HEAD_Check_Itemz_Trace_Exists__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<ItemzTraceDTO>> CheckItemzTraceExistsAsync([FromQuery] Guid fromTraceItemzId, Guid toTraceItemzId) // TODO: Try from Query.
        {
            var tempItemzTraceDTO = new ItemzTraceDTO();

            tempItemzTraceDTO.FromTraceItemzId = fromTraceItemzId;
            tempItemzTraceDTO.ToTraceItemzId = toTraceItemzId;
            if (!(await _itemzTraceRepository.ItemzsTraceExistsAsync(tempItemzTraceDTO)))  // Check if ItemzTrace association exists or not
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}From Itemz ID {fromTraceItemzId} and To Itemz ID {toTraceItemzId} Trace could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    tempItemzTraceDTO.FromTraceItemzId,
                    tempItemzTraceDTO.ToTraceItemzId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}From Itemz ID {fromTraceItemzId} and To Itemz ID {toTraceItemzId} Trace was found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    tempItemzTraceDTO.FromTraceItemzId,
                    tempItemzTraceDTO.ToTraceItemzId);
            return Ok(tempItemzTraceDTO);
        }


        /// <summary>
        /// Used for Establishing Trace link between Itemz 
        /// </summary>
        /// <param name="itemzTraceDTO">Used for Associating Trace between two Itemz </param>
        /// <returns>ItemzTraceDTO for the Itemz Trace Association</returns>
        /// <response code="200">Itemz Trace association was either found or added successfully</response>
        /// <response code="404">Either FromItemz or ToItemz was not found </response>
        [HttpPost(Name = "__POST_Establish_Trace_Between_Itemz__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ItemzTraceDTO>> EstablishTraceBetweenItemzAsync(ItemzTraceDTO itemzTraceDTO)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(itemzTraceDTO.FromTraceItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {ItemzTraceFromItemzID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzTraceDTO.FromTraceItemzId);
                return NotFound();
            }
            if (!(await _itemzTraceRepository.ItemzExistsAsync(itemzTraceDTO.ToTraceItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {ItemzTraceToItemzID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzTraceDTO.ToTraceItemzId);
                return NotFound();
            }

            await _itemzTraceRepository.EstablishTraceBetweenItemzAsync(itemzTraceDTO);
            await _itemzTraceRepository.SaveAsync();
            _logger.LogDebug("{FormattedControllerAndActionNames}Itemz Trace was either created or found between " +
                "Parent FromItemz {FromItemzTrace} and Child ToItemz {ToItemzTrace}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzTraceDTO.FromTraceItemzId, 
                itemzTraceDTO.ToTraceItemzId);

            return itemzTraceDTO;
        }

        /// <summary>
        /// Used for deleting Trace link between Itemz. This will not delete Itemz from the database,
        /// instead it will only remove their trace association if found. 
        /// </summary>
        /// <returns>Status code 204 is returned without any content indicating that deletion of the specified FromItemz and ToItemz trace association was successful</returns>
        /// <response code="404">FromItemz and ToItemz Trace not found</response>
        [HttpDelete(Name = "__DELETE_Itemz_Trace__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteItemzTraceAsync(ItemzTraceDTO itemzTraceDTO)
        {
            if (!(await _itemzTraceRepository.ItemzsTraceExistsAsync(itemzTraceDTO)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find trace asscoaition between " +
                    "Itemz ID {FromItemz} and Itemz ID {ToItemz}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzTraceDTO.FromTraceItemzId,
                    itemzTraceDTO.ToTraceItemzId);
                return NotFound($"Cannot find trace asscoaition between Itemz ID {itemzTraceDTO.FromTraceItemzId} and Itemz ID {itemzTraceDTO.ToTraceItemzId}");
            }
            try
            {
                var successResult = await _itemzTraceRepository.RemoveItemzTraceAsync(itemzTraceDTO);
                if (successResult == false)
                {
					return Conflict($"Issue encountered while trying to delete Trace between {itemzTraceDTO.FromTraceItemzId} and Itemz ID {itemzTraceDTO.ToTraceItemzId}");
				}
				await _itemzTraceRepository.SaveAsync();
            }
            catch
            {
				return Conflict($"Issue encountered while trying to delete Trace between {itemzTraceDTO.FromTraceItemzId} and Itemz ID {itemzTraceDTO.ToTraceItemzId}");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Deleted Itemz Trace between " +
                "Itemz ID {FromItemz} and Itemz ID {ToItemz}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzTraceDTO.FromTraceItemzId,
                itemzTraceDTO.ToTraceItemzId);
            return NoContent();
        }

        /// <summary>
        /// Used for deleting All From-Trace links from provided parent ItemzID.
        /// </summary>
        /// <param name="ItemzID">Itemz ID for which All From Itemz Traces are getting deleted</param>        
        /// <returns>Status code 204 is returned without any content indicating that deletion of All From Trace links for provided ItemzID was successful</returns>
        /// <response code="204">Returns No Content indicating success</response>
        /// <response code="404">Itemz with ItemzID was not found</response>
        [HttpDelete("DeleteAllFromItemzTraces/{ItemzID:Guid}", Name = "__DELETE_AllFromItemz_Trace__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteAllFromItemzTracesAsync(Guid ItemzID)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(ItemzID)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find Itemz with Itemz ID {ItemzID}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzID);
                return NotFound($"Cannot find Itemz with Itemz ID {ItemzID}");
            }
            int deletedFromItemzTraceCount = 0;

			try
            {
                deletedFromItemzTraceCount = await _itemzTraceRepository.RemoveAllFromItemzTraceAsync(ItemzID);
                await _itemzTraceRepository.SaveAsync();
			}
			catch
			{
				return Conflict($"Issue encountered while trying to delete All From Itemz Traces for Itemz ID {ItemzID}");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}In Total, deleted all {deletedFromItemzTraceCount} From ItemzTrace from " +
                "Itemz ID {FromItemz}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                deletedFromItemzTraceCount,
                ItemzID);

            return NoContent();
        }


        /// <summary>
        /// Used for deleting All To-Trace links from provided parent ItemzID.
        /// </summary>
        /// <param name="ItemzID">Itemz ID for which All To Itemz Traces are getting deleted</param>        
        /// <returns>Status code 204 is returned without any content indicating that deletion of All To Trace links for provided ItemzID was successful</returns>
        /// <response code="204">Returns No Content indicating success</response>
        /// <response code="404">Itemz with ItemzID was not found</response>
        [HttpDelete("DeleteAllToItemzTraces/{ItemzID:Guid}",  Name = "__DELETE_AllToItemz_Trace__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteAllToItemzTracesAsync(Guid ItemzID)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(ItemzID)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find Itemz with Itemz ID {ItemzID}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzID);
                return NotFound($"Cannot find Itemz with Itemz ID {ItemzID}");
            }

            int deletedToItemzTraceCount = 0;
			try
            {
                deletedToItemzTraceCount = await _itemzTraceRepository.RemoveAllToItemzTraceAsync(ItemzID);
                await _itemzTraceRepository.SaveAsync();
			}
			catch
			{
				return Conflict($"Issue encountered while trying to delete All To Itemz Traces for Itemz ID {ItemzID}");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}In Total, deleted all {deletedToItemzTraceCount} To ItemzTrace from " +
                "Itemz ID {FromItemz}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                deletedToItemzTraceCount,
                ItemzID);

            return NoContent();
        }

        /// <summary>
        /// Gets collection of Itemz Traces by Itemz ID
        /// </summary>
        /// <param name="itemzId">Itemz ID for which Itemz Traces are queried</param>
        /// <returns>Collection of Itemz Traces by Itemz ID</returns>
        /// <response code="200">Returns Collection of Itemz Traces by Itemz ID</response>
        /// <response code="404">Either ItemzID was not found or No Itemz Traces were found for given ItemzID</response>
        [HttpGet("{itemzId:Guid}", Name = "__GET_Itemz_Traces_By_ItemzID__")]
        [HttpHead("{itemzId:Guid}", Name = "__HEAD_Itemz_Traces_By_ItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<ItemzTraceDTO>>> GetItemzTracesByItemzIDAsync(Guid itemzId)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(itemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {itemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzId);
                return NotFound();
            }

            var itemzTracesFromRepo = await  _itemzTraceRepository.GetAllTracesByItemzIdAsync(itemzId);

            // EXPLANATION : Check if list is IsNullOrEmpty
            // By default we don't have option baked in the .NET to check
            // for null or empty for List type. In the following code we are first checking
            // for nullable itemzsFromRepo? and then for count great then zero via Any()
            // If any of above is true then we return true. This way we log that no itemz traces were
            // found in the database.
            // Ref: https://stackoverflow.com/a/54549818
            if (!itemzTracesFromRepo?.Any() ?? true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Itemz Traces found for Itemz with ID {ItemzId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzId);
                // TODO: If no Itemz Traces are found for an ItemzID then shall we return an error back to the calling client?
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ItemzTraceNumbers} Itemz Traces found in Itemz with ID {ItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzTracesFromRepo?.Count(), itemzId);

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {ItemzNumbers} Itemzs to the caller",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzTracesFromRepo?.Count());
            return Ok(_mapper.Map<IEnumerable<ItemzTraceDTO>>(itemzTracesFromRepo));
        }


        /// <summary>
        /// Gets All Parent and Child Itemz Traces byItemz ID
        /// </summary>
        /// <param name="itemzId">Itemz ID for which Parent and Child Itemz Traces are returned.</param>
        /// <returns>Collection of all Parent and Child Itemz Traces by Itemz ID</returns>
        /// <response code="200">Returns Collection of all Parent and Child Itemz Traces by Itemz ID</response>
        /// <response code="404">ItemzID was not found in the repository</response>
        [HttpGet("AllItemzTraces/{itemzId:Guid}", Name = "__GET_All_Parent_and_Child_Itemz_Traces_By_ItemzID__")]
        [HttpHead("AllItemzTraces/{itemzId:Guid}", Name = "__HEAD_All_Parent_and_Child_Itemz_Traces_By_ItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ItemzParentAndChildTraceDTO>> GetAllParentAndChildTracesByItemzIdAsync(Guid itemzId)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(itemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {itemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzId);
                return NotFound();
            }

            var itemzParentAndChildTraceDTO = await _itemzTraceRepository.GetAllParentAndChildTracesByItemzIdAsync(itemzId);

            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ParentItemzTraceCount} Parent Itemz Traces found for Itemz with ID {ItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzParentAndChildTraceDTO.Itemz?.ParentItemz?.Count, itemzId);

            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ChildItemzTraceCount} Child Itemz Traces found for Itemz with ID {ItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzParentAndChildTraceDTO.Itemz?.ChildItemz?.Count, itemzId);

            return Ok(itemzParentAndChildTraceDTO);
        }

        /// <summary>
        /// Get count of FromItemz Traces associated with ItemzID
        /// </summary>
        /// <param name="ItemzId">Provide ItemzId in GUID form</param>
        /// <returns>Integer representing total number of direct From Itemz Traces associated with ItemzID</returns>
        /// <response code="200">Count of From Itemz Traces associated with ItemzID. ZERO means no From Itemz Traces were found for targeted ItemzID</response>
        /// <response code="404">Itemz for given ID could not be found</response>
        [HttpGet("GetFromItemzTraceCount/{ItemzId:Guid}", Name = "__GET_FromItemz_Count_By_ItemzID__")]
        [HttpHead("GetFromItemzTraceCount/{ItemzId:Guid}", Name = "__HEAD_FromItemz_Count_By_ItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<int>> GetFromItemzTraceCountByItemzID(Guid ItemzId)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(ItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {itemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzId);
                return NotFound();
            }
            int countOfFromItemzTraces = await _itemzTraceRepository.GetFromTraceCountByItemz(ItemzId);
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {countOfFromItemzTracess} From Itemz Traces were found associated with ItemzID {ItemzID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                countOfFromItemzTraces,
                ItemzId);
            return Ok(countOfFromItemzTraces);
        }


        /// <summary>
        /// Get count of ToItemz Traces associated with ItemzID
        /// </summary>
        /// <param name="ItemzId">Provide ItemzId in GUID form</param>
        /// <returns>Integer representing total number of direct To Itemz Traces associated with ItemzID</returns>
        /// <response code="200">Count of To Itemz Traces associated with ItemzID. ZERO means no To Itemz Traces were found for targeted ItemzID</response>
        /// <response code="404">Itemz for given ID could not be found</response>
        [HttpGet("GetToItemzTraceCount/{ItemzId:Guid}", Name = "__GET_ToItemz_Count_By_ItemzID__")]
        [HttpHead("GetToItemzTraceCount/{ItemzId:Guid}", Name = "__HEAD_ToItemz_Count_By_ItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<int>> GetToItemzTraceCountByItemzID(Guid ItemzId)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(ItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {itemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzId);
                return NotFound();
            }
            int countOfToItemzTraces = await _itemzTraceRepository.GetToTraceCountByItemz(ItemzId);
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {countOfToItemzTracess} To Itemz Traces were found associated with ItemzID {ItemzID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                countOfToItemzTraces,
                ItemzId);
            return Ok(countOfToItemzTraces);
        }

        /// <summary>
        /// Get count of FromItemz and ToItemz Traces associated with ItemzID
        /// </summary>
        /// <param name="ItemzId">Provide ItemzId in GUID form</param>
        /// <returns>Integer representing total number of direct From and To Itemz Traces associated with ItemzID</returns>
        /// <response code="200">Count of From and To Itemz Traces associated with ItemzID. ZERO means no Direct Itemz Traces were found for targeted ItemzID</response>
        /// <response code="404">Itemz for given ID could not be found</response>
        [HttpGet("GetAllFromAndToTracesCountByItemzId/{ItemzId:Guid}", Name = "__GET_All_From_and_To_Itemz_Traces_Count_By_ItemzID__")]
        [HttpHead("GetAllFromAndToTracesCountByItemzId/{ItemzId:Guid}", Name = "__HEAD_All_From_and_To_Itemz_Traces_Count_By_ItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<int>> GetAllFromAndToTracesCountByItemzId(Guid ItemzId)
        {
            if (!(await _itemzTraceRepository.ItemzExistsAsync(ItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {itemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    ItemzId);
                return NotFound();
            }
            int countOfAllFromAndToTraces = await _itemzTraceRepository.GetAllFromAndToTracesCountByItemzIdAsync(ItemzId);
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {countOfAllFromAndToTraces} From and To Itemz Traces were found associated with ItemzID {ItemzID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                countOfAllFromAndToTraces,
                ItemzId);
            return Ok(countOfAllFromAndToTraces);
        }


        /// <summary>
        /// Used for bulk creating or verifiying that Itemz Trace record is present in the database
        /// </summary>
        /// <param name="itemzTraceDTOCollection">Array of ItemzTraceDTO used bulk creation or validation of Itemz Trace record is present in the database</param>
        /// <returns>Collection of created or verified ItemzTraceDTO property details</returns>
        /// <response code="200">Collection of created or verified ItemzTraceDTO property details</response>
        /// <response code="404">Either FromItemz or ToItemz was not found from within the itemzTraceDTOCollection </response>
        /// <response code="400">Provided itemzTraceDTOCollection is an empty list. Bad Request </response>
        [HttpPost("CreateOrVerifyItemzTraceCollection/", Name = "__POST_Create_Or_Verify_Itemz_Trace_Collection__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<ItemzTraceDTO>>> CreateOrVerifyItemzTraceCollectionAsync(
            IEnumerable<ItemzTraceDTO> itemzTraceDTOCollection)
        {
            if (!itemzTraceDTOCollection?.Any() ?? true)
            {

                _logger.LogDebug("{FormattedControllerAndActionNames}Provided an empty list for parameter itemzTraceDTOCollection",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));

                return BadRequest(ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext) + " itemzTraceDTOCollection can not be an empty list");
            }

            foreach (var itemzTraceDTO in itemzTraceDTOCollection!)
            {

                if (!(await _itemzTraceRepository.ItemzExistsAsync(itemzTraceDTO.FromTraceItemzId)))
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {FromTraceItemzId} was not found in the repository",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                        itemzTraceDTO.FromTraceItemzId);
                    return NotFound();
                }
                if (!(await _itemzTraceRepository.ItemzExistsAsync(itemzTraceDTO.ToTraceItemzId)))
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames}Itemz with ID {ToTraceItemzId} was not found in the repository",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                        itemzTraceDTO.ToTraceItemzId);
                    return NotFound();
                }
                await _itemzTraceRepository.EstablishTraceBetweenItemzAsync(itemzTraceDTO);
            }
            await _itemzTraceRepository.SaveAsync();

              _logger.LogDebug("{FormattedControllerAndActionNames}Created Or Verified {NumberOfItemzTraceCreatedOrVerified} ItemzTraces",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                itemzTraceDTOCollection.Count());
            return Ok(itemzTraceDTOCollection);
        }


        /// <summary>
        /// Used for bulk deleting of Itemz Trace records
        /// </summary>
        /// <param name="itemzTraceDTOCollection">Array of ItemzTraceDTO used for bulk deletion of Itemz Trace record</param>
        /// <returns>Success or Failure message for Bulk deleting of Itemz Trace request</returns>
        /// <response code="204">Indicating that request to bulk delete Itemz Traces completed successfully</response>
        /// <response code="404">Itemz Trace was not found for minimum of atleast one record from within the provided itemzTraceDTOCollection</response>
        /// <response code="400">Provided itemzTraceDTOCollection is an empty list. Bad Request </response>
        [HttpDelete("DeleteItemzTraceCollection/", Name = "__DELETE_Itemz_Trace_Collection__")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteItemzTraceCollectionAsync(
            IEnumerable<ItemzTraceDTO> itemzTraceDTOCollection)
        {
            if (!itemzTraceDTOCollection?.Any() ?? true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Provided an empty list for parameter itemzTraceDTOCollection",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));

                return BadRequest(ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext) + " itemzTraceDTOCollection can not be an empty list");
            }

            foreach (var itemzTraceDTO in itemzTraceDTOCollection!)
            {
                if(!await _itemzTraceRepository.ItemzsTraceExistsAsync(itemzTraceDTO))
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames}Itemz Trace between From Itemz ID {FromTraceItemzId} and To Itemz ID {ToTraceItemzId} was not found in the repository",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                        itemzTraceDTO.FromTraceItemzId,
                        itemzTraceDTO.ToTraceItemzId);
                    return NotFound($"Itemz Trace between From Itemz ID {itemzTraceDTO.FromTraceItemzId} and To Itemz ID {itemzTraceDTO.ToTraceItemzId} was not found in the repository");
                }
                
				var successResult = await _itemzTraceRepository.RemoveItemzTraceAsync(itemzTraceDTO);
				if (successResult == false)
				{
					return Conflict($"Issue encountered while trying to delete Trace between {itemzTraceDTO.FromTraceItemzId} and Itemz ID {itemzTraceDTO.ToTraceItemzId}");
				}
			}

            try
            {
                await _itemzTraceRepository.SaveAsync();
			}
			catch
			{
				return Conflict("Issue encountered while trying to delete Itemz Trace Collection");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Deleted {NumberOfItemzTraceDeleted} ItemzTraces",
              ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
              itemzTraceDTOCollection.Count());
            return NoContent();
        }
    }
}
