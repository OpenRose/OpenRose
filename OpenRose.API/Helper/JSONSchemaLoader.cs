// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ItemzApp.API.Helper
{
	public static class SchemaLoader
	{
		public static async Task<JSchema> LoadSchemaAsync(Assembly? assembly = null)
		{

			const string resourceName = "OpenRose.API.Schemas.OpenRose.Export.schema.1.0.json";

			assembly ??= Assembly.GetExecutingAssembly();

			try
			{
				using var stream = assembly.GetManifestResourceStream(resourceName);

				if (stream == null)
				{
					var available = string.Join("," + Environment.NewLine, assembly.GetManifestResourceNames());

					throw new FileNotFoundException(
						$"Embedded schema resource '{resourceName}' was not found." +
						$"{Environment.NewLine}Available resources:{Environment.NewLine}{available}"
					);
				}

				using var reader = new StreamReader(stream);
				var schemaJson = await reader.ReadToEndAsync();

				return JSchema.Parse(schemaJson);
			}
			catch (FileNotFoundException ex)
			{
				// _logger.LogError(ex, "Schema resource missing");
				throw new ApplicationException(ex.Message, ex);
			}
			catch (JsonException ex)
			{
				// _logger.LogError(ex, "Schema JSON invalid");
				throw new ApplicationException("The JSON schema file is present but contains invalid JSON.", ex);
			}
			catch (Exception ex)
			{
				// _logger.LogError(ex, "Unexpected schema load error");
				throw new ApplicationException("An unexpected error occurred while loading the JSON schema.", ex);
			}
		}
	}

}
