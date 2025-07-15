using ItemzApp.API.Controllers;
using ItemzApp.API.Models;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	/// <summary>
	/// Interface for the import service.
	/// </summary>
	public interface IImportService
	{
		Task<ImportResult> ImportAsync(RepositoryExportDTO repositoryExportDto,
										string detectedType,
										ImportDataPlacementDTO placementDto);
	}
}
