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
using ItemzApp.API.Models.BetweenControllerAndRepository;

namespace ItemzApp.API.Controllers
{
    [ApiController]
    //[Route("api/Hierarchy")]
    [Route("api/[controller]")] // e.g. http://HOST:PORT/api/Hierarchy
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class HierarchyController : ControllerBase
    {
        private readonly IHierarchyRepository _hierarchyRepository;
        private readonly ILogger<HierarchyController> _logger;

        public HierarchyController(IHierarchyRepository hierarchyRepository,
                                    ILogger<HierarchyController> logger)
        {
            _hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }




		/// <summary>
		/// Gets Hierarchy Record details of the next Sibling of Record Id provided in GUID form.
		/// </summary>
		/// <param name="RecordId">GUID representing an unique ID of a hierarchy record</param>
		/// <returns>Hierarchy record details containing various information about given Record Id's next Sibling</returns>
		/// <response code="200">Hierarchy record details containing various information about given Record Id's next Sibling</response>
		/// <response code="404">Either Sibling OR Hierarchy record not found in the repository for the given GUID ID</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HierarchyIdRecordDetailsDTO))]
		[HttpGet("GetNextSibling/{RecordId:Guid}",
			Name = "__Get_Next_Sibling_Hierarchy_Record_Details_By_GUID__")] // e.g. http://HOST:PORT/api/Hierarchy/42f62a6c-fcda-4dac-a06c-406ac1c17770
		[HttpHead("GetNextSibling/{RecordId:Guid}", Name = "__HEAD_Next_Sibling_Hierarchy_Record_Details_By_GUID__")]
		public async Task<ActionResult<HierarchyIdRecordDetailsDTO>> GetNextSiblingHierarchyRecordDetailsAsync(Guid RecordId)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get Next Sibling Hierarchy record details for ID {1stRecordId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				RecordId);

			var nextSiblingRecordDetailsDTO = new HierarchyIdRecordDetailsDTO();
			try
			{
				nextSiblingRecordDetailsDTO = await _hierarchyRepository.GetNextSiblingHierarchyRecordDetailsByID(RecordId);
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get Next Sibling Hierarchy Details : " + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				var tempMessage = $"Could not produce next sibling hierarchy details for given Record Id {RecordId}" +
					$" :: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}

			if (nextSiblingRecordDetailsDTO != null)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Returning Next Sibling Hierarchy Record details for ID {ParentRecordId} " +
					"with '{HierarchyId}' as HierarchyID and {RecordType} as Record Type.",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					RecordId,
					nextSiblingRecordDetailsDTO.HierarchyId,
					nextSiblingRecordDetailsDTO.RecordType);
			}
			return Ok(nextSiblingRecordDetailsDTO);
		}






		/// <summary>
		/// Gets Hierarchy Record details based on Record Id provided in GUID form.
		/// </summary>
		/// <param name="RecordId">GUID representing an unique ID of a hierarchy record</param>
		/// <returns>Hierarchy record details containing various information about given Record Id</returns>
		/// <response code="200">Hierarchy record details containing various information about given Record Id</response>
		/// <response code="404">Hierarchy record not found in the repository for the given GUID ID</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HierarchyIdRecordDetailsDTO))]
        [HttpGet("{RecordId:Guid}",
            Name = "__Get_Hierarchy_Record_Details_By_GUID__")] // e.g. http://HOST:PORT/api/Hierarchy/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("{RecordId:Guid}", Name = "__HEAD_Hierarchy_Record_Details_By_GUID__")]
        public async Task<ActionResult<HierarchyIdRecordDetailsDTO>> GetHierarchyRecordDetailsAsync(Guid RecordId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get Hierarchy record details for ID {ParentRecordId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                RecordId);

            var hierarchyIdRecordDetailsDTO = new HierarchyIdRecordDetailsDTO();
            try 
            { 
                hierarchyIdRecordDetailsDTO = await _hierarchyRepository.GetHierarchyRecordDetailsByID(RecordId);
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get Hierarchy Details : " + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not produce hierarchy details for given Record Id {RecordId}" +
                    $" :: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }

            if (hierarchyIdRecordDetailsDTO != null)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Returning Hierarchy Record details for ID {ParentRecordId} " +
                    "with '{HierarchyId}' as HierarchyID and {RecordType} as Record Type.",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    RecordId,
                    hierarchyIdRecordDetailsDTO.HierarchyId,
                    hierarchyIdRecordDetailsDTO.RecordType);
            }
            return Ok(hierarchyIdRecordDetailsDTO);
        }

		/// <summary>
		/// Gets Hierarchy Records of immediate children under Record Id provided in GUID form.
		/// </summary>
		/// <param name="RecordId">GUID representing an unique ID of a hierarchy record</param>
		/// <returns>Collection of Immediate children Hierarchy record details </returns>
		/// <response code="200">Immediate children Hierarchy record details </response>
		/// <response code="400">Bad Request</response>
		/// <response code="404">Immediate children Hierarchy record(s) not found in the repository for the given GUID ID</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HierarchyIdRecordDetailsDTO))]
		[HttpGet("GetImmediateChildren/{RecordId:Guid}"
            , Name = "__Get_Immediate_Children_Hierarchy_By_GUID__")] // e.g. http://HOST:PORT/api/Hierarchy/GetImmediateChildren/42f62a6c-fcda-4dac-a06c-406ac1c17770
		[HttpHead("GetImmediateChildren/{RecordId:Guid}", Name = "__HEAD_Immediate_Children_Hierarchy_By_GUID__")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<IEnumerable<HierarchyIdRecordDetailsDTO>>> GetImmediateChildrenOfItemzHierarchy(Guid RecordId)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get Immediate Children Hierarchy records for ID {ParentRecordId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				RecordId);

			IEnumerable<HierarchyIdRecordDetailsDTO?> immediateChildrenhierarchyRecords = [];
			try
			{
				immediateChildrenhierarchyRecords = await _hierarchyRepository.GetImmediateChildrenOfItemzHierarchy(RecordId);
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get Immediate Children Hierarchy records : " + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				var tempMessage = $"Could not produce get immediate children hierarchy records for given Record Id {RecordId}" +
					$" :: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}

			if (!(immediateChildrenhierarchyRecords.IsNullOrEmpty()))
			{
				_logger.LogDebug("{FormattedControllerAndActionNames} Returning {hirarchyChildRecordCount} Immediate Children Hierarchy Records for ID {RecordId} ",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					immediateChildrenhierarchyRecords.Count(),
					RecordId );
			}
            else
            {
				_logger.LogDebug("{FormattedControllerAndActionNames} Returning 0 (ZERO) Immediate Children Hierarchy Records for ID {RecordId} ",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					RecordId);
				return Ok();
            }
			return Ok(immediateChildrenhierarchyRecords);

		}

		/// <summary>
		/// Gets the root Repository Hierarchy record (Level 0).
		/// </summary>
		/// <returns>Repository Hierarchy record details</returns>
		/// <response code="200">Repository Hierarchy record details</response>
		/// <response code="404">Repository hierarchy record not found</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NestedHierarchyIdRecordDetailsDTO))]
		[HttpGet("GetRepositoryHierarchyRecord", Name = "__Get_Repository_Hierarchy_Record__")] // e.g. http://HOST:PORT/api/Hierarchy/GetRepositoryHierarchyRecord
		[HttpHead("GetRepositoryHierarchyRecord", Name = "__HEAD_Repository_Hierarchy_Record__")]
		public async Task<ActionResult<NestedHierarchyIdRecordDetailsDTO>> GetRepositoryHierarchyRecord()
		{
			_logger.LogDebug("{FormattedControllerAndActionNames} Processing request to get Repository (root) Hierarchy record.",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));

			NestedHierarchyIdRecordDetailsDTO? repositoryRecordDto;
			try
			{
				repositoryRecordDto = await _hierarchyRepository.GetRepositoryHierarchyRecord();

				if (repositoryRecordDto == null)
				{
					_logger.LogDebug("{FormattedControllerAndActionNames} No Repository (root) Hierarchy record found.",
						ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
					return NotFound();
				}
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames} Exception occurred while trying to get Repository Hierarchy record: " + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));
				var tempMessage = $"Could not produce Repository Hierarchy record :: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}

			_logger.LogDebug("{FormattedControllerAndActionNames} Returning Repository (root) Hierarchy record.",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext));

			return Ok(repositoryRecordDto);
		}


		/// <summary>
		/// Gets Hierarchy Records of all parents above Record Id provided in GUID form.
		/// </summary>
		/// <param name="RecordId">GUID representing an unique ID of a hierarchy record</param>
		/// <returns>Collection of All Parents Hierarchy record details </returns>
		/// <response code="200">All Parents Hierarchy record details </response>
		/// <response code="400">Bad Request</response>
		/// <response code="404">All Parents Hierarchy record(s) not found in the repository for the given GUID ID</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NestedHierarchyIdRecordDetailsDTO))]
		[HttpGet("GetAllParents/{RecordId:Guid}"
			, Name = "__Get_All_Parents_Hierarchy_By_GUID__")] // e.g. http://HOST:PORT/api/Hierarchy/GetAllParents/42f62a6c-fcda-4dac-a06c-406ac1c17770
		[HttpHead("GetAllParents/{RecordId:Guid}", Name = "__HEAD_All_Parents_Hierarchy_By_GUID__")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<IEnumerable<NestedHierarchyIdRecordDetailsDTO>>> GetAllParentsOfItemzHierarchy(Guid RecordId)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get All Parents Hierarchy records for ID {ParentRecordId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				RecordId);
			RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>();

			IEnumerable<NestedHierarchyIdRecordDetailsDTO?> allParentshierarchyRecords = [];
			try
			{
				recordCountAndEnumerable = await _hierarchyRepository.GetAllParentsOfItemzHierarchy(RecordId);
				if (recordCountAndEnumerable.AllRecords.Any())
				{
					allParentshierarchyRecords = recordCountAndEnumerable.AllRecords;
				}
				else
				{
					_logger.LogDebug("{FormattedControllerAndActionNames} Returning {RecordCount} (ZERO) All Parents Hierarchy Records for ID {RecordId} ",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					recordCountAndEnumerable.RecordCount,
					RecordId);
					return Ok();
				}
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get All Parents Hierarchy records : " + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				var tempMessage = $"Could not produce All Parents hierarchy records for given Record Id {RecordId}" +
					$" :: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}

			_logger.LogDebug("{FormattedControllerAndActionNames} Returning {CountOfAllParentHierarchyRecords} All Parents Hierarchy Records for ID {RecordId} ",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				recordCountAndEnumerable.RecordCount,
				RecordId);

			return Ok(allParentshierarchyRecords);
		}


		public static int CountRecordsInNestedList(List<NestedHierarchyIdRecordDetailsDTO> itemzList)
		{
			return itemzList.Sum(item => 1 + CountRecordsInNestedList(item.Children ?? new List<NestedHierarchyIdRecordDetailsDTO>()));
		}


		/// <summary>
		/// Gets Hierarchy Records of all children under Record Id provided in GUID form.
		/// </summary>
		/// <param name="RecordId">GUID representing an unique ID of a hierarchy record</param>
		/// <returns>Collection of All children Hierarchy record details </returns>
		/// <response code="200">All children Hierarchy record details </response>
		/// <response code="400">Bad Request</response>
		/// <response code="404">All children Hierarchy record(s) not found in the repository for the given GUID ID</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NestedHierarchyIdRecordDetailsDTO))]
		[HttpGet("GetAllChildren/{RecordId:Guid}"
			, Name = "__Get_All_Children_Hierarchy_By_GUID__")] // e.g. http://HOST:PORT/api/Hierarchy/GetAllChildren/42f62a6c-fcda-4dac-a06c-406ac1c17770
		[HttpHead("GetAllChildren/{RecordId:Guid}", Name = "__HEAD_All_Children_Hierarchy_By_GUID__")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<IEnumerable<NestedHierarchyIdRecordDetailsDTO>>> GetAllChildrenOfItemzHierarchy(Guid RecordId)
		{
			_logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get All Children Hierarchy records for ID {RecordId}",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				RecordId);

			RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO> recordCountAndEnumerable = new RecordCountAndEnumerable<NestedHierarchyIdRecordDetailsDTO>();

			IEnumerable<NestedHierarchyIdRecordDetailsDTO?> allChildrenhierarchyRecords = [];
			try
			{
				recordCountAndEnumerable = await _hierarchyRepository.GetAllChildrenOfItemzHierarchy(RecordId);
				if (recordCountAndEnumerable.AllRecords.Any())
				{
					allChildrenhierarchyRecords = recordCountAndEnumerable.AllRecords;	
				}
				else
				{
					_logger.LogDebug("{FormattedControllerAndActionNames} Returning {RecordCount} (ZERO) All Children Hierarchy Records for ID {RecordId} ",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
					recordCountAndEnumerable.RecordCount,
					RecordId);
					return Ok();
				}
			}
			catch (ApplicationException appException)
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get All Children Hierarchy records : " + appException.Message,
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				var tempMessage = $"Could not produce All Children hierarchy records for given Record Id {RecordId}" +
					$" :: InnerException :: {appException.Message} ";
				return BadRequest(tempMessage);
			}

			_logger.LogDebug("{FormattedControllerAndActionNames} Returning {hirarchyChildRecordCount} All Children Hierarchy Records for ID {RecordId} ",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				recordCountAndEnumerable.RecordCount,
				RecordId);


			return Ok(allChildrenhierarchyRecords);

		}

        /// <summary>
        /// Gets count of all hierarchy children under Record Id provided in GUID form.
        /// </summary>
        /// <param name="RecordId">GUID representing an unique ID of a hierarchy record</param>
        /// <returns>Count of All children Hierarchy record </returns>
        /// <response code="200">All children Hierarchy record count </response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Record ID not found in the repository for the given GUID ID</response>
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [HttpGet("GetAllChildrenCount/{RecordId:Guid}"
            , Name = "__Get_All_Children_Hierarchy_Count_By_GUID__")] // e.g. http://HOST:PORT/api/Hierarchy/GetAllChildrenCount/42f62a6c-fcda-4dac-a06c-406ac1c17770
        [HttpHead("GetAllChildrenCount/{RecordId:Guid}", Name = "__HEAD_All_Children_Hierarchy_Count_By_GUID__")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> GetAllChildrenCountOfItemzHierarchy(Guid RecordId)
        {
            _logger.LogDebug("{FormattedControllerAndActionNames}Processing request to get All Children Hierarchy records count for ID {RecordId}",
                ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                RecordId);

            try
            {
                var allChildHierarchyRecordCount = await _hierarchyRepository.GetAllChildrenCountOfItemzHierarchy(RecordId);
                if (allChildHierarchyRecordCount == 0)
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames} Returning {allChildHierarchyRecordCount} ZERO All Children Hierarchy Records for ID {RecordId} ",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    allChildHierarchyRecordCount,
                    RecordId);
                    return Ok(allChildHierarchyRecordCount);
                }
                else
                {
                    _logger.LogDebug("{FormattedControllerAndActionNames} Returning {allChildHierarchyRecordCount} All Children Hierarchy Records for ID {RecordId} ",
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
                    allChildHierarchyRecordCount,
                    RecordId);
                    return Ok(allChildHierarchyRecordCount);
                }
            }
            catch (ApplicationException appException)
            {
                _logger.LogDebug("{FormattedControllerAndActionNames}Exception occured while trying to get All Children Hierarchy records count : " + appException.Message,
                    ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
                    );
                var tempMessage = $"Could not produce All Children hierarchy records count for given Record Id {RecordId}" +
                    $" :: InnerException :: {appException.Message} ";
                return BadRequest(tempMessage);
            }
        }


		[HttpGet("GoTo/{recordId:Guid}")]
		public async Task<ActionResult<GoToResolutionDTO>> GetGoToAsync(Guid recordId)
		{
			// Step 1: Get the hierarchy record for the GUID
			var hierarchyRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByID(recordId);
			if (hierarchyRecord == null)
				return NotFound(new { error = "Record not found." });

			// Step 2: If record is not a Project, get the project's details via parent chain
			Guid? projectId = null;
			string? projectName = null;
			string? projectHierarchyId = null;
			int? projectHierarchyLevel = 0;

			if (!string.Equals(hierarchyRecord.RecordType, "Project", StringComparison.OrdinalIgnoreCase))
			{
				var parentRecords = await _hierarchyRepository.GetAllParentsOfItemzHierarchy(recordId);
				var projectParent = parentRecords.AllRecords.FirstOrDefault(p =>
					string.Equals(p.RecordType, "Project", StringComparison.OrdinalIgnoreCase)
				);
				if (projectParent != null)
				{
					projectId = projectParent.RecordId;
					projectName = projectParent.Name;
					projectHierarchyId = projectParent.HierarchyId;
					projectHierarchyLevel = projectParent.Level;
				}
			}
			else
			{
				projectId = hierarchyRecord.RecordId;
				projectName = hierarchyRecord.Name;
				projectHierarchyId = hierarchyRecord.HierarchyId;
				projectHierarchyLevel = hierarchyRecord.Level;
			}

			// Step 3: Build DTO
			var dto = new GoToResolutionDTO
			{
				RecordId = hierarchyRecord.RecordId,
				RecordType = hierarchyRecord.RecordType,
				Name = hierarchyRecord.Name,
				RecordHierarchyId = hierarchyRecord.HierarchyId,
				RecordHierarchyLevel = hierarchyRecord.Level,
				ProjectId = projectId,
				ProjectName = projectName,
				ProjectHierarchyId = projectHierarchyId,
				ProjectHierarchyLevel = projectHierarchyLevel
			};

			return Ok(dto);
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
