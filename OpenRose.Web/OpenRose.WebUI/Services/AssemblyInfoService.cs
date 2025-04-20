// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System.Reflection;

namespace OpenRose.WebUI.Services
{
    public class AssemblyInfoService
    {
        public string GetAssemblyVersion()
        {
            // var version = Assembly.GetExecutingAssembly().GetName().Version;
            // return version?.ToString() ?? "Version not found";

			var assembly = Assembly.GetExecutingAssembly();
			var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
			return informationalVersion ?? "Version not set";
    	}
    }
}
