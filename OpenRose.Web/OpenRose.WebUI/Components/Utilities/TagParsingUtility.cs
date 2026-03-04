// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Components.Utilities
{
	/// <summary>
	/// EXPLANATION: NEW TAG PARSING UTILITY
	/// This utility provides methods to parse and format tags stored as pipe-delimited strings.
	/// It handles Unicode characters, special characters, and edge cases.
	/// </summary>
	public static class TagParsingUtility
	{
		/// <summary>
		/// Parses a pipe-delimited tag string into individual tags.
		/// </summary>
		/// <param name="tagsString">The pipe-delimited string of tags (e.g., "tag1|tag2|tag3")</param>
		/// <returns>List of individual tags, or empty list if null/empty</returns>
		public static List<string> ParseTags(string? tagsString)
		{
			if (string.IsNullOrWhiteSpace(tagsString))
				return new List<string>();

			return tagsString
				.Split('|', StringSplitOptions.RemoveEmptyEntries)
				.Select(tag => tag.Trim())
				.Where(tag => !string.IsNullOrWhiteSpace(tag))
				.ToList();
		}

		/// <summary>
		/// Joins individual tags back into a pipe-delimited string.
		/// </summary>
		/// <param name="tags">List of individual tags</param>
		/// <returns>Pipe-delimited string of tags</returns>
		public static string JoinTags(List<string>? tags)
		{
			if (tags == null || tags.Count == 0)
				return string.Empty;

			return string.Join("|", tags.Where(t => !string.IsNullOrWhiteSpace(t)));
		}

		/// <summary>
		/// Validates that the combined tags string doesn't exceed the 512 character limit.
		/// </summary>
		/// <param name="tagsString">The pipe-delimited string to validate</param>
		/// <returns>true if valid, false if exceeds limit</returns>
		public static bool ValidateTagsLength(string? tagsString)
		{
			return string.IsNullOrWhiteSpace(tagsString) || tagsString.Length <= 512;
		}
	}
}