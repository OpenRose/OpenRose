using ItemzApp.API.Controllers;
using ItemzApp.API.Models;
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

	}
}
