// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Services.NotificationHub
{
	/// <summary>
	/// Monitoring hub for SignalR health and connection tracking
	/// Provides real-time metrics about SignalR connections and events
	/// </summary>
	public class SignalRMonitoringHub : Hub
	{
		private readonly ILogger<SignalRMonitoringHub> _logger;
		private static readonly ConcurrentDictionary<string, ConnectionInfo> ActiveConnections =
			new ConcurrentDictionary<string, ConnectionInfo>();

		public SignalRMonitoringHub(ILogger<SignalRMonitoringHub> logger)
		{
			_logger = logger;
		}

		public override async Task OnConnectedAsync()
		{
			var connectionInfo = new ConnectionInfo
			{
				ConnectionId = Context.ConnectionId,
				UserId = Context.User?.Identity?.Name ?? "Anonymous",
				ConnectedAt = DateTime.UtcNow,
				IpAddress = Context.GetHttpContext()?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown"
			};

			ActiveConnections.TryAdd(Context.ConnectionId, connectionInfo);

			_logger.LogInformation(
				"Client connected to monitoring hub. ConnectionId: {ConnectionId}, User: {UserId}, IP: {IpAddress}",
				Context.ConnectionId,
				connectionInfo.UserId,
				connectionInfo.IpAddress);

			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			ActiveConnections.TryRemove(Context.ConnectionId, out _);

			_logger.LogInformation(
				"Client disconnected from monitoring hub. ConnectionId: {ConnectionId}, Reason: {Reason}",
				Context.ConnectionId,
				exception?.Message ?? "Normal disconnection");

			await base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Get current SignalR health metrics
		/// </summary>
		public SignalRHealthMetrics GetHealthMetrics()
		{
			return new SignalRHealthMetrics
			{
				TotalConnections = ActiveConnections.Count,
				ConnectedUsers = ActiveConnections.Values.Select(c => c.UserId).Distinct().Count(),
				HubInstances = new[]
				{
					"ProjectNotificationHub",
					"RequirementNotificationHub",
					"BaselineNotificationHub",
					"EstimationNotificationHub",
					"TraceabilityNotificationHub"
				},
				LastCheckAt = DateTime.UtcNow,
				IsHealthy = ActiveConnections.Count < 10000 // Warning threshold
			};
		}

		/// <summary>
		/// Get list of all connected clients
		/// </summary>
		public IEnumerable<ConnectionInfo> GetConnectedClients()
		{
			return ActiveConnections.Values
				.OrderByDescending(c => c.ConnectedAt)
				.ToList();
		}

		/// <summary>
		/// Get connections for a specific user
		/// </summary>
		public IEnumerable<ConnectionInfo> GetConnectionsByUser(string userId)
		{
			return ActiveConnections.Values
				.Where(c => c.UserId == userId)
				.ToList();
		}

		/// <summary>
		/// Get connection duration statistics
		/// </summary>
		public ConnectionDurationStats GetConnectionDurationStats()
		{
			var now = DateTime.UtcNow;
			var durations = ActiveConnections.Values
				.Select(c => (now - c.ConnectedAt).TotalSeconds)
				.ToList();

			return new ConnectionDurationStats
			{
				AverageConnectionDurationSeconds = durations.Any() ? durations.Average() : 0,
				MinConnectionDurationSeconds = durations.Any() ? durations.Min() : 0,
				MaxConnectionDurationSeconds = durations.Any() ? durations.Max() : 0,
				TotalConnections = durations.Count
			};
		}
	}

	/// <summary>
	/// SignalR Health Metrics
	/// </summary>
	public class SignalRHealthMetrics
	{
		public int TotalConnections { get; set; }
		public int ConnectedUsers { get; set; }
		public string[] HubInstances { get; set; } = Array.Empty<string>();
		public DateTime LastCheckAt { get; set; }
		public bool IsHealthy { get; set; }
	}

	/// <summary>
	/// Individual Connection Information
	/// </summary>
	public class ConnectionInfo
	{
		public string ConnectionId { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public DateTime ConnectedAt { get; set; }
		public string IpAddress { get; set; } = string.Empty;

		public double ConnectionDurationSeconds =>
			(DateTime.UtcNow - ConnectedAt).TotalSeconds;
	}

	/// <summary>
	/// Connection Duration Statistics
	/// </summary>
	public class ConnectionDurationStats
	{
		public double AverageConnectionDurationSeconds { get; set; }
		public double MinConnectionDurationSeconds { get; set; }
		public double MaxConnectionDurationSeconds { get; set; }
		public int TotalConnections { get; set; }
	}
}