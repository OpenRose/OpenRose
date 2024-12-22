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
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ItemzApp.API.ResourceParameters;
using ItemzApp.API.Entities;
using Newtonsoft.Json;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    //[Route("api/BaselineItemzType")]
    [Route("api/[controller]")] // e.g. http://HOST:PORT/api/BaselineItemzTypes
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BaselineItemzTypesController : ControllerBase
    {
        private readonly IBaselineItemzTypeRepository _baselineItemzTypeRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ILogger<BaselineItemzTypesController> _logger;

        public BaselineItemzTypesController(IBaselineItemzTypeRepository baselineItemzTypeRepository,
                                    IMapper mapper,
                                    IPropertyMappingService propertyMappingService,
                                     ILogger<BaselineItemzTypesController> logger
                                    )
        {
            _baselineItemzTypeRepository = baselineItemzTypeRepository ?? throw new ArgumentNullException(nameof(baselineItemzTypeRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
               throw new ArgumentNullException(nameof(propertyMappingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a BaselineItemzType by ID (represented by a GUID)
        /// </summary>
        /// <param name="BaselineItemzTypeId">GUID representing an unique ID of the BaselineItemzType that you want to get</param>
        /// <returns>A single BaselineItemzType record based on provided ID (GUID) </returns>
        /// <response code="200">Returns the requested BaselineItemzType</response>
        /// <response code="404">Requested BaselineItemzType not found</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetBaselineItemzTypeDTO))]
        [HttpGet("{BaselineItemzTypeId:Guid}",
            Name = "__Single_BaselineItemzType_By_GUID_ID__")] // e.g. http://HOST:PORT/api/BaselineItemzTypes/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("{BaselineItemzTypeId:Guid}", Name = "__HEAD_BaselineItemzType_By_GUID_ID__")]
        public async Task<ActionResult<GetBaselineItemzTypeDTO>> GetBaselineItemzTypeAsync(Guid BaselineItemzTypeId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get BaselineItemzType for ID {BaselineItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                BaselineItemzTypeId);
            var BaselineItemzTypeFromRepo = await _baselineItemzTypeRepository.GetBaselineItemzTypeAsync(BaselineItemzTypeId);

            if (BaselineItemzTypeFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}BaselineItemzType for ID {BaselineItemzTypeId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    BaselineItemzTypeId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}Found BaselineItemzType for ID {BaselineItemzTypeId} and now returning results",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                BaselineItemzTypeId);
            return Ok(_mapper.Map<GetBaselineItemzTypeDTO>(BaselineItemzTypeFromRepo));
        }

        /// <summary>
        /// Gets collection of BaselineItemzTypes
        /// </summary>
        /// <returns>Collection of BaselineItemzTypes based on expected sorting order.</returns>
        /// <response code="200">Returns collection of BaselineItemzTypes based on sorting order</response>
        /// <response code="404">No BaselineItemzTypes were found</response>
        [HttpGet(Name = "__GET_BaselineItemzTypes_Collection__")]
        [HttpHead(Name = "__HEAD_BaselineItemzTypes_Collection__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<GetBaselineItemzTypeDTO>>> GetBaselineItemzTypesAsync()
        {
            var BaselineItemzTypesFromRepo = await _baselineItemzTypeRepository.GetBaselineItemzTypesAsync();
            if (BaselineItemzTypesFromRepo == null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No BaselineItemzTypes found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                return NotFound();
            }

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {BaselineItemzTypeNumbers} BaselineItemzTypes to the caller",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                BaselineItemzTypesFromRepo.Count());
            return Ok(_mapper.Map<IEnumerable<GetBaselineItemzTypeDTO>>(BaselineItemzTypesFromRepo));
        }



        /// <summary>
        /// Get total number of BaselineItemz by BaselineItemzType
        /// </summary>
        /// <param name="baselineItemzTypeId">Provide BaselineItemzTypeID representated in GUID form</param>
        /// <returns>Number of BaselineItemz found for the given BaselineItemzTypeID. Zero if none found.</returns>
        /// <response code="200">Returns number of BaselineItemz count that were associated with a given BaselineItemzType</response>
        /// <response code="404">BaselineItemzType based on baselineItemzTypeId was not found</response>
        [HttpGet("GetBaselineItemzCount/{BaselineItemzTypeId:Guid}", Name = "__GET_BaselineItemz_Count_By_BaselineItemzType__")]
        [HttpHead("GetBaselineItemzCount/{BaselineItemzTypeId:Guid}", Name = "__HEAD_BaselineItemz_Count_By_BaselineItemzType__")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<int>> GetBaselineItemzCountByBaselineItemzTypeAsync(Guid baselineItemzTypeId)
        {
            if (!(await _baselineItemzTypeRepository.BaselineItemzTypeExistsAsync(baselineItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Cannot find count of BaselineItemz as BaselineItemzType with ID {BaselineItemzTypeId} could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineItemzTypeId);
                return NotFound();
            }

            var foundBaselineItemzCountByBaselineItemzTypeId = await _baselineItemzTypeRepository.GetBaselineItemzCountByBaselineItemzTypeAsync(baselineItemzTypeId);
            _logger.LogDebug("{FormattedControllerAndActionNames} Found {foundBaselineItemzCountByBaselineItemzTypeId} BaselineItemz records for BaselineItemzType with ID {baselineItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                foundBaselineItemzCountByBaselineItemzTypeId,
                baselineItemzTypeId);
            return foundBaselineItemzCountByBaselineItemzTypeId;
        }

        /// <summary>
        /// Gets collection of BaselineItemzs by BaselineItemzType ID
        /// </summary>
        /// <param name="BaselineItemzTypeId">BaselineItemzType ID for which BaselineItemz are queried</param>
        /// <param name="itemzResourceParameter">Pass in information related to Pagination and Sorting Order via this parameter</param>
        /// <returns>Collection of BaselineItemz based on expectated pagination and sorting order.</returns>
        /// <response code="200">Returns collection of BaselineItemzs based on pagination</response>
        /// <response code="404">Either BaselineItemzType or BaselineItemzs were not found</response>
        [HttpGet("GetBaselineItemzByBaselineItemzType/{BaselineItemzTypeId:Guid}", Name = "__GET_BaselineItemzs_By_BaselineItemzType__")]
        [HttpHead("GetBaselineItemzByBaselineItemzType/{BaselineItemzTypeId:Guid}", Name = "__HEAD_BaselineItemzs_By_BaselineItemzType__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<IEnumerable<GetBaselineItemzDTO>>> GetBaselineItemzsByBaselineItemzTypeAsync(Guid BaselineItemzTypeId,
            [FromQuery] ItemzResourceParameter itemzResourceParameter)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<GetBaselineItemzDTO, BaselineItemz>
                (itemzResourceParameter.OrderBy))
            {
                _logger.LogWarning("{FormattedControllerAndActionNames}Requested Order By Field {OrderByFieldName} is not found. Property Validation Failed!",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    itemzResourceParameter.OrderBy);
                return BadRequest();
            }

            if (!(await _baselineItemzTypeRepository.BaselineItemzTypeExistsAsync(BaselineItemzTypeId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}BaselineItemzType with ID {BaselineItemzTypeID} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    BaselineItemzTypeId);
                return NotFound();
            }

            var baselineItemzsFromRepo = _baselineItemzTypeRepository.GetBaselineItemzsByBaselineItemzType(BaselineItemzTypeId, itemzResourceParameter);
            // EXPLANATION : Check if list is IsNullOrEmpty
            // By default we don't have option baked in the .NET to check
            // for null or empty for List type. In the following code we are first checking
            // for nullable BaselineItemzsFromRepo? and then for count great then zero via Any()
            // If any of above is true then we return true. This way we log that no BaselineItemz were
            // found in the database.
            // Ref: https://stackoverflow.com/a/54549818
            if (!baselineItemzsFromRepo?.Any() ?? true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No BaselineItemzs found in BaselineItemzType with ID {BaselineItemzTypeID}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    BaselineItemzTypeId);
                // TODO: If no BaselineItemz are found in a BaselineItemzType then shall we return an error back to the calling client?
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {BaselineItemzNumbers} BaselineItemz found in BaselineItemzType with ID {BaselineItemzTypeId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineItemzsFromRepo?.TotalCount, BaselineItemzTypeId);
            var previousPageLink = baselineItemzsFromRepo!.HasPrevious ?
                CreateBaselineItemzTypeBaselineItemzResourceUri(itemzResourceParameter,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = baselineItemzsFromRepo.HasNext ?
                CreateBaselineItemzTypeBaselineItemzResourceUri(itemzResourceParameter,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = baselineItemzsFromRepo.TotalCount,
                pageSize = baselineItemzsFromRepo.PageSize,
                currentPage = baselineItemzsFromRepo.CurrentPage,
                totalPages = baselineItemzsFromRepo.TotalPages,
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

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {BaselineItemzNumbers} BaselineItemzs to the caller",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineItemzsFromRepo.TotalCount);
            return Ok(_mapper.Map<IEnumerable<GetBaselineItemzDTO>>(baselineItemzsFromRepo));
        }

        private string CreateBaselineItemzTypeBaselineItemzResourceUri(
            ItemzResourceParameter itemzResourceParameter,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("__GET_BaselineItemzs_By_BaselineItemzType__",
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber - 1,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
                case ResourceUriType.NextPage:
                    return Url.Link("__GET_BaselineItemzs_By_BaselineItemzType__", 
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber + 1,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
                default:
                    return Url.Link("__GET_BaselineItemzs_By_BaselineItemzType__",
                        new
                        {
                            orderBy = itemzResourceParameter.OrderBy,
                            pageNumber = itemzResourceParameter.PageNumber,
                            pageSize = itemzResourceParameter.PageSize
                        })!;
            }
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
        /// Get list of supported HTTP Options for the BaselineItemzTypes controller.
        /// </summary>
        /// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
        /// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

        [HttpOptions(Name = "__OPTIONS_for_BaselineItemzTypes_Controller__")]
        public IActionResult GetBaselineItemzTypesOptions()
        {
            Response.Headers.Add("Allow","GET,HEAD,OPTIONS");
            return Ok();
        }
    }
}
