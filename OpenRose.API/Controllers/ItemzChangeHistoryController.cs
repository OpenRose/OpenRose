// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ItemzApp.API.Models;
using AutoMapper;
using ItemzApp.API.Helper;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    [Route("api/ItemzChangeHistory")] // e.g. http://HOST:PORT/api/ItemzChangeHistory
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzChangeHistoryController : ControllerBase
    {
        private readonly IItemzChangeHistoryRepository _itemzChangeHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ItemzChangeHistoryController> _logger;

        public ItemzChangeHistoryController(IItemzChangeHistoryRepository itemzChangeHistoryRepository,
            IMapper mapper,
            ILogger<ItemzChangeHistoryController> logger)
        {
            _itemzChangeHistoryRepository = itemzChangeHistoryRepository ?? throw new ArgumentNullException(nameof(itemzChangeHistoryRepository)); ;
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get collection of Itemz Change History by Itemz ID (represented by a GUID)
        /// </summary>
        /// <param name="ItemzId">GUID representing an unique ID of the Itemz that you want to get change history for</param>
        /// <returns>A collection of Itemz Change History records based on provided Itemz ID (GUID) </returns>
        /// <response code="200">Returns the requested Itemz Change History Records</response>
        /// <response code="404">Requested Itemz and/or it's change history records not found</response>
        /// 

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetItemzChangeHistoryDTO>))]
        [HttpGet("{ItemzId:Guid}",
            Name = "__GET_ItemzChangeHistory_By_GUID_ItemzID__")] // e.g. http://HOST:PORT/api/ItemzChangeHistory/9153a516-d69e-4364-b17e-03b87442e21c
        [HttpHead("{ItemzId:Guid}", Name = "__HEAD_ItemzChangeHistory_By_GUID_ItemzID__")]
        public async Task<ActionResult<IEnumerable<GetItemzChangeHistoryDTO>>> GetItemzChangeHistoryAsync(Guid ItemzId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get ItemzChangeHistory for ID {ItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                ItemzId);
            var itemzChangeHistoryFromRepo = await _itemzChangeHistoryRepository.GetItemzChangeHistoryAsync(ItemzId);

            if (itemzChangeHistoryFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}ItemzChangeHistory for ID {ItemzId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                    ItemzId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Found ItemzChangeHistory for ID {ItemzId} and now returning results",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                ItemzId);
            return Ok(_mapper.Map<IEnumerable<GetItemzChangeHistoryDTO>>(itemzChangeHistoryFromRepo)); 
        }

        /// <summary>
        /// Deleting ItemzChangeHistory for a given ItemzID upto provided Date and Time.
        /// </summary>
        /// <param name="deleteItemzChangeHistoryDTO">Provide ItemzID representated in GUID form along with Upto Date Time indicating till the time Itemz Change History data has to be deleted.</param>
        /// <returns>Status code 204 is returned without any content indicating that action to delete Itemz Change History was successful. Either it found older records to be deleted or it did not find any records to be deleted.</returns>
        /// <response code="200">Returns number of Itemz Change History records that were deleted</response>
        [HttpDelete(Name = "__DELETE_ItemzChangeHistory_By_GUID_ID__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> DeleteItemzChangeHistoryAsync(DeleteChangeHistoryDTO deleteItemzChangeHistoryDTO)
        {
            var numberOfDeletedRecords = await _itemzChangeHistoryRepository.DeleteItemzChangeHistoryAsync(deleteItemzChangeHistoryDTO.Id,deleteItemzChangeHistoryDTO.UptoDateTime);

            _logger.LogDebug("{FormattedControllerAndActionNames}Deleted {numberOfDeletedRecords} record(s) from Itemz Change History for ItemzID {ItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext), 
                numberOfDeletedRecords, deleteItemzChangeHistoryDTO.Id );
            return Ok(numberOfDeletedRecords);
        }

        /// <summary>
        /// Get list of supported HTTP Options for the ItemzChangeHistory controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions(Name = "__OPTIONS_ItemzChangeHistory__")]
        public IActionResult GetItemzOptions()
        {
            Response.Headers.Add("Allow", "GET,HEAD,DELETE,OPTIONS");
            return Ok();
        }
    }
}
