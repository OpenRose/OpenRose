// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    [Route("api/ItemzChangeHistoryByRepository")] // e.g. http://HOST:PORT/api/ItemzChangeHistoryByRepository
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class ItemzChangeHistoryByRepositoryController : ControllerBase
    {
        private readonly IItemzChangeHistoryByRepositoryRepository _itemzChangeHistoryByRepositoryRepository;
        private readonly ILogger<ItemzChangeHistoryByRepositoryController> _logger;

        public ItemzChangeHistoryByRepositoryController(
            IItemzChangeHistoryByRepositoryRepository itemzChangeHistoryByRepositoryRepository,
            ILogger<ItemzChangeHistoryByRepositoryController> logger)
        {
            _itemzChangeHistoryByRepositoryRepository = itemzChangeHistoryByRepositoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Count of number of ItemzChangeHistory records in the repository
        /// </summary>
        /// <returns>Number of records found for ItemzChangeHistory in the repository</returns>
        /// <response code="200">Returns number of ItemzChangeHistory records in the repository</response>
        [HttpGet(Name = "__GET_Number_of_ItemzChangeHistory_By_Repository__")]
        [HttpHead(Name = "__HEAD_Number_of_ItemzChangeHistory_By_Repository__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetNumberOfItemzChangeHistoryByRepositoryAsync()
        {
            var foundItemzChangeHistoryByRepository = await _itemzChangeHistoryByRepositoryRepository.TotalNumberOfItemzChangeHistoryByRepositoryAsync();
            _logger.LogDebug("{FormattedControllerAndActionNames}Found {foundItemzChangeHistoryByRepository} ItemzChangeHistory records in the repository",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundItemzChangeHistoryByRepository);
            return foundItemzChangeHistoryByRepository;
        }

        /// <summary>
        /// Number of ItemzChangeHistory records for all the Itemz within the repository upto provided Date and Time
        /// </summary>
        /// <param name="getNumberOfChangeHistoryByRepositoryDTO">Provide cut off upto DateTime</param>
        /// <returns>Number of records found for ItemzChangeHistory within the repository upto provided Date and Time</returns>
        /// <response code="200">Returns number of ItemzChangeHistory records within the repository upto provided Date and Time</response>
        [HttpGet("ByUptoDateTime/", Name = "__GET_Number_of_ItemzChangeHistory_By_Repository_Upto_DateTime__")]
        [HttpHead("ByUptoDateTime/", Name = "__HEAD_Number_of_ItemzChangeHistory_By_Repository_Upto_DateTime__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<ActionResult<int>> GetNumberOfItemzChangeHistoryByRepositoryUptoDateTimeAsync([FromBody] GetNumberOfChangeHistoryByRepositoryDTO getNumberOfChangeHistoryByRepositoryDTO)
        {
            var foundItemzChangeHistoryByRepository = await _itemzChangeHistoryByRepositoryRepository.TotalNumberOfItemzChangeHistoryByRepositoryUptoDateTimeAsync(
                    getNumberOfChangeHistoryByRepositoryDTO.UptoDateTime);
            _logger.LogDebug("{FormattedControllerAndActionNames}Found {foundItemzChangeHistoryByRepositoryId} ItemzChangeHistory records within the repository upto Date Time {UptoDateTime}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundItemzChangeHistoryByRepository,
                getNumberOfChangeHistoryByRepositoryDTO.UptoDateTime);
            return foundItemzChangeHistoryByRepository;
        }

        /// <summary>
        /// Get list of supported HTTP Options for the ItemzChangeHistoryByRepository controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions(Name = "__OPTIONS_ItemzChangeHistory_By_Repository__")]
        public IActionResult GetItemzOptions()
        {
            Response.Headers.Add("Allow", "GET,HEAD,OPTIONS");
            return Ok();
        }
    }
}
