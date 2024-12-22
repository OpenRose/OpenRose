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
    [Route("api/BaselineItemzTrace")] // e.g. http://HOST:PORT/api/ItemzTypeItemzs
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BaselineItemzTraceController : ControllerBase
    {
        private readonly IBaselineItemzTraceRepository _baselineItemzTraceRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly ILogger<BaselineItemzTraceController> _logger;

        public BaselineItemzTraceController(IBaselineItemzTraceRepository baselineItemzTraceRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            ILogger<BaselineItemzTraceController> logger)
        {
            _baselineItemzTraceRepository = baselineItemzTraceRepository ?? throw new ArgumentNullException(nameof(baselineItemzTraceRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        /// <summary>
        /// Check if specific Baseline Itemz Trace association exists
        /// </summary>
        /// <param name="fromTraceBaselineItemzId">Provide From Trace Baseline Itemz Id</param>
        /// <param name="toTraceBaselineItemzId">Provide To Trace Baseline Itemz Id</param>
        /// <returns>BaselineItemzTraceDTO for the Baseline Itemz that has specified Baseline Itemz Trace</returns>
        /// <response code="200">Returns BaselineItemzTraceDTO for the From and To Baseline Itemz Trace</response>
        /// <response code="404">Baseline Itemz Trace was not found</response>
        [HttpGet("CheckExists/", Name = "__GET_Check_Baseline_Itemz_Trace_Exists__")]
        [HttpHead("CheckExists/", Name = "__HEAD_Check_Baseline_Itemz_Trace_Exists__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<BaselineItemzTraceDTO>> CheckBaselineItemzTraceExistsAsync([FromQuery] Guid fromTraceBaselineItemzId, Guid toTraceBaselineItemzId) // TODO: Try from Query.
        {
            var tempBaselineItemzTraceDTO = new BaselineItemzTraceDTO();

            tempBaselineItemzTraceDTO.FromTraceBaselineItemzId = fromTraceBaselineItemzId;
            tempBaselineItemzTraceDTO.ToTraceBaselineItemzId = toTraceBaselineItemzId;
            if (!(await _baselineItemzTraceRepository.BaselineItemzsTraceExistsAsync(tempBaselineItemzTraceDTO)))  // Check if BaselineItemzTrace association exists or not
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}From BaselineItemz ID {fromTraceBaselineItemzId} and To BaselineItemz ID {toTraceBaselineItemzId} Trace could not be found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    tempBaselineItemzTraceDTO.FromTraceBaselineItemzId,
                    tempBaselineItemzTraceDTO.ToTraceBaselineItemzId);
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}From Baseline Itemz ID {fromTraceBaselineItemzId} and To Baseline Itemz ID {toTraceBaselineItemzId} Trace was found",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    tempBaselineItemzTraceDTO.FromTraceBaselineItemzId,
                    tempBaselineItemzTraceDTO.ToTraceBaselineItemzId);
            return Ok(tempBaselineItemzTraceDTO);
        }

        /// <summary>
        /// Gets collection of Baseline Itemz Traces by Baseline Itemz ID
        /// </summary>
        /// <param name="baselineItemzId">Baseline Itemz ID for which Baseline Itemz Traces are queried</param>
        /// <returns>Collection of Baseline Itemz Traces by Baseline Itemz ID</returns>
        /// <response code="200">Returns Collection of Baseline Itemz Traces by Baseline Itemz ID</response>
        /// <response code="404">Either Baseline ItemzID was not found or No Baseline Itemz Traces were found for given BaselineItemzID</response>
        [HttpGet("{baselineItemzId:Guid}", Name = "__GET_Baseline_Itemz_Traces_By_BaselineItemzID__")]
        [HttpHead("{baselineItemzId:Guid}", Name = "__HEAD_Baseline_Itemz_Traces_By_BaselineItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<BaselineItemzTraceDTO>>> GetBaselineItemzTracesByBaselineItemzIDAsync(Guid baselineItemzId)
        {
            if (!(await _baselineItemzTraceRepository.BaselineItemzExistsAsync(baselineItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline Itemz with ID {baselineItemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineItemzId);
                return NotFound();
            }

            var baselineItemzTracesFromRepo = await  _baselineItemzTraceRepository.GetAllTracesByBaselineItemzIdAsync(baselineItemzId);

            // EXPLANATION : Check if list is IsNullOrEmpty
            // By default we don't have option baked in the .NET to check
            // for null or empty for List type. In the following code we are first checking
            // for nullable itemzsFromRepo? and then for count great then zero via Any()
            // If any of above is true then we return true. This way we log that no itemz traces were
            // found in the database.
            // Ref: https://stackoverflow.com/a/54549818
            if (!baselineItemzTracesFromRepo?.Any() ?? true)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}No Baseline Itemz Traces found for Baseline Itemz with ID {BaselineItemzId}",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineItemzId);
                // TODO: If no Baseline Itemz Traces are found for an ItemzID then shall we return an error back to the calling client?
                return NotFound();
            }
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {BaselineItemzTraceNumbers} Baseline Itemz Traces found in Baseline Itemz with ID {BaselineItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineItemzTracesFromRepo?.Count(), baselineItemzId);

            _logger.LogDebug("{FormattedControllerAndActionNames}Returning results for {BaselineItemzNumbers} Baseline Itemzs to the caller",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineItemzTracesFromRepo?.Count());
            return Ok(_mapper.Map<IEnumerable<ItemzTraceDTO>>(baselineItemzTracesFromRepo));
        }

        /// <summary>
        /// Gets All Parent and Child Baseline Itemz Traces by Baseline Itemz ID
        /// </summary>
        /// <param name="baselineItemzId">Baseline Itemz ID for which Parent and Child Baseline Itemz Traces are returned.</param>
        /// <returns>Collection of all Parent and Child Baseline Itemz Traces by Baseline Itemz ID</returns>
        /// <response code="200">Returns Collection of all Parent and Child Baseline Itemz Traces by Baseline Itemz ID</response>
        /// <response code="404">BaselineItemzID was not found in the repository</response>
        [HttpGet("AllBaselineItemzTraces/{baselineItemzId:Guid}", Name = "__GET_All_Parent_and_Child_Baseline_Itemz_Traces_By_BaselineItemzID__")]
        [HttpHead("AllBaselineItemzTraces/{baselineItemzId:Guid}", Name = "__HEAD_All_Parent_and_Child_Baseline_Itemz_Traces_By_BaselineItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<BaselineItemzParentAndChildTraceDTO>> GetAllParentAndChildTracesByBaselineItemzIdAsync(Guid baselineItemzId)
        {
            if (!(await _baselineItemzTraceRepository.BaselineItemzExistsAsync(baselineItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline Itemz with ID {baselineItemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineItemzId);
                return NotFound();
            }

            var baselineItemzParentAndChildTraceDTO = await _baselineItemzTraceRepository.GetAllParentAndChildTracesByBaselineItemzIdAsync(baselineItemzId);

            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ParentBaselineItemzTraceCount} Parent Baseline Itemz Traces found for Baseline Itemz with ID {BaselineItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineItemzParentAndChildTraceDTO.BaselineItemz?.ParentBaselineItemz?.Count, baselineItemzId);

            _logger.LogDebug("{FormattedControllerAndActionNames}In total {ChildBaselineItemzTraceCount} Child Baseline Itemz Traces found for Baseline Itemz with ID {BaselineItemzId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                baselineItemzParentAndChildTraceDTO.BaselineItemz?.ChildBaselineItemz?.Count, baselineItemzId);

            return Ok(baselineItemzParentAndChildTraceDTO);
        }

        /// <summary>
        /// Get count of FromBaselineItemz Traces associated with BaselineItemzID
        /// </summary>
        /// <param name="baselineItemzId">Provide BaselineItemzId in GUID form</param>
        /// <returns>Integer representing total number of direct From Baseline Itemz Traces associated with BaselineItemzID</returns>
        /// <response code="200">Count of From Baseline Itemz Traces associated with BaselineItemzID. ZERO means no From Baseline Itemz Traces were found for targeted BaselineItemzID</response>
        /// <response code="404">Baseline Itemz for given ID could not be found</response>
        [HttpGet("GetFromBaselineItemzTraceCount/{BaselineItemzId:Guid}", Name = "__GET_From_BaselineItemz_Trace_Count_By_BaselineItemzID__")]
        [HttpHead("GetFromBaselineItemzTraceCount/{BaselineItemzId:Guid}", Name = "__HEAD_From_BaselineItemz_Trace_Count_By_BaselineItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<int>> GetFromBaselineItemzTraceCountByBaselineItemzID(Guid baselineItemzId)
        {
            if (!(await _baselineItemzTraceRepository.BaselineItemzExistsAsync(baselineItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline Itemz with ID {baselineItemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineItemzId);
                return NotFound();
            }
            int countOfFromBaselineItemzTraces = await _baselineItemzTraceRepository.GetFromTraceCountByBaselineItemz(baselineItemzId);
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {countOfFromBaselineItemzTraces} From Baseline Itemz Traces were found associated with BaselineItemzID {BaselineItemzID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                countOfFromBaselineItemzTraces,
                baselineItemzId);
            return Ok(countOfFromBaselineItemzTraces);
        }

        /// <summary>
        /// Get count of ToBaselineItemz Traces associated with BaselineItemzID
        /// </summary>
        /// <param name="baselineItemzId">Provide BaselineItemzId in GUID form</param>
        /// <returns>Integer representing total number of direct To Baseline Itemz Traces associated with BaselineItemzID</returns>
        /// <response code="200">Count of To Baseline Itemz Traces associated with BaselineItemzID. ZERO means no To Baseline Itemz Traces were found for targeted BaselineItemzID</response>
        /// <response code="404">Baseline Itemz for given ID could not be found</response>
        [HttpGet("GetToBaselineItemzTraceCount/{BaselineItemzId:Guid}", Name = "__GET_To_BaselineItemz_Trace_Count_By_BaselineItemzID__")]
        [HttpHead("GetToBaselineItemzTraceCount/{BaselineItemzId:Guid}", Name = "__HEAD_To_BaselineItemz__Trace_Count_By_BaselineItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<int>> GetToBaselineItemzTraceCountByBaselineItemzID(Guid baselineItemzId)
        {
            if (!(await _baselineItemzTraceRepository.BaselineItemzExistsAsync(baselineItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline Itemz with ID {baselineItemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineItemzId);
                return NotFound();
            }
            int countOfToBaselineItemzTraces = await _baselineItemzTraceRepository.GetToTraceCountByBaselineItemz(baselineItemzId);
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {countOfToBaselineItemzTracess} To Baseline Itemz Traces were found associated with BaselineItemzID {baselineItemzID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                countOfToBaselineItemzTraces,
                baselineItemzId);
            return Ok(countOfToBaselineItemzTraces);
        }

        /// <summary>
        /// Get count of From and To Traces associated with BaselineItemzID
        /// </summary>
        /// <param name="baselineItemzId">Provide BaselineItemzId in GUID form</param>
        /// <returns>Integer representing total number of direct From and To Baseline Itemz Traces associated with BaselineItemzID</returns>
        /// <response code="200">Count of From and To Baseline Itemz Traces associated with BaselineItemzID. ZERO means no Direct Baseline Itemz Traces were found for targeted BaselineItemzID</response>
        /// <response code="404">Baseline Itemz for given ID could not be found</response>
        [HttpGet("GetAllFromAndToTracesCountByBaselineItemzId/{baselineItemzId:Guid}", Name = "__GET_All_From_and_To_Traces_Count_By_BaselineItemzID__")]
        [HttpHead("GetAllFromAndToTracesCountByItemzId/{baselineItemzId:Guid}", Name = "__HEAD_All_From_and_To_Traces_Count_By_BaselineItemzID__")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]

        public async Task<ActionResult<int>> GetAllFromAndToTracesCountByBaselineItemzId(Guid baselineItemzId)
        {
            if (!(await _baselineItemzTraceRepository.BaselineItemzExistsAsync(baselineItemzId)))
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Baseline Itemz with ID {baselineItemzId} was not found in the repository",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    baselineItemzId);
                return NotFound();
            }
            int countOfAllFromAndToBaselineTraces = await _baselineItemzTraceRepository.GetAllFromAndToTracesCountByBaselineItemzIdAsync(baselineItemzId);
            _logger.LogDebug("{FormattedControllerAndActionNames}In total {countOfAllFromAndToBaselineTraces} From and To Baseline Itemz Traces were found associated with BaselineItemzID {baselineItemzID}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                countOfAllFromAndToBaselineTraces,
                baselineItemzId);
            return Ok(countOfAllFromAndToBaselineTraces);
        }
    }
}
