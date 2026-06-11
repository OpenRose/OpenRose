// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// SignalR Hub for broadcasting estimation-related notifications to connected clients.
	/// This uses the EXISTING SignalR circuit between Blazor Server and browsers.
	/// 
	/// When a user deletes/updates/creates a estimation via the WebUI,
	/// this hub broadcasts the notification to all other connected users
	/// viewing that same estimation.
	/// </summary>
	public class EstimationNotificationHub : Hub
	{
		private readonly ILogger<EstimationNotificationHub> _logger;

		public EstimationNotificationHub(ILogger<EstimationNotificationHub> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Called by browser clients to join a group for a specific estimation.
		/// This way, when a estimation is deleted/updated, we only notify users viewing that estimation.
		/// 
		/// Example: User views estimation 123abc, calls SubscribeToEstimationAsync(123abc)
		/// Now this connection is added to group "estimation-123abc"
		/// When estimation 123abc is deleted, notification is sent to "estimation-123abc" group
		/// </summary>
		/// <param name="estimationId">The GUID of the estimation to subscribe to notifications for</param>
		public async Task SubscribeToEstimationAsync(Guid estimationId)
		{
			var groupName = $"estimation-{estimationId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) subscribed to estimation {EstimationId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				estimationId);
		}

		/// <summary>
		/// Called when user leaves the estimation view or navigates away.
		/// Removes the connection from the estimation's notification group.
		/// </summary>
		/// <param name="estimationId">The GUID of the estimation to unsubscribe from</param>
		public async Task UnsubscribeFromEstimationAsync(Guid estimationId)
		{
			var groupName = $"estimation-{estimationId}";
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

			_logger.LogInformation(
				"User {UserId} (Connection: {ConnectionId}) unsubscribed from estimation {EstimationId}",
				Context.User?.Identity?.Name ?? "Anonymous",
				Context.ConnectionId,
				estimationId);
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