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
	/// Service to handle project creation with real-time notification broadcasting.
	/// This service ensures that when a project is created via the WebUI,
	/// all other connected users are immediately notified so they can see the new project.
	/// </summary>
	public class ProjectCreationService
	{
		private readonly IProjectService _projectService;
		private readonly IProjectNotificationService _notificationService;
		private readonly ILogger<ProjectCreationService> _logger;

		public ProjectCreationService(
			IProjectService projectService,
			IProjectNotificationService notificationService,
			ILogger<ProjectCreationService> logger)
		{
			_projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Create a new project and notify all connected users that a new project exists.
		/// 
		/// Process:
		/// 1. Call API to create the project
		/// 2. If successful, broadcast creation notification via SignalR to all users
		/// 3. All connected users will receive notification of the new project
		/// </summary>
		/// <param name="createProjectDTO">The project details to create</param>
		/// <returns>The created project DTO if successful, null otherwise</returns>
		public async Task<GetProjectDTO?> CreateProjectWithNotificationAsync(GetProjectDTO createProjectDTO)
		{
			try
			{
				_logger.LogInformation(
					"Attempting to create new project: {ProjectName}",
					createProjectDTO?.Name ?? "Unknown");

				// Step 1: Call the API to create the project
				var createdProject = await _projectService.__POST_Create_Project__Async(createProjectDTO);

				if (createdProject != null)
				{
					_logger.LogInformation(
						"Successfully created project {ProjectId} ({ProjectName})",
						createdProject.Id,
						createdProject.Name);

					// Step 2: BROADCAST NOTIFICATION - Only pass ProjectId
					// All users will receive notification and can refresh their project lists
					await _notificationService.NotifyProjectCreatedAsync(createdProject.Id);

					return createdProject;
				}
				else
				{
					_logger.LogWarning(
						"Failed to create project - API returned null");
					return null;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Exception occurred while creating project");
				return null;
			}
		}
	}
}