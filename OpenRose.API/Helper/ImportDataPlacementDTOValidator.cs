// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.Models;
using System;
using System.Collections.Generic;

namespace ItemzApp.API.Helper
{
	public static class ImportDataPlacementDTOValidator
	{
		private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"ItemzType", "BaselineItemzType", "Itemz", "BaselineItemz"
		};

		public static (bool IsValid, string ErrorMessage) ValidatePlacement(string detectedType, ImportDataPlacementDTO dto)
		{
			if (!SupportedTypes.Contains(detectedType))
			{
				return (false, $"Unsupported detectedType '{detectedType}'. Must be one of: {string.Join(", ", SupportedTypes)}.");
			}

			if (string.Equals(detectedType, "ItemzType", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(detectedType, "BaselineItemzType", StringComparison.OrdinalIgnoreCase))
			{
				dto.FirstItemzId = null;
				dto.SecondItemzId = null;

				return ValidatePlacementLogic(
					dto.AtBottomOfChildNodes,
					dto.FirstItemzTypeId,
					dto.SecondItemzTypeId,
					detectedType
				);
			}

			if (string.Equals(detectedType, "Itemz", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(detectedType, "BaselineItemz", StringComparison.OrdinalIgnoreCase))
			{
				dto.FirstItemzTypeId = null;
				dto.SecondItemzTypeId = null;

				return ValidatePlacementLogic(
					dto.AtBottomOfChildNodes,
					dto.FirstItemzId,
					dto.SecondItemzId,
					detectedType
				);
			}

			return (false, "Unknown import entity type.");
		}

		private static (bool IsValid, string ErrorMessage) ValidatePlacementLogic(
			bool atBottom,
			Guid? firstId,
			Guid? secondId,
			string entityType)
		{
			bool isBetween = firstId.HasValue && secondId.HasValue;
			bool isSimple = !firstId.HasValue && !secondId.HasValue;

			if (!isSimple && !isBetween)
			{
				return (false, $"For {entityType} import, you must provide both First and Second IDs to position between two nodes — or leave them blank to place at bottom.");
			}

			return (true, string.Empty);
		}
	}
}
