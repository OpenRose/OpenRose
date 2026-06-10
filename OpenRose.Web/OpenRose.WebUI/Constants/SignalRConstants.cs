// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Constants
{
	/// <summary>
	/// SignalR Hub and Event Constants
	/// This file centralizes all SignalR configuration and event names
	/// to prevent hard-coded strings and ensure consistency across the application.
	/// Any changes here automatically propagate to all consumers.
	/// </summary>
	public static class SignalRConstants
	{
		/// <summary>
		/// SignalR Hub Paths
		/// </summary>
		public static class HubPaths
		{
			public const string ProjectNotificationHub = "/projectNotificationHub";
			public const string RequirementNotificationHub = "/requirementNotificationHub";
			public const string BaselineNotificationHub = "/baselineNotificationHub";
			public const string EstimationNotificationHub = "/estimationNotificationHub";
			public const string TraceabilityNotificationHub = "/traceabilityNotificationHub";
		}

		/// <summary>
		/// Project-related Events
		/// </summary>
		public static class ProjectEvents
		{
			public const string ProjectCreated = "ProjectCreated";
			public const string ProjectDeleted = "ProjectDeleted";
			public const string ProjectModified = "ProjectModified";
		}

		/// <summary>
		/// Requirement Item Type Events
		/// </summary>
		public static class RequirementItemTypeEvents
		{
			public const string ItemzTypeCreated = "ItemzTypeCreated";
			public const string ItemzTypeDeleted = "ItemzTypeDeleted";
			public const string ItemzTypeModified = "ItemzTypeModified";
			public const string ItemzTypeMoved = "ItemzTypeMoved";
		}

		/// <summary>
		/// Requirement Item Events
		/// </summary>
		public static class RequirementItemEvents
		{
			public const string ItemzCreated = "ItemzCreated";
			public const string ItemzDeleted = "ItemzDeleted";
			public const string ItemzModified = "ItemzModified";
			public const string ItemzMoved = "ItemzMoved";
		}

		/// <summary>
		/// Baseline Events
		/// </summary>
		public static class BaselineEvents
		{
			public const string BaselineCreated = "BaselineCreated";
			public const string BaselineDeleted = "BaselineDeleted";
			public const string BaselineModified = "BaselineModified";
		}

		/// <summary>
		/// Baseline Item Type Events
		/// </summary>
		public static class BaselineItemTypeEvents
		{
			public const string BaselineItemzTypeCreated = "BaselineItemzTypeCreated";
			public const string BaselineItemzTypeDeleted = "BaselineItemzTypeDeleted";
			public const string BaselineItemzTypeModified = "BaselineItemzTypeModified";
		}

		/// <summary>
		/// Baseline Item Events
		/// </summary>
		public static class BaselineItemEvents
		{
			public const string BaselineItemzCreated = "BaselineItemzCreated";
			public const string BaselineItemzDeleted = "BaselineItemzDeleted";
			public const string BaselineItemzModified = "BaselineItemzModified";
		}

		/// <summary>
		/// Estimation Events
		/// </summary>
		public static class EstimationEvents
		{
			public const string ProjectEstimationUpdated = "ProjectEstimationUpdated";
			public const string ItemzTypeEstimationUpdated = "ItemzTypeEstimationUpdated";
			public const string ItemzEstimationUpdated = "ItemzEstimationUpdated";
		}

		/// <summary>
		/// Traceability Events
		/// </summary>
		public static class TraceabilityEvents
		{
			public const string TraceabilityLinkCreated = "TraceabilityLinkCreated";
			public const string TraceabilityLinkDeleted = "TraceabilityLinkDeleted";
		}

		/// <summary>
		/// SignalR Group Name Patterns
		/// Use these to organize clients into groups for targeted notifications
		/// </summary>
		public static class GroupNamePatterns
		{
			public const string ProjectGroup = "project-{0}";
			public const string ItemzTypeGroup = "itemztype-{0}";
			public const string ItemzGroup = "itemz-{0}";
			public const string BaselineGroup = "baseline-{0}";
			public const string ProjectEstimationGroup = "projectestimation-{0}";
			public const string ItemzTypeEstimationGroup = "itemztypeestimation-{0}";
			public const string ItemzEstimationGroup = "itemzestimation-{0}";
			public const string TraceabilityGroup = "traceability-{0}-{1}";

			/// <summary>
			/// Helper method to generate group names
			/// </summary>
			public static string GetProjectGroup(Guid projectId) => string.Format(ProjectGroup, projectId);
			public static string GetItemzTypeGroup(Guid itemzTypeId) => string.Format(ItemzTypeGroup, itemzTypeId);
			public static string GetItemzGroup(Guid itemzId) => string.Format(ItemzGroup, itemzId);
			public static string GetBaselineGroup(Guid baselineId) => string.Format(BaselineGroup, baselineId);
			public static string GetProjectEstimationGroup(Guid projectId) => string.Format(ProjectEstimationGroup, projectId);
			public static string GetItemzTypeEstimationGroup(Guid itemzTypeId) => string.Format(ItemzTypeEstimationGroup, itemzTypeId);
			public static string GetItemzEstimationGroup(Guid itemzId) => string.Format(ItemzEstimationGroup, itemzId);
			public static string GetTraceabilityGroup(Guid fromItemzId, Guid toItemzId) => string.Format(TraceabilityGroup, fromItemzId, toItemzId);
		}
	}
}