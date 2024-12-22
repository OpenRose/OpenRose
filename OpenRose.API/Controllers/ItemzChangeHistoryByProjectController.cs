// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    [Route("api/ItemzChangeHistoryByProject")] // e.g. http://HOST:PORT/api/ItemzChangeHistoryByProject
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzChangeHistoryByProjectController : ControllerBase
    {
        private readonly IItemzChangeHistoryByProjectRepository _itemzChangeHistoryByProjectRepository;
        private readonly ILogger<ItemzChangeHistoryByProjectController> _logger;

        public ItemzChangeHistoryByProjectController(
            IItemzChangeHistoryByProjectRepository itemzChangeHistoryByProjectRepository,
            ILogger<ItemzChangeHistoryByProjectController> logger)
        {
            _itemzChangeHistoryByProjectRepository = itemzChangeHistoryByProjectRepository;
            _logger = logger;
        }

        /// <summary>
        /// Deleting ItemzChangeHistory for all the Itemz that are associated with given Project ID upto provided Date and Time.
        /// </summary>
        /// <param name="deleteItemzChangeHistoryByProjectDTO">Provide ProjectID representated in GUID form along with Upto Date Time indicating till the time associated Itemz Change History data has to be deleted.</param>
        /// <returns>Status code 200 is returned without any content indicating that action to delete Itemz Change History by Itemz Type was successful. Either it found older records to be deleted or it did not find any records to be deleted.</returns>
        /// <response code="200">Returns number of Itemz Change History records that were deleted by Itemz Type</response>
        [HttpDelete(Name = "__DELETE_Itemz_Change_History_By_Project_GUID_ID__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> DeleteItemzChangeHistoryByProjectAsync(DeleteChangeHistoryDTO deleteItemzChangeHistoryByProjectDTO)
        {
            var numberOfDeletedRecords = await _itemzChangeHistoryByProjectRepository.DeleteItemzChangeHistoryByProjectAsync(deleteItemzChangeHistoryByProjectDTO.Id, deleteItemzChangeHistoryByProjectDTO.UptoDateTime);

            _logger.LogDebug("{FormattedControllerAndActionNames}Deleted {numberOfDeletedRecords} record(s) from Itemz Change History associated with Project ID {Id} upto Date Time {UptoDateTime}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                numberOfDeletedRecords, 
                deleteItemzChangeHistoryByProjectDTO.Id,
                deleteItemzChangeHistoryByProjectDTO.UptoDateTime);
            return Ok(numberOfDeletedRecords);
        }

        /// <summary>
        /// Number of ItemzChangeHistory records for all the Itemz that are associated with given Project ID
        /// </summary>
        /// <param name="ProjectId">Provide ProjectID representated in GUID form</param>
        /// <returns>Number of records found for ItemzChangeHistory indirectly associated with a given ProjectID</returns>
        /// <response code="200">Returns number of Itemz Change History records that were indirectly associated with a given Project</response>
        [HttpGet("{ProjectId:Guid}", Name = "__GET_Number_of_ItemzChangeHistory_By_Project__")]
        [HttpHead("{ProjectId:Guid}", Name = "__HEAD_Number_of_ItemzChangeHistory_By_Project__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetNumberOfItemzChangeHistoryByProjectAsync(Guid ProjectId)
        {
            var foundItemzChangeHistoryByProjectId = await _itemzChangeHistoryByProjectRepository.TotalNumberOfItemzChangeHistoryByProjectAsync(ProjectId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundItemzChangeHistoryByProjectId} ItemzChangeHistory records for Project with ID {ProjectId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundItemzChangeHistoryByProjectId,
                ProjectId);
            return foundItemzChangeHistoryByProjectId;
        }

        /// <summary>
        /// Number of ItemzChangeHistory records for all the Itemz that are associated with given Project ID upto provided Date and Time.
        /// </summary>
        /// <param name="getItemzChangeHistoryByProjectDTO">Provide ProjectID representated in GUID form along with cut off upto DateTime.</param>
        /// <returns>Number of records found for ItemzChangeHistory indirectly associated with a given ProjectID</returns>
        /// <response code="200">Returns number of Itemz Change History records that were indirectly associated with a given Project upto provided Date and Time.</response>
        [HttpGet(Name = "__GET_Number_of_ItemzChangeHistory_By_Project_Upto_DateTime__")]
        [HttpHead(Name = "__HEAD_Number_of_ItemzChangeHistory_By_Project_Upto_DateTime__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetNumberOfItemzChangeHistoryByProjectUptoDateTimeAsync(GetNumberOfChangeHistoryDTO getItemzChangeHistoryByProjectDTO)
        {
            var foundItemzChangeHistoryByProjectId = await _itemzChangeHistoryByProjectRepository.TotalNumberOfItemzChangeHistoryByProjectUptoDateTimeAsync(
                    getItemzChangeHistoryByProjectDTO.Id,
                    getItemzChangeHistoryByProjectDTO.UptoDateTime);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundItemzChangeHistoryByProjectId} ItemzChangeHistory records for Project with ID {ProjectId} upto Date Time {UptoDateTime}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundItemzChangeHistoryByProjectId,
                getItemzChangeHistoryByProjectDTO.Id,
                getItemzChangeHistoryByProjectDTO.UptoDateTime);
            return foundItemzChangeHistoryByProjectId;
        }

        /// <summary>
        /// Get list of supported HTTP Options for the ItemzChangeHistoryByProject controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions(Name = "__OPTIONS_ItemzChangeHistory_By_Project__")]
        public IActionResult GetItemzChangeHistoryByProjectOptions()
        {
            Response.Headers.Add("Allow", "GET,HEAD,DELETE,OPTIONS");
            return Ok();
        }
    }
}
