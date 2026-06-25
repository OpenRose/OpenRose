// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// SignalR Hub for broadcasting project-related notifications to connected clients.
	/// This uses the EXISTING SignalR circuit between Blazor Server and browsers.
	/// 
	/// When a user deletes/updates/creates a project via the WebUI,
	/// this hub broadcasts the notification to all other connected users
	/// viewing that same project.
	/// </summary>
	public class ProjectNotificationHub : Hub
	{
		private readonly ILogger<ProjectNotificationHub> _logger;

		public ProjectNotificationHub(ILogger<ProjectNotificationHub> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Called by browser clients to join a group for a specific project.
		/// This way, when a project is deleted/updated, we only notify users viewing that project.
		/// 
		/// Example: User views project 123abc, calls SubscribeToProjectAsync(123abc)
		/// Now this connection is added to group "project-123abc"
		/// When project 123abc is deleted, notification is sent to "project-123abc" group
		/// </summary>
		/// <param name="projectId">The GUID of the project to subscribe to notifications for</param>
		public async Task SubscribeToProjectAsync(Guid projectId)
		{
			var groupName = $"project-{projectId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) subscribed to project {ProjectId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				projectId);
		}

		/// <summary>
		/// Called when user leaves the project view or navigates away.
		/// Removes the connection from the project's notification group.
		/// </summary>
		/// <param name="projectId">The GUID of the project to unsubscribe from</param>
		public async Task UnsubscribeFromProjectAsync(Guid projectId)
		{
			var groupName = $"project-{projectId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) unsubscribed from project {ProjectId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				projectId);
		}

		public override async Task OnConnectedAsync()
		{
			_logger.LogInformation(
				"Client connected: {ConnectionId}, User: {UserId}",
				Context.ConnectionId,
				Context.User?.Identity?.Name ?? "Anonymous");

			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			_logger.LogInformation(
				"Client disconnected: {ConnectionId}, User: {UserId}, Reason: {Reason}",
				Context.ConnectionId,
				Context.User?.Identity?.Name ?? "Anonymous",
				exception?.Message ?? "Normal disconnection");

			await base.OnDisconnectedAsync(exception);
		}
	}
}