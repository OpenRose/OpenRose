// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// SignalR Hub for broadcasting traceability-related notifications to connected clients.
	/// This uses the EXISTING SignalR circuit between Blazor Server and browsers.
	/// 
	/// When a user deletes/updates/creates a traceability via the WebUI,
	/// this hub broadcasts the notification to all other connected users
	/// viewing that same traceability.
	/// </summary>
	public class TraceabilityNotificationHub : Hub
	{
		private readonly ILogger<TraceabilityNotificationHub> _logger;

		public TraceabilityNotificationHub(ILogger<TraceabilityNotificationHub> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Called by browser clients to join a group for a specific traceability.
		/// This way, when a traceability is deleted/updated, we only notify users viewing that traceability.
		/// 
		/// Example: User views traceability 123abc, calls SubscribeToTraceabilityAsync(123abc)
		/// Now this connection is added to group "traceability-123abc"
		/// When traceability 123abc is deleted, notification is sent to "traceability-123abc" group
		/// </summary>
		/// <param name="traceabilityId">The GUID of the traceability to subscribe to notifications for</param>
		public async Task SubscribeToTraceabilityAsync(Guid traceabilityId)
		{
			var groupName = $"traceability-{traceabilityId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) subscribed to traceability {TraceabilityId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				traceabilityId);
		}

		/// <summary>
		/// Called when user leaves the traceability view or navigates away.
		/// Removes the connection from the traceability's notification group.
		/// </summary>
		/// <param name="traceabilityId">The GUID of the traceability to unsubscribe from</param>
		public async Task UnsubscribeFromTraceabilityAsync(Guid traceabilityId)
		{
			var groupName = $"traceability-{traceabilityId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) unsubscribed from traceability {TraceabilityId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				traceabilityId);
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