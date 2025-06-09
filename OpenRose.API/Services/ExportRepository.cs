using ItemzApp.API.Services;
using System;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	public class ExportRepository : IExportRepository
	{
		public Task<string> GenerateExportFileAsync(Guid recordId)
		{
			throw new NotImplementedException();
		}
	}
}
