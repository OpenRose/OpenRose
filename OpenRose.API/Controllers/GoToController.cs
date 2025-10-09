// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Models;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GoToController : ControllerBase
	{
		private readonly IHierarchyRepository _hierarchyRepository;
		private readonly IBaselineHierarchyRepository _baselineHierarchyRepository;
		private readonly ILogger<GoToController> _logger;

		public GoToController(
			IHierarchyRepository hierarchyRepository,
			IBaselineHierarchyRepository baselineHierarchyRepository,
			ILogger<GoToController> logger)
		{
			_hierarchyRepository = hierarchyRepository ?? throw new ArgumentNullException(nameof(hierarchyRepository));
			_baselineHierarchyRepository = baselineHierarchyRepository ?? throw new ArgumentNullException(nameof(baselineHierarchyRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Universal GoTo endpoint for resolving record navigation context for supported record types.
		/// </summary>
		/// <param name="recordId">GUID representing a unique ID of any supported record type</param>
		/// <returns>GoToResolutionDTO containing navigation and hierarchy context for the record</returns>
		/// <response code="200">GoToResolutionDTO with navigation details for the specified record ID</response>
		/// <response code="404">Record not found for the specified GUID</response>
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GoToResolutionDTO))]
		[HttpGet("{recordId:Guid}", Name = "__Get_GoTo_Details_By_GUID__")] // e.g. http://HOST:PORT/api/GoTo/42f62a6c-fcda-4dac-a06c-406ac1c17770
		[HttpHead("{recordId:Guid}", Name = "__HEAD_GoTo_Details_By_GUID__")]
		public async Task<ActionResult<GoToResolutionDTO>> GetGoToAsync(Guid recordId)
		{
			// Try HierarchyRepository (Projects/ItemzTypes/Itemz)
			HierarchyIdRecordDetailsDTO? hierarchyRecord = null;
			try
			{
				hierarchyRecord = await _hierarchyRepository.GetHierarchyRecordDetailsByID(recordId);
			}
			catch (ApplicationException ex)
			{
				// Log and ignore; try Baseline repository next
				_logger.LogDebug("Record with ID {recordId} Not found in HierarchyRepository: {Message}", recordId.ToString(), ex.Message);
			}
			if (hierarchyRecord != null && !string.IsNullOrWhiteSpace(hierarchyRecord.RecordType))
			{
				// Get Project context
				Guid? projectId = null;
				string? projectName = null;
				string? projectHierarchyId = null;
				int? projectHierarchyLevel = null;

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
					ProjectHierarchyLevel = projectHierarchyLevel ?? 0
				};

				return Ok(dto);
			}

			// Try BaselineHierarchyRepository (Baseline, BaselineItemzType, BaselineItemz)

			BaselineHierarchyIdRecordDetailsDTO? baselineHierarchyRecord = null;
			try
			{
				baselineHierarchyRecord = await _baselineHierarchyRepository.GetBaselineHierarchyRecordDetailsByID(recordId);
			}
			catch (ApplicationException ex)
			{
				_logger.LogDebug("Record with ID {recordId} Not found in BaselineHierarchyRepository: {Message}", recordId.ToString(), ex.Message);
			}  
			if (baselineHierarchyRecord != null && !string.IsNullOrWhiteSpace(baselineHierarchyRecord.RecordType))
			{
				// Get Project context via parent chain
				Guid? projectId = null;
				string? projectName = null;
				string? projectHierarchyId = null;
				int? projectHierarchyLevel = null;
				Guid? baselineId = null;

				var parentRecords = await _baselineHierarchyRepository.GetAllParentsOfBaselineItemzHierarchy(recordId);
				var projectParent = parentRecords.AllRecords.FirstOrDefault(p =>
					string.Equals(p.RecordType, "Project", StringComparison.OrdinalIgnoreCase)
				);
				if (projectParent != null)
				{
					projectId = projectParent.RecordId;
					projectName = projectParent.Name;
					projectHierarchyId = projectParent.BaselineHierarchyId;
					projectHierarchyLevel = projectParent.Level;
				}


				var baselineParent = FindFirstBaselineRecord(parentRecords.AllRecords);


				if (baselineParent != null)
				{
					baselineId = baselineParent.RecordId;
				}

				var dto = new GoToResolutionDTO
				{
					RecordId = baselineHierarchyRecord.RecordId,
					RecordType = baselineHierarchyRecord.RecordType,
					Name = baselineHierarchyRecord.Name,
					RecordHierarchyId = baselineHierarchyRecord.BaselineHierarchyId,
					RecordHierarchyLevel = baselineHierarchyRecord.Level,
					ProjectId = projectId,
					ProjectName = projectName,
					ProjectHierarchyId = projectHierarchyId,
					ProjectHierarchyLevel = projectHierarchyLevel ?? 0,
					BaselineId = baselineId
				};

				return Ok(dto);
			}

			// Not found in either repository
			return NotFound(new { error = "Record not found." });
		}

		private NestedBaselineHierarchyIdRecordDetailsDTO? FindFirstBaselineRecord(IEnumerable<NestedBaselineHierarchyIdRecordDetailsDTO>? records)
		{
			if (records == null) return null;

			foreach (var record in records)
			{
				if (string.Equals(record.RecordType, "Baseline", StringComparison.OrdinalIgnoreCase))
				{
					return record;
				}
				// Search children recursively
				var resultInChildren = FindFirstBaselineRecord(record.Children);
				if (resultInChildren != null)
				{
					return resultInChildren;
				}
			}
			return null;
		}
	}
}