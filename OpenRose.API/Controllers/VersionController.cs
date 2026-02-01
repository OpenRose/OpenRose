// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace ItemzApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class VersionController : ControllerBase
	{
		[HttpGet(Name = "GetVersion")]
		[HttpHead(Name = "__HEAD_APIVersion_Get_Version_Info__")]
		public IActionResult GetVersion()
		{
			var assembly = Assembly.GetExecutingAssembly();

			var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
			var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "";
			var assemblyVersion = assembly.GetName().Version?.ToString() ?? "";

			//var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToArray();
			//string branchName = metadata.FirstOrDefault(m => m.Key == "BranchName")?.Value ?? "";
			//string commitSha = metadata.FirstOrDefault(m => m.Key == "CommitSha")?.Value ?? "";
			//string buildDateTime = metadata.FirstOrDefault(m => m.Key == "BuildDateTime")?.Value ?? "";
			//string buildNumber = metadata.FirstOrDefault(m => m.Key == "BuildNumber")?.Value ?? "";

			var result = new
			{
				InformationalVersion = informationalVersion,
				FileVersion = fileVersion,
				AssemblyVersion = assemblyVersion
				//BranchName = branchName,
				//CommitSha = commitSha,
				//BuildDateTime = buildDateTime,
				//BuildNumber = buildNumber
			};

			return Ok(result);
		}
	}
}