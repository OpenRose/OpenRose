// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRose.API.Services
{
	/// <summary>
	/// Interface for Keeping SQL Connection Alive between API and SQL Server
	/// </summary>
	public interface IKeepSQLConnectionAliveService
	{
		/// <summary>
		/// Ping SQL Server form API at a fixed interval for keeping connection alive
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		Task SendKeepAliveAsync(CancellationToken stoppingToken);
	}
}