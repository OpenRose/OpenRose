// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.Project;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// Service to handle project deletion with real-time notification broadcasting.
	/// This service ensures that when a project is deleted via the WebUI,
	/// all other connected users viewing that project are immediately notified.
	/// </summary>
	public class ProjectDeletionService
	{
		private readonly IProjectService _projectService;
		private readonly IProjectNotificationService _notificationService;
		private readonly ILogger<ProjectDeletionService> _logger;

		public ProjectDeletionService(
			IProjectService projectService,
			IProjectNotificationService notificationService,
			ILogger<ProjectDeletionService> logger)
		{
			_projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Delete a project via the API and notify all connected users viewing that project.
		/// 
		/// Process:
		/// 1. Call API to delete the project
		/// 2. If successful, broadcast deletion notification via SignalR
		/// 3. All other users viewing this project will receive the notification
		/// </summary>
		/// <param name="projectId">The GUID of the project to delete</param>
		/// <returns>True if deletion was successful, false otherwise</returns>
		public async Task<bool> DeleteProjectWithNotificationAsync(Guid projectId)
		{
			try
			{
				_logger.LogInformation(
					"Attempting to delete project {ProjectId}",
					projectId);

				// Step 1: Call the API to delete the project
				await _projectService.__DELETE_Project_By_GUID_ID__Async(projectId);

				_logger.LogInformation(
					"Successfully deleted project {ProjectId}",
					projectId);

				// Step 2: BROADCAST NOTIFICATION - Only pass ProjectId
				// Clients have all details already loaded and will use that information
				await _notificationService.NotifyProjectDeletedAsync(projectId);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Exception occurred while deleting project {ProjectId}",
					projectId);
				return false;
			}
		}
	}
}