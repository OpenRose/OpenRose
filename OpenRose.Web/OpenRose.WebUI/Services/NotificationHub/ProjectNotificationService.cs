// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.SignalR;
using OpenRose.WebUI.Constants;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// Service to send project notifications to connected clients via SignalR.
	/// Only passes ProjectId - clients have all needed details already loaded.
	/// 
	/// This service is responsible for broadcasting notifications when:
	/// - A project is deleted
	/// - A project is created
	/// - A project is updated
	/// </summary>
	public class ProjectNotificationService : IProjectNotificationService
	{
		private readonly IHubContext<ProjectNotificationHub> _hubContext;
		private readonly ILogger<ProjectNotificationService> _logger;

		public ProjectNotificationService(
			IHubContext<ProjectNotificationHub> hubContext,
			ILogger<ProjectNotificationService> logger)
		{
			_hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Broadcast project deletion notification to all users viewing that specific project.
		/// </summary>
		/// <param name="projectId">The GUID of the deleted project</param>
		public async Task NotifyProjectDeletedAsync(Guid projectId)
		{
			try
			{
				var groupName = SignalRConstants.GroupNamePatterns.GetProjectGroup(projectId);

				// Send only the ProjectId - clients have all details already
				await _hubContext.Clients.Group(groupName)
					.SendAsync(SignalRConstants.ProjectEvents.ProjectDeleted, projectId);

				_logger.LogInformation(
					"Broadcasted {EventName} notification for project {ProjectId}",
					SignalRConstants.ProjectEvents.ProjectDeleted,
					projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Error broadcasting {EventName} notification for project {ProjectId}",
					SignalRConstants.ProjectEvents.ProjectDeleted,
					projectId);
				throw;
			}
		}

		/// <summary>
		/// Broadcast project creation notification to all connected clients.
		/// Since all users may need to see new projects in their list,
		/// we send to ALL clients rather than a specific group.
		/// </summary>
		/// <param name="projectId">The GUID of the newly created project</param>
		public async Task NotifyProjectCreatedAsync(Guid projectId)
		{
			try
			{
				// Send to all connected clients (new project visible in all project lists)
				await _hubContext.Clients.All
					.SendAsync(SignalRConstants.ProjectEvents.ProjectCreated, projectId);

				_logger.LogInformation(
					"Broadcasted {EventName} notification for project {ProjectId}",
					SignalRConstants.ProjectEvents.ProjectCreated,
					projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Error broadcasting {EventName} notification for project {ProjectId}",
					SignalRConstants.ProjectEvents.ProjectCreated,
					projectId);
				throw;
			}
		}

		/// <summary>
		/// Broadcast project update notification to all users viewing that specific project.
		/// </summary>
		/// <param name="projectId">The GUID of the updated project</param>
		public async Task NotifyProjectUpdatedAsync(Guid projectId)
		{
			try
			{
				var groupName = SignalRConstants.GroupNamePatterns.GetProjectGroup(projectId);

				// Send only the ProjectId - clients have all details already
				await _hubContext.Clients.Group(groupName)
					.SendAsync(SignalRConstants.ProjectEvents.ProjectModified, projectId);

				_logger.LogInformation(
					"Broadcasted {EventName} notification for project {ProjectId}",
					SignalRConstants.ProjectEvents.ProjectModified,
					projectId);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Error broadcasting {EventName} notification for project {ProjectId}",
					SignalRConstants.ProjectEvents.ProjectModified,
					projectId);
				throw;
			}
		}
	}
}