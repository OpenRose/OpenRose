// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.Services.Project;
using OpenRose.WebUI.Client.SharedModels;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// Service to handle project updates with real-time notification broadcasting.
	/// This service ensures that when a project is updated via the WebUI,
	/// all other connected users viewing that project are immediately notified of the changes.
	/// </summary>
	public class ProjectUpdateService
	{
		private readonly IProjectService _projectService;
		private readonly IProjectNotificationService _notificationService;
		private readonly ILogger<ProjectUpdateService> _logger;

		public ProjectUpdateService(
			IProjectService projectService,
			IProjectNotificationService notificationService,
			ILogger<ProjectUpdateService> logger)
		{
			_projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Update a project and notify all connected users viewing that project.
		/// 
		/// Process:
		/// 1. Call API to update the project
		/// 2. If successful, broadcast update notification via SignalR to users viewing this project
		/// 3. All other users viewing this project will receive the notification
		/// </summary>
		/// <param name="projectId">The GUID of the project to update</param>
		/// <param name="updateProjectDTO">The updated project details</param>
		/// <returns>The updated project DTO if successful, null otherwise</returns>
		public async Task<GetProjectDTO?> UpdateProjectWithNotificationAsync(Guid projectId, GetProjectDTO updateProjectDTO)
		{
			try
			{
				_logger.LogInformation(
					"Attempting to update project {ProjectId}",
					projectId);

				// Step 1: Call the API to update the project
				var updatedProject = await _projectService.__PUT_Update_Project_By_GUID_ID__Async(projectId, updateProjectDTO);

				if (updatedProject != null)
				{
					_logger.LogInformation(
						"Successfully updated project {ProjectId}",
						projectId);

					// Step 2: BROADCAST NOTIFICATION - Only pass ProjectId
					// Users viewing this project will receive the update notification
					await _notificationService.NotifyProjectUpdatedAsync(projectId);

					return updatedProject;
				}
				else
				{
					_logger.LogWarning(
						"Failed to update project {ProjectId} - API returned null",
						projectId);
					return null;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Exception occurred while updating project {ProjectId}",
					projectId);
				return null;
			}
		}
	}
}