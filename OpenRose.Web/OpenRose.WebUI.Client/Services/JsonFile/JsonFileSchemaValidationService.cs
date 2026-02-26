

// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace OpenRose.WebUI.Client.Services.JsonFile
{
	/// <summary>
	/// NEW SERVICE FOR JSON FILE DATA SOURCE FEATURE
	/// 
	/// JsonFileSchemaValidationService validates that an exported OpenRose JSON file
	/// conforms to the OpenRose export schema before it can be loaded as a data source.
	/// 
	/// EXPLANATION: This service reuses the same OpenRose.Export.schema.1.0.json file
	/// that already exists in the API project for import validation. We copy this schema
	/// to the WebUI.Client project and use it here to validate JSON files before loading.
	/// This ensures consistency between what the API accepts and what the WebUI client loads.
	/// 
	/// The validation process:
	/// 1. Load the JSON schema from embedded resources
	/// 2. Deserialize the JSON file content
	/// 3. Validate against the schema using NJsonSchema/NewtonsoftJson
	/// 4. Return result with user-friendly error messages if validation fails
	/// </summary>
	public class JsonFileSchemaValidationService
	{
		// ========================================================================
		// PRIVATE FIELDS
		// ========================================================================

		/// <summary>
		/// Cached JSON schema loaded from embedded resources in OpenRose.WebUI.Client project.
		/// The schema file should be located at: 
		/// OpenRose.Web/OpenRose.WebUI.Client/Schemas/OpenRose.Export.schema.1.0.json
		/// 
		/// EXPLANATION: We cache the schema after first load to avoid reloading it
		/// from resources every time validation is called.
		/// </summary>
		private JSchema? _cachedExportSchemaForValidation = null;

		private bool _isSchemaLoadedIntoCache = false;

		// ========================================================================
		// PUBLIC METHODS
		// ========================================================================

		/// <summary>
		/// Validates that the provided JSON file content conforms to the OpenRose export schema.
		/// This is called before loading the JSON file as a data source.
		/// 
		/// EXPLANATION: This method uses the same validation approach as the API's ImportController.
		/// It parses the JSON and validates it against the schema using Newtonsoft.Json.Schema.
		/// If validation passes, the method returns true. If it fails, it throws a
		/// JsonFileValidationException with a user-friendly error message.
		/// </summary>
		/// <param name="jsonFileContentAsStringToValidate">
		/// Complete JSON file content as a string read from the file.
		/// </param>
		/// <returns>
		/// True if JSON file passes schema validation.
		/// Throws JsonFileValidationException if validation fails.
		/// </returns>
		/// <exception cref="JsonFileValidationException">
		/// Thrown when JSON file fails schema validation.
		/// </exception>
		public bool ValidateJsonFileContentAgainstSchema(string jsonFileContentAsStringToValidate)
		{
			try
			{
				// Validate input is not empty
				if (string.IsNullOrWhiteSpace(jsonFileContentAsStringToValidate))
				{
					throw new JsonFileValidationException(
						"JSON file content is empty. Please provide a valid OpenRose export file.");
				}

				// EXPLANATION: Parse the JSON content into a JObject.
				// JObject is from Newtonsoft.Json and allows flexible property access
				// similar to how the API does it in ImportController.
				JObject deserializedJsonObjectFromFile;
				try
				{
					deserializedJsonObjectFromFile = JObject.Parse(jsonFileContentAsStringToValidate);
				}
				catch (JsonReaderException jsonParsingException)
				{
					throw new JsonFileValidationException(
						"JSON file format is invalid. Unable to parse file as valid JSON. " +
						"Please ensure the file is a valid OpenRose export file.",
						jsonParsingException);
				}

				if (deserializedJsonObjectFromFile == null)
				{
					throw new JsonFileValidationException(
						"JSON file content could not be parsed. File may be corrupted or empty.");
				}

				// Load the schema from embedded resources if not already cached
				if (!_isSchemaLoadedIntoCache)
				{
					LoadJsonSchemaFromEmbeddedResources();
				}

				if (_cachedExportSchemaForValidation == null)
				{
					throw new JsonFileValidationException(
						"Unable to load validation schema. Please contact your system administrator.");
				}

				// EXPLANATION: Validate the JSON object against the schema.
				// This is the same approach used in ImportController.cs
				// The Validate method returns a list of validation errors if any exist.
				var validationErrorsFromSchemaCheck = new List<string>();
				bool isValidAgainstSchema = deserializedJsonObjectFromFile.IsValid(
					_cachedExportSchemaForValidation,
					out IList<string> schemaValidationErrorMessagesFromLibrary
				);

				// Convert validation errors to user-friendly messages
				if (!isValidAgainstSchema && schemaValidationErrorMessagesFromLibrary != null)
				{
					validationErrorsFromSchemaCheck = schemaValidationErrorMessagesFromLibrary.ToList();
				}

				// If validation failed, throw exception with error details
				if (!isValidAgainstSchema)
				{
					string allErrorMessagesJoinedForDisplay = string.Join("; ", validationErrorsFromSchemaCheck);
					throw new JsonFileValidationException(
						$"JSON file format does not match OpenRose export schema. Error details: {allErrorMessagesJoinedForDisplay}");
				}

				// Additional validation: Ensure at least one data type is present
				// (similar to how ImportController validates this)
				bool hasProjectData = deserializedJsonObjectFromFile["Projects"] != null &&
									   deserializedJsonObjectFromFile["Projects"]!.HasValues;
				bool hasItemzTypeData = deserializedJsonObjectFromFile["ItemzTypes"] != null &&
										deserializedJsonObjectFromFile["ItemzTypes"]!.HasValues;
				bool hasItemzData = deserializedJsonObjectFromFile["Itemz"] != null &&
									 deserializedJsonObjectFromFile["Itemz"]!.HasValues;
				bool hasBaselineData = deserializedJsonObjectFromFile["Baselines"] != null &&
										deserializedJsonObjectFromFile["Baselines"]!.HasValues;
				bool hasBaselineItemzTypeData = deserializedJsonObjectFromFile["BaselineItemzTypes"] != null &&
											   deserializedJsonObjectFromFile["BaselineItemzTypes"]!.HasValues;
				bool hasBaselineItemzData = deserializedJsonObjectFromFile["BaselineItemz"] != null &&
											deserializedJsonObjectFromFile["BaselineItemz"]!.HasValues;

				if (!hasProjectData && !hasItemzTypeData && !hasItemzData &&
					!hasBaselineData && !hasBaselineItemzTypeData && !hasBaselineItemzData)
				{
					throw new JsonFileValidationException(
						"JSON file does not contain any valid OpenRose data. " +
						"File should contain at least one of: Projects, ItemzTypes, Itemz, " +
						"Baselines, BaselineItemzTypes, or BaselineItemz.");
				}

				// If we got here, validation passed
				return true;
			}
			catch (JsonFileValidationException jsonValidationException)
			{
				// Re-throw validation exceptions as-is
				throw jsonValidationException;
			}
			catch (Exception generalException)
			{
				throw new JsonFileValidationException(
					"An unexpected error occurred while validating the JSON file. " +
					"Please contact your system administrator.",
					generalException);
			}
		}

		// ========================================================================
		// PRIVATE HELPER METHODS
		// ========================================================================

		/// <summary>
		/// Loads the OpenRose export schema from embedded resources in the WebUI.Client project.
		/// The schema file path is: Schemas/OpenRose.Export.schema.1.0.json
		/// 
		/// EXPLANATION: The schema file is embedded as a resource in the project, so we
		/// load it using reflection to get the resource stream. This ensures the schema
		/// is always available without requiring external file access.
		/// </summary>
		private void LoadJsonSchemaFromEmbeddedResources()
		{
			try
			{
				// EXPLANATION: Get the assembly where this service is defined
				var currentAssemblyForResourceLoading = typeof(JsonFileSchemaValidationService).Assembly;

				// Resource name format: {RootNamespace}.{Folder}.{FileName}
				// For OpenRose.WebUI.Client project with Schemas folder: 
				// OpenRose.WebUI.Client.Schemas.OpenRose.Export.schema.1.0.json
				const string schemaResourceNameInEmbeddedResources =
					"OpenRose.WebUI.Client.Schemas.OpenRose.Export.schema.1.0.json";

				using (var schemaResourceStreamFromEmbeddedResources =
					   currentAssemblyForResourceLoading.GetManifestResourceStream(schemaResourceNameInEmbeddedResources))
				{
					if (schemaResourceStreamFromEmbeddedResources == null)
					{
						throw new InvalidOperationException(
							$"Could not find embedded schema resource: {schemaResourceNameInEmbeddedResources}");
					}

					using (var streamReaderForSchemaContent = new System.IO.StreamReader(schemaResourceStreamFromEmbeddedResources))
					{
						string schemaJsonContentAsString = streamReaderForSchemaContent.ReadToEnd();

						// Parse the schema JSON and create a JSchema object
						// JSchema is from Newtonsoft.Json.Schema package
						_cachedExportSchemaForValidation = JSchema.Parse(schemaJsonContentAsString);
						_isSchemaLoadedIntoCache = true;
					}
				}
			}
			catch (InvalidOperationException resourceNotFoundException)
			{
				throw new JsonFileValidationException(
					"Unable to load OpenRose export schema. The schema file may be missing from the application. " +
					"Please contact your system administrator.",
					resourceNotFoundException);
			}
			catch (Exception schemaParsingException)
			{
				throw new JsonFileValidationException(
					"Error parsing the OpenRose export schema. Please contact your system administrator.",
					schemaParsingException);
			}
		}
	}

	/// <summary>
	/// Custom exception for JSON file schema validation errors.
	/// This exception is thrown when a JSON file fails validation and includes
	/// a user-friendly error message along with technical details for logging.
	/// </summary>
	public class JsonFileValidationException : Exception
	{
		/// <summary>
		/// Creates a new validation exception with a user-friendly error message.
		/// </summary>
		public JsonFileValidationException(string userFriendlyErrorMessageToDisplay)
			: base(userFriendlyErrorMessageToDisplay)
		{
			UserFriendlyErrorMessage = userFriendlyErrorMessageToDisplay;
		}

		/// <summary>
		/// Creates a new validation exception with both error message and inner exception.
		/// The inner exception is used for detailed logging and debugging.
		/// </summary>
		public JsonFileValidationException(string userFriendlyErrorMessageToDisplay, Exception innerExceptionForLogging)
			: base(userFriendlyErrorMessageToDisplay, innerExceptionForLogging)
		{
			UserFriendlyErrorMessage = userFriendlyErrorMessageToDisplay;
		}

		/// <summary>
		/// User-friendly error message to display in UI dialogs to the end user.
		/// This should not contain technical jargon or stack traces.
		/// </summary>
		public string UserFriendlyErrorMessage { get; }
	}
}
