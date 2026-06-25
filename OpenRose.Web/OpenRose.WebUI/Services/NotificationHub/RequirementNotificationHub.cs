// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// SignalR Hub for broadcasting requirement-related notifications to connected clients.
	/// This uses the EXISTING SignalR circuit between Blazor Server and browsers.
	/// 
	/// When a user deletes/updates/creates a requirement via the WebUI,
	/// this hub broadcasts the notification to all other connected users
	/// viewing that same requirement.
	/// </summary>
	public class RequirementNotificationHub : Hub
	{
		private readonly ILogger<RequirementNotificationHub> _logger;

		public RequirementNotificationHub(ILogger<RequirementNotificationHub> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Called by browser clients to join a group for a specific requirement.
		/// This way, when a requirement is deleted/updated, we only notify users viewing that requirement.
		/// 
		/// Example: User views requirement 123abc, calls SubscribeToRequirementAsync(123abc)
		/// Now this connection is added to group "requirement-123abc"
		/// When requirement 123abc is deleted, notification is sent to "requirement-123abc" group
		/// </summary>
		/// <param name="requirementId">The GUID of the requirement to subscribe to notifications for</param>
		public async Task SubscribeToRequirementAsync(Guid requirementId)
		{
			var groupName = $"requirement-{requirementId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) subscribed to requirement {RequirementId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				requirementId);
		}

		/// <summary>
		/// Called when user leaves the requirement view or navigates away.
		/// Removes the connection from the requirement's notification group.
		/// </summary>
		/// <param name="requirementId">The GUID of the requirement to unsubscribe from</param>
		public async Task UnsubscribeFromRequirementAsync(Guid requirementId)
		{
			var groupName = $"requirement-{requirementId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) unsubscribed from requirement {RequirementId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				requirementId);
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