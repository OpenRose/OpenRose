// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


// using Newtonsoft.Json;
// using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Client.SharedEntities
{

	/// <summary>
	/// Project Status list values as ENUM
	/// </summary>
	// [JsonConverter(typeof(StringEnumConverter))]
	public enum ProjectStatus
	{
		// EXPLANATION - We have added Medium as 0 (ZERO) becauase that's the default value to be used by EF Core if no value is provided.
		// This is called sentinel value as far as .NET compilation warning is concerned.
		// See comment by use David Liang at ... https://stackoverflow.com/a/77978854
		New = 0, Approved = 1, Active = 2, OnHold = 3, Completed = 4, Closed = 5
	}

	/// <summary>
	/// ItemzType Status list values as ENUM
	/// </summary>
	// [JsonConverter(typeof(StringEnumConverter))]
	public enum ItemzTypeStatus
	{
		// EXPLANATION - We have added Medium as 0 (ZERO) becauase that's the default value to be used by EF Core if no value is provided.
		// This is called sentinel value as far as .NET compilation warning is concerned.
		// See comment by use David Liang at ... https://stackoverflow.com/a/77978854
		New = 0, Approved = 1, Active = 2, OnHold = 3, Completed = 4, Closed = 5
	}

	/// <summary>
	/// Itemz Severity list values as ENUM
	/// </summary>
	// [JsonConverter(typeof(StringEnumConverter))]
	public enum ItemzSeverity
    {
		// EXPLANATION - We have added Medium as 0 (ZERO) becauase that's the default value to be used by EF Core if no value is provided.
		// This is called sentinel value as far as .NET compilation warning is concerned.
		// See comment by use David Liang at ... https://stackoverflow.com/a/77978854
		Low = 0, Medium = 1, High = 2
	}

	/// <summary>
	/// Itemz Priority list values as ENUM
	/// </summary>
	// [JsonConverter(typeof(StringEnumConverter))]
	public enum ItemzPriority
	{
		// EXPLANATION - We have added Medium as 0 (ZERO) becauase that's the default value to be used by EF Core if no value is provided.
		// This is called sentinel value as far as .NET compilation warning is concerned.
		// See comment by use David Liang at ... https://stackoverflow.com/a/77978854
		Low = 0, Medium = 1, High = 2
	}

	/// <summary>
	/// Itemz Status list values as ENUM
	/// </summary>
	// [JsonConverter(typeof(StringEnumConverter))]
	public enum ItemzStatus
	{
		// EXPLANATION - We have added Medium as 0 (ZERO) becauase that's the default value to be used by EF Core if no value is provided.
		// This is called sentinel value as far as .NET compilation warning is concerned.
		// See comment by use David Liang at ... https://stackoverflow.com/a/77978854
		New = 0, Approved = 1, Active = 2, OnHold = 3, Completed = 4, Closed = 5
	}
}
