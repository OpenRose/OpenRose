// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
	public interface IExportRepository
	{
		Task<string> GenerateExportFileAsync(Guid entityId);
	}
}
