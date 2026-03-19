
// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

namespace OpenRose.WebUI.Client.SharedModels.ClientSideUIOnlyModel
{
	public class JsonViewerMetadata
	{
		public string FileName { get; set; } = "";
		public string? FullPath { get; set; }
		public long FileSizeBytes { get; set; }
		public DateTime LoadedAt { get; set; }
		public bool IsServerSide { get; set; }
	}

}
