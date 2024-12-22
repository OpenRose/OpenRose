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

using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ItemzApp.API.Models.BetweenControllerAndRepository;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    //[Route("api/Hierarchy")]
    [Route("api/[controller]")] // e.g. http://HOST:PORT/api/Hierarchy
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BaselineHierarchyController : ControllerBase
    {
        private readonly IBaselineHierarchyRepository _baselineHierarchyRepository;
        private readonly ILogger<BaselineHierarchyController> _logger;

        public BaselineHierarchyController(IBaselineHierarchyRepository baselineHierarchyRepository,
                                    ILogger<BaselineHierarchyController> logger)
        {
            _baselineHierarchyRepository = baselineHierarchyRepository ?? throw new ArgumentNullException(nameof(baselineHierarchyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets BaselineHierarchy Record details based on Record Id provided in GUID form.
        /// </summary>
        /// <param name="RecordId">GUID representing an unique ID of a BaselineHierarchy record</param>
        /// <returns>BaselineHierarchy record details containing various information about given Record Id</returns>
        /// <response code="200">BaselineHierarchy record details containing various information about given Record Id</response>
        /// <response code="404">BaselineHierarchy record not found in the repository for the given GUID ID</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaselineHierarchyIdRecordDetailsDTO))]
        [HttpGet("{RecordId:Guid}",
            Name = "__Get_BaselineHierarchy_Record_Details_By_GUID__")] // e.g. http://HOST:PORT/api/BaselineHierarchy/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("{RecordId:Guid}", Name = "__HEAD_BaselineHierarchy_Record_Details_By_GUID__")]
        public async Task<ActionResult<BaselineHierarchyIdRecordDetailsDTO>> GetBaselineHierarchyRecordDetailsAsync(Guid RecordId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get BaselineHierarchy record details for ID {ParentRecordId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                RecordId);

            var baselineHierarchyIdRecordDetailsDTO = new BaselineHierarchyIdRecordDetailsDTO();
            try 
            { 
                baselineHierarchyIdRecordDetailsDTO = await _baselineHierarchyRepository.GetBaselineHierarchyRecordDetailsByID(RecordId);
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get BaselineHierarchy Details : " + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not produce BaselineHierarchy details for given Record Id {RecordId}" +
                    $" :: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }

            if (baselineHierarchyIdRecordDetailsDTO != null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Returning BaselineHierarchy Record details for ID {ParentRecordId} " +
                    "with '{BaselineHierarchyId}' as BaselineHierarchyID and {RecordType} as Record Type.",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    RecordId,
                    baselineHierarchyIdRecordDetailsDTO.BaselineHierarchyId,
                    baselineHierarchyIdRecordDetailsDTO.RecordType);
            }
            return Ok(baselineHierarchyIdRecordDetailsDTO);
        }

		/// <summary>
		/// Gets Baseline Hierarchy Records of immediate children under Record Id provided in GUID form.
		/// </summary>
		/// <param name="RecordId">GUID representing an unique ID of a baseline hierarchy record</param>
		/// <returns>Collection of Immediate children Baseline Hierarchy record details </returns>
		/// <response code="200">Immediate children Baseline Hierarchy record details </response>
		/// <response code="400">Bad Request</response>
		/// <response code="404">Immediate children Baseline Hierarchy record(s) not found in the repository for the given GUID ID</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaselineHierarchyIdRecordDetailsDTO))]
		[HttpGet("GetImmediateChildren/{RecordId:Guid}"
			, Name = "__Get_Immediate_Children_Baseline_Hierarchy_By_GUID__")] // e.g. http://HOST:PORT/api/BaselineHierarchy/GetImmediateChildren/42f62a6c-fcda-4dac-a06c-406ac1c17770
		[HttpHead("GetImmediateChildren/{RecordId:Guid}", Name = "__HEAD_Immediate_Children_Baseline_Hierarchy_By_GUID__")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<IEnumerable<BaselineHierarchyIdRecordDetailsDTO>>> GetImmediateChildrenOfBaselineItemzHierarchy(Guid RecordId)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get Immediate Children Baseline Hierarchy records for ID {ParentRecordId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				RecordId);

			// IEnumerable<BaselineHierarchyIdRecordDetailsDTO?> immediateChildrenBaselineHierarchyRecords = [];
			IEnumerable<BaselineHierarchyIdRecordDetailsDTO?> immediateChildrenBaselineHierarchyRecords = new List<BaselineHierarchyIdRecordDetailsDTO?>();

			try
			{

				var tempImmediateChildrenBaselineHierarchyRecords = await _baselineHierarchyRepository.GetImmediateChildrenOfBaselineItemzHierarchy(RecordId);
                if (tempImmediateChildrenBaselineHierarchyRecords != null)
                {
                    immediateChildrenBaselineHierarchyRecords = tempImmediateChildrenBaselineHierarchyRecords;
                }
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get Immediate Children Baseline Hierarchy records : " + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				var tempMessage = $"Could not produce get immediate children baseline hierarchy records for given Record Id {RecordId}" +
					$" :: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}

			if (!(immediateChildrenBaselineHierarchyRecords.IsNullOrEmpty()))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames} Returning {baselineHirarchyChildRecordCount} Immediate Children Baseline Hierarchy Records for ID {RecordId} ",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					immediateChildrenBaselineHierarchyRecords.Count(),
					RecordId);
			}
			else
			{
				_logger.LogDebug("{FormattedControllerAndActionNames} Returning 0 (ZERO) Immediate Children Baseline Hierarchy Records for ID {RecordId} ",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					RecordId);
			}

            return Ok(immediateChildrenBaselineHierarchyRecords);

		}

		/// <summary>
		/// Verify that two Baseline Hierarchy IDs are part of same Breakdown Structure within a given Baseline.
		/// </summary>
		/// <param name="parentId">GUID representing an unique ID of a Parent BaselineHierarchy record</param>
		/// <param name="childId">GUID representing an unique ID of a Child BaselineHierarchy record</param>
		/// <returns>True if ParentHierarchyId and ChildHierarchyId are part of the same Breakdown Structure within a given Baseline. Otherwise False </returns>
		/// <response code="200">True or False based on outcome of verifying Breakdown Structure while looking for Parent and Child Baseline Hierarchy IDs</response>
		/// <response code="404">Invalid Id provided for either Parent or Child Baseline Hierarchy record</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [HttpGet("VerifyParentChildBreakdownStructure/",
            Name = "__Get_VerifyParentChild_BreakdownStructure__")] // e.g. http://HOST:PORT/api/BaselineHierarchy/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("VerifyParentChildBreakdownStructure/", Name = "__HEAD_VerifyParentChild_BreakdownStructure__")]
        public async Task<ActionResult<bool>> VerifyParentChildBreakdownStructureAsync([FromQuery] Guid parentId, [FromQuery] Guid childId)
        {

            if (parentId == Guid.Empty)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames} Parent Baseline Hierarchy Breakdown Structure is an empty ID.",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
                return NotFound();
            }

            if (childId == Guid.Empty)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames} Child Baseline Hierarchy Breakdown Structure is an empty ID.",
                        ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
                return NotFound();
            }

            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to verify parent and child Baseline Hieararchy " +
                "Breakdown Structure between Parent ID {ParentId} and Child ID {ChildId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                , parentId
                , childId);

            var parentBaselineHierarchyIdRecordDetailsDTO = new BaselineHierarchyIdRecordDetailsDTO();
            try
            {
                parentBaselineHierarchyIdRecordDetailsDTO = await _baselineHierarchyRepository.GetBaselineHierarchyRecordDetailsByID(parentId);
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get Parent BaselineHierarchy Details : " + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not produce Parent BaselineHierarchy details for given Id {parentId}" +
                    $" :: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }

            var childBaselineHierarchyIdRecordDetailsDTO = new BaselineHierarchyIdRecordDetailsDTO();
            try
            {
                childBaselineHierarchyIdRecordDetailsDTO = await _baselineHierarchyRepository.GetBaselineHierarchyRecordDetailsByID(childId);
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get child BaselineHierarchy Details : " + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not produce child BaselineHierarchy details for given Id {childId}" +
                    $" :: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }

            if (parentBaselineHierarchyIdRecordDetailsDTO is null)
            {
                return BadRequest($"Baseline Hierarchy Id details could not be found for provided Parent ID {parentId}");
            }

            if (childBaselineHierarchyIdRecordDetailsDTO is null)
            { 
                return BadRequest($"Baseline Hierarchy Id details could not be found for provided Child ID {childId}");
            }

            if (parentBaselineHierarchyIdRecordDetailsDTO.Level < 2 )
            { 
                return BadRequest("Provided Parent Id is for record above Baseline Level.");
            }

            if (childBaselineHierarchyIdRecordDetailsDTO.Level < 2)
            {
                return BadRequest("Provided Child Id is for record above Baseline Level.");
            }

            return Ok(await
                _baselineHierarchyRepository.CheckIfPartOfSingleBaselineHierarchyBreakdownStructureAsync(parentId, childId)
            );

        }


        /// <summary>
        /// Gets Baseline Hierarchy Records of all children under Record Id provided in GUID form.
        /// </summary>
        /// <param name="RecordId">GUID representing an unique ID of a Baseline Hierarchy record</param>
        /// <returns>Collection of All children Baseline Hierarchy record details </returns>
        /// <response code="200">All children Baseline Hierarchy record details </response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">All children Baseline Hierarchy record(s) not found in the repository for the given GUID ID</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NestedBaselineHierarchyIdRecordDetailsDTO))]
        [HttpGet("GetAllChildren/{RecordId:Guid}"
            , Name = "__Get_All_Children_Baseline_Hierarchy_By_GUID__")] // e.g. http://HOST:PORT/api/BaselineHierarchy/GetAllChildren/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("GetAllChildren/{RecordId:Guid}", Name = "__HEAD_All_Children_Baseline_Hierarchy_By_GUID__")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>>> GetAllChildrenOfBaselineItemzHierarchy(Guid RecordId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get All Children Baseline Hierarchy records for ID {ParentRecordId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                RecordId);

			RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>();

			IEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO?> allChildrenBaselineHierarchyRecords = [];
            try
            {
				recordCountAndEnumerable = await _baselineHierarchyRepository.GetAllChildrenOfBaselineItemzHierarchy(RecordId);
				// allChildrenBaselineHierarchyRecords = await _baselineHierarchyRepository.GetAllChildrenOfBaselineItemzHierarchy(RecordId);

                if (recordCountAndEnumerable.AllRecords.Any())
                {
                    allChildrenBaselineHierarchyRecords = recordCountAndEnumerable.AllRecords;
				}
                else
                {
					_logger.LogDebug("{FormattedControllerAndActionNames} Returning {RecordCount} (ZERO) All Children Baseline Hierarchy Records for ID {RecordId} ",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
						recordCountAndEnumerable.RecordCount,
						RecordId);
    					return Ok();
				}
				

			}
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get All Children Baseline Hierarchy records : " + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not produce All Children Baseline Hierarchy records for given Record Id {RecordId}" +
                    $" :: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }

			_logger.LogDebug("{FormattedControllerAndActionNames} Returning {baselineHirarchyChildRecordCount} All Children Baseline Hierarchy Records for ID {RecordId} ",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				recordCountAndEnumerable.RecordCount,
				RecordId);

            return Ok(allChildrenBaselineHierarchyRecords);

        }

        /// <summary>
        /// Gets count of all baseline hierarchy children under Record Id provided in GUID form.
        /// </summary>
        /// <param name="RecordId">GUID representing an unique ID of a baseline hierarchy record</param>
        /// <returns>Count of All children Baseline Hierarchy record </returns>
        /// <response code="200">All children Baseline Hierarchy record count </response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Record ID not found in the repository for the given GUID ID</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [HttpGet("GetAllChildrenCount/{RecordId:Guid}"
            , Name = "__Get_All_Children_Baseline_Hierarchy_Count_By_GUID__")] // e.g. http://HOST:PORT/api/BaselineHierarchy/GetAllChildrenCount/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("GetAllChildrenCount/{RecordId:Guid}", Name = "__HEAD_All_Children_Baseline_Hierarchy_Count_By_GUID__")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> GetAllChildrenCountOfBaselineItemzHierarchy(Guid RecordId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get All Children Baseline Hierarchy records count for ID {RecordId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                RecordId);

            try
            {
                var allChildBaselineHierarchyRecordCount = await _baselineHierarchyRepository.GetAllChildrenCountOfBaselineItemzHierarchy(RecordId);
                if (allChildBaselineHierarchyRecordCount == 0)
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames} Returning {allChildHierarchyRecordCount} ZERO All Children Baseline Hierarchy Records for ID {RecordId} ",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    allChildBaselineHierarchyRecordCount,
                    RecordId);
                    return Ok(allChildBaselineHierarchyRecordCount);
                }
                else
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames} Returning {allChildHierarchyRecordCount} All Children Baseline Hierarchy Records for ID {RecordId} ",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    allChildBaselineHierarchyRecordCount,
                    RecordId);
                    return Ok(allChildBaselineHierarchyRecordCount);
                }
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get All Children Baseline Hierarchy records count : " + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not produce All Children Baseline Hierarchy records count for given Record Id {RecordId}" +
                    $" :: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }
        }

        /// <summary>
        /// Gets Baseline Hierarchy Records of all parents above Record Id provided in GUID form.
        /// </summary>
        /// <param name="RecordId">GUID representing an unique ID of a Baseline Hierarchy record</param>
        /// <returns>Collection of All Parents  Baseline Hierarchy record details </returns>
        /// <response code="200">All Parents Baseline Hierarchy record details </response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">All Parents Baseline Hierarchy record(s) not found in the repository for the given GUID ID</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NestedBaselineHierarchyIdRecordDetailsDTO))]
		[HttpGet("GetAllParents/{RecordId:Guid}"
			, Name = "__Get_All_Parents_Baseline_Hierarchy_By_GUID__")] // e.g. http://HOST:PORT/api/BaselineHierarchy/GetAllParents/42f62a6c-fcda-4dac-a06c-406ac1c17770
		[HttpHead("GetAllParents/{RecordId:Guid}", Name = "__HEAD_All_Parents_Baseline_Hierarchy_By_GUID__")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<IEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>>> GetAllParentsOfBaselineItemzHierarchy(Guid RecordId)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get All Parents Baseline Hierarchy records for ID {ParentRecordId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				RecordId);

			RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>();

			IEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO?> allParentsBaselineHierarchyRecords = [];
			try
			{

				// allParentsBaselineHierarchyRecords = await _baselineHierarchyRepository.GetAllParentsOfBaselineItemzHierarchy(RecordId);
				recordCountAndEnumerable = await _baselineHierarchyRepository.GetAllParentsOfBaselineItemzHierarchy(RecordId);


				if (recordCountAndEnumerable.AllRecords.Any())
				{
					allParentsBaselineHierarchyRecords = recordCountAndEnumerable.AllRecords;
				}
				else
				{
					_logger.LogDebug("{FormattedControllerAndActionNames} Returning {RecordCount} (ZERO) All Parents Baseline Hierarchy Records for ID {RecordId} ",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					recordCountAndEnumerable.RecordCount,
					RecordId);
					return Ok();
				}

			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get All Parents Baseline Hierarchy records : " + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				var tempMessage = $"Could not produce All Parents Baseline Hierarchy records for given Record Id {RecordId}" +
					$" :: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}

			_logger.LogDebug("{FormattedControllerAndActionNames} Returning {CountOfAllParentHierarchyRecords} All Parents Baseline Hierarchy Records for ID {RecordId} ",
	            ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				recordCountAndEnumerable.RecordCount,
				RecordId);

			return Ok(allParentsBaselineHierarchyRecords);

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
    }
}
