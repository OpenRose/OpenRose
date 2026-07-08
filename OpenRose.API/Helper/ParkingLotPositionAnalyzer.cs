// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0.
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemzApp.API.Helper
{
	/// <summary>
	/// EXPLANATION: This class analyzes the position of the System "Parking Lot" ItemzType
	/// within the imported JSON hierarchy. The Parking Lot ItemzType is identified by checking
	/// both the Name and IsSystem flag. Only ItemzTypes with Name="Parking Lot" AND IsSystem=true
	/// are considered the system-generated Parking Lot. User-defined ItemzTypes with the same
	/// name but IsSystem=false are treated as normal custom ItemzTypes.
	/// 
	/// When a project is imported, OpenRose creates a default System Parking Lot at the time
	/// of project creation. This analyzer ensures that the imported System Parking Lot is
	/// repositioned to match the location it had in the original export file (top, bottom, or between).
	/// </summary>
	public class ParkingLotPositionAnalyzer
	{
		/// <summary>
		/// Analyzes the position of the System "Parking Lot" ItemzType relative to other ItemzTypes
		/// in the imported JSON hierarchy.
		/// 
		/// EXPLANATION: This method searches for an ItemzType with both:
		/// - Name: "Parking Lot"
		/// - IsSystem: true
		/// 
		/// If found, it determines whether the Parking Lot is positioned at the top, bottom,
		/// or between two other ItemzTypes. This metadata is used later to reposition the
		/// default Parking Lot created during project creation to match the original position.
		/// </summary>
		/// <param name="itemzTypeNodes">List of ItemzType nodes from the import JSON</param>
		/// <returns>
		/// ParkingLotPositionMetadata containing position information about the System Parking Lot.
		/// If no System Parking Lot is found, Exists will be false.
		/// </returns>
		public static ParkingLotPositionMetadata AnalyzeParkingLotPosition(
			List<ItemzTypeImportNode> itemzTypeNodes)
		{
			if (itemzTypeNodes == null || itemzTypeNodes.Count == 0)
				return new ParkingLotPositionMetadata { Exists = false };

			// EXPLANATION: Find the SYSTEM Parking Lot (IsSystem=true AND Name="Parking Lot")
			// User-defined ItemzTypes with the same name are NOT considered the system Parking Lot
			var parkingLotIndex = itemzTypeNodes.FindIndex(it =>
				string.Equals(it.ItemzType.Name, "Parking Lot", StringComparison.OrdinalIgnoreCase) &&
				it.ItemzType.IsSystem == true);

			if (parkingLotIndex == -1)
			{
				// No System Parking Lot found in the JSON
				return new ParkingLotPositionMetadata { Exists = false };
			}

			var metadata = new ParkingLotPositionMetadata
			{
				Exists = true,
				ParkingLotIndex = parkingLotIndex,
				TotalItemzTypeCount = itemzTypeNodes.Count,
				ParkingLotId = itemzTypeNodes[parkingLotIndex].ItemzType.Id
			};

			// EXPLANATION: Determine the relative position of the Parking Lot within the ItemzType list
			// This distinction is important because the position determines which move operation to use
			if (parkingLotIndex == 0)
			{
				// Parking Lot is at the top of the ItemzType list
				metadata.Position = ParkingLotPosition.Top;
			}
			else if (parkingLotIndex == itemzTypeNodes.Count - 1)
			{
				// Parking Lot is at the bottom of the ItemzType list
				metadata.Position = ParkingLotPosition.Bottom;
			}
			else
			{
				// Parking Lot is between two ItemzTypes
				metadata.Position = ParkingLotPosition.Between;
				metadata.PreviousItemzTypeId = itemzTypeNodes[parkingLotIndex - 1].ItemzType.Id;
				metadata.NextItemzTypeId = itemzTypeNodes[parkingLotIndex + 1].ItemzType.Id;
			}

			return metadata;
		}
	}

	/// <summary>
	/// EXPLANATION: This class holds metadata about the System Parking Lot's position
	/// in the imported JSON file. It is used to determine which repositioning strategy
	/// should be applied after the project is created and all ItemzTypes are imported.
	/// </summary>
	public class ParkingLotPositionMetadata
	{
		/// <summary>
		/// Indicates whether a System Parking Lot ItemzType was found in the JSON file
		/// </summary>
		public bool Exists { get; set; }

		/// <summary>
		/// The position of the Parking Lot relative to other ItemzTypes (Top, Bottom, or Between)
		/// </summary>
		public ParkingLotPosition Position { get; set; }

		/// <summary>
		/// The ID of the Parking Lot ItemzType from the JSON file
		/// </summary>
		public Guid ParkingLotId { get; set; }

		/// <summary>
		/// The zero-based index of the Parking Lot in the ItemzTypes list
		/// </summary>
		public int ParkingLotIndex { get; set; }

		/// <summary>
		/// The total number of ItemzTypes in the JSON file for this project
		/// </summary>
		public int TotalItemzTypeCount { get; set; }

		/// <summary>
		/// EXPLANATION: Used when Parking Lot is positioned between two ItemzTypes.
		/// This is the ID of the ItemzType that comes immediately before the Parking Lot.
		/// </summary>
		public Guid? PreviousItemzTypeId { get; set; }

		/// <summary>
		/// EXPLANATION: Used when Parking Lot is positioned between two ItemzTypes.
		/// This is the ID of the ItemzType that comes immediately after the Parking Lot.
		/// </summary>
		public Guid? NextItemzTypeId { get; set; }
	}

	/// <summary>
	/// EXPLANATION: Enumeration representing the position of the System Parking Lot
	/// relative to other ItemzTypes in the project.
	/// </summary>
	public enum ParkingLotPosition
	{
		/// <summary>
		/// Parking Lot is the first ItemzType in the project
		/// </summary>
		Top,

		/// <summary>
		/// Parking Lot is the last ItemzType in the project
		/// </summary>
		Bottom,

		/// <summary>
		/// Parking Lot is positioned between two other ItemzTypes
		/// </summary>
		Between
	}
}