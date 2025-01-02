// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    [Route("api/ItemzChangeHistoryByItemzType")] // e.g. http://HOST:PORT/api/ItemzChangeHistoryByItemzType
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzChangeHistoryByItemzTypeController : ControllerBase
    {
        private readonly IItemzChangeHistoryByItemzTypeRepository _itemzChangeHistoryByItemzTypeRepository;
        private readonly ILogger<ItemzChangeHistoryByItemzTypeController> _logger;

        public ItemzChangeHistoryByItemzTypeController(
            IItemzChangeHistoryByItemzTypeRepository itemzChangeHistoryByItemzTypeRepository,
            ILogger<ItemzChangeHistoryByItemzTypeController> logger)
        {
            _itemzChangeHistoryByItemzTypeRepository = itemzChangeHistoryByItemzTypeRepository;
            _logger = logger;
        }

        /// <summary>
        /// Deleting ItemzChangeHistory for all the Itemz that are associated with given ItemzType ID upto provided Date and Time.
        /// </summary>
        /// <param name="deleteItemzChangeHistoryByItemzTypeDTO">Provide ItemzTypeID representated in GUID form along with Upto Date Time indicating till the time associated Itemz Change History data has to be deleted.</param>
        /// <returns>Status code 200 is returned without any content indicating that action to delete Itemz Change History by Itemz Type was successful. Either it found older records to be deleted or it did not find any records to be deleted.</returns>
        /// <response code="200">Returns number of Itemz Change History records that were deleted by Itemz Type</response>
        [HttpDelete(Name = "__DELETE_Itemz_Change_History_By_ItemzType_GUID_ID__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ProducesDefaultResponseType]
        public async Task<ActionResult<int>> DeleteItemzChangeHistoryByItemzTypeAsync(DeleteChangeHistoryDTO deleteItemzChangeHistoryByItemzTypeDTO)
        {

			int numberOfDeletedRecords = 0;
            try
            {
                numberOfDeletedRecords = await _itemzChangeHistoryByItemzTypeRepository.DeleteItemzChangeHistoryByItemzTypeAsync(deleteItemzChangeHistoryByItemzTypeDTO.Id, deleteItemzChangeHistoryByItemzTypeDTO.UptoDateTime);
            }
			catch (DbUpdateException dbEx)
			{
				_logger.LogError(dbEx, "Database update error while deleting Itemz Change History for Project ID {ProjectID} upto Date Time {UptoDateTime}",
					deleteItemzChangeHistoryByItemzTypeDTO.Id, deleteItemzChangeHistoryByItemzTypeDTO.UptoDateTime);
				return StatusCode(StatusCodes.Status500InternalServerError, "A database error occurred while processing Delete Itemz Change History by ItemzType.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting Itemz Change History for Project ID {ProjectID} upto Date Time {UptoDateTime}",
					deleteItemzChangeHistoryByItemzTypeDTO.Id, deleteItemzChangeHistoryByItemzTypeDTO.UptoDateTime);
				return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your Delete Itemz Change History by ItemzType.");
			}

			_logger.LogDebug("{FormattedControllerAndActionNames}Deleted {numberOfDeletedRecords} record(s) from Itemz Change History associated with Itemz Type ID {Id} upto Date Time {UptoDateTime}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                numberOfDeletedRecords, 
                deleteItemzChangeHistoryByItemzTypeDTO.Id,
                deleteItemzChangeHistoryByItemzTypeDTO.UptoDateTime);
            
            return Ok(numberOfDeletedRecords);
        }

        /// <summary>
        /// Number of ItemzChangeHistory records for all the Itemz that are associated with given ItemzType ID
        /// </summary>
        /// <param name="ItemzTypeId">Provide ItemzTypeID representated in GUID form</param>
        /// <returns>Number of records found for ItemzChangeHistory indirectly associated with a given ItemzTypeID</returns>
        /// <response code="200">Returns number of Itemz Change History records that were indirectly associated with a given Itemz Type</response>
        [HttpGet("{ItemzTypeId:Guid}", Name = "__GET_Number_of_ItemzChangeHistory_By_ItemzType__")]
        [HttpHead("{ItemzTypeId:Guid}", Name = "__HEAD_Number_of_ItemzChangeHistory_By_ItemzType__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetNumberOfItemzChangeHistoryByItemzTypeAsync(Guid ItemzTypeId)
        {
            var foundItemzChangeHistoryByItemzTypeId = await _itemzChangeHistoryByItemzTypeRepository.TotalNumberOfItemzChangeHistoryByItemzTypeAsync(ItemzTypeId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundItemzChangeHistoryByItemzTypeId} ItemzChangeHistory records for ItemzType with ID {ItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundItemzChangeHistoryByItemzTypeId,
                ItemzTypeId);
            return foundItemzChangeHistoryByItemzTypeId;
        }

        /// <summary>
        /// Number of ItemzChangeHistory records for all the Itemz that are associated with given ItemzType ID upto provided Date and Time.
        /// </summary>
        /// <param name="getItemzChangeHistoryByItemzTypeDTO">Provide ItemzTypeID representated in GUID form along with cut off upto DateTime.</param>
        /// <returns>Number of records found for ItemzChangeHistory indirectly associated with a given ItemzTypeID</returns>
        /// <response code="200">Returns number of Itemz Change History records that were indirectly associated with a given Itemz Type upto provided Date and Time.</response>
        [HttpGet(Name = "__GET_Number_of_ItemzChangeHistory_By_ItemzType_Upto_DateTime__")]
        [HttpHead(Name = "__HEAD_Number_of_ItemzChangeHistory_By_ItemzType_Upto_DateTime__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetNumberOfItemzChangeHistoryByItemzTypeUptoDateTimeAsync(GetNumberOfChangeHistoryDTO getItemzChangeHistoryByItemzTypeDTO)
        {
            var foundItemzChangeHistoryByItemzTypeId = await _itemzChangeHistoryByItemzTypeRepository.TotalNumberOfItemzChangeHistoryByItemzTypeUptoDateTimeAsync(
                    getItemzChangeHistoryByItemzTypeDTO.Id,
                    getItemzChangeHistoryByItemzTypeDTO.UptoDateTime);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundItemzChangeHistoryByItemzTypeId} ItemzChangeHistory records for ItemzType with ID {ItemzTypeId} upto Date Time {UptoDateTime}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundItemzChangeHistoryByItemzTypeId,
                getItemzChangeHistoryByItemzTypeDTO.Id,
                getItemzChangeHistoryByItemzTypeDTO.UptoDateTime);
            return foundItemzChangeHistoryByItemzTypeId;
        }

        /// <summary>
        /// Get list of supported HTTP Options for the ItemzChangeHistoryByItemzType controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions(Name = "__OPTIONS_ItemzChangeHistory_By_ItemzType__")]
        public IActionResult GetItemzChangeHistoryByItemzTypeOptions()
        {
            Response.Headers.Add("Allow", "GET,HEAD,DELETE,OPTIONS");
            return Ok();
        }
    }
}
