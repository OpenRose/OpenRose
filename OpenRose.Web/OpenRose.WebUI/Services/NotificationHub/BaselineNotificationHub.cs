// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// SignalR Hub for broadcasting baseline-related notifications to connected clients.
	/// This uses the EXISTING SignalR circuit between Blazor Server and browsers.
	/// 
	/// When a user deletes/updates/creates a baseline via the WebUI,
	/// this hub broadcasts the notification to all other connected users
	/// viewing that same baseline.
	/// </summary>
	public class BaselineNotificationHub : Hub
	{
		private readonly ILogger<BaselineNotificationHub> _logger;

		public BaselineNotificationHub(ILogger<BaselineNotificationHub> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Called by browser clients to join a group for a specific baseline.
		/// This way, when a baseline is deleted/updated, we only notify users viewing that baseline.
		/// 
		/// Example: User views baseline 123abc, calls SubscribeToBaselineAsync(123abc)
		/// Now this connection is added to group "baseline-123abc"
		/// When baseline 123abc is deleted, notification is sent to "baseline-123abc" group
		/// </summary>
		/// <param name="baselineId">The GUID of the baseline to subscribe to notifications for</param>
		public async Task SubscribeToBaselineAsync(Guid baselineId)
		{
			var groupName = $"baseline-{baselineId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) subscribed to baseline {BaselineId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				baselineId);
		}

		/// <summary>
		/// Called when user leaves the baseline view or navigates away.
		/// Removes the connection from the baseline's notification group.
		/// </summary>
		/// <param name="baselineId">The GUID of the baseline to unsubscribe from</param>
		public async Task UnsubscribeFromBaselineAsync(Guid baselineId)
		{
			var groupName = $"baseline-{baselineId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) unsubscribed from baseline {BaselineId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				baselineId);
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