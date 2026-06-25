// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// Interface for broadcasting project notifications to connected clients via SignalR.
	/// 
	/// Only passes ProjectId - clients have all needed details already loaded.
	/// </summary>
	public interface IProjectNotificationService
	{
		/// <summary>
		/// Notify all connected users viewing a specific project that it has been deleted.
		/// </summary>
		/// <param name="projectId">The GUID of the deleted project</param>
		Task NotifyProjectDeletedAsync(Guid projectId);

		/// <summary>
		/// Notify all connected users that a new project has been created.
		/// </summary>
		/// <param name="projectId">The GUID of the newly created project</param>
		Task NotifyProjectCreatedAsync(Guid projectId);

		/// <summary>
		/// Notify all connected users viewing a specific project that it has been updated.
		/// </summary>
		/// <param name="projectId">The GUID of the updated project</param>
		Task NotifyProjectUpdatedAsync(Guid projectId);
	}
}