// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0.
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.



using ItemzApp.API.Controllers;
using ItemzApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	public interface IImportService
	{
		/// <summary>
		/// Imports a hierarchy starting at an Itemz record.
		/// </summary>
		Task<ImportResult> ImportAsync(
			RepositoryExportDTO repositoryExportDto,
			string detectedType,
			ImportDataPlacementDTO placementDto);

		/// <summary>
		/// Imports a hierarchy starting at an ItemzType record.
		/// </summary>
		Task<ImportResult> ImportItemzTypeHierarchyAsync(
			RepositoryExportDTO repositoryExportDto,
			ImportDataPlacementDTO placementDto);

		/// <summary>
		/// Imports a hierarchy starting at a Project record.
		/// </summary>
		Task<ImportResult> ImportProjectHierarchyAsync(
			RepositoryExportDTO repositoryExportDto,
			ImportDataPlacementDTO placementDto);

		/// <summary>
		/// Imports a hierarchy of BaselineItemz records.
		/// </summary>
		Task<ImportResult> ImportBaselineItemzAsync(
			RepositoryExportDTO repositoryExportDto,
			ImportDataPlacementDTO placementDto);

		/// <summary>
		/// Imports a hierarchy of BaselineItemzType records, potentially containing BaselineItemz breakdown.
		/// </summary>
		Task<ImportResult> ImportBaselineItemzTypeAsync(
			RepositoryExportDTO repositoryExportDto,
			ImportDataPlacementDTO placementDto);

		/// <summary>
		/// Imports a hierarchy of Baseline record, potentially containing BaselineItemzType and BaselineItemz breakdown.
		/// </summary>
		Task<ImportResult> ImportBaselineAsProjectAsync(
			RepositoryExportDTO repositoryExportDto,
			ImportDataPlacementDTO placementDto);
	}
}
