

// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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



////// OpenRose - Requirements Management
////// Licensed under the Apache License, Version 2.0. 
////// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Text.Json;
////using System.Threading.Tasks;

////namespace OpenRose.WebUI.Client.Services.JsonFile
////{
////	/// <summary>
////	/// NEW SERVICE FOR JSON FILE DATA SOURCE FEATURE
////	/// 
////	/// JsonFileSchemaValidationService validates that an exported OpenRose JSON file
////	/// conforms to the expected structure before it can be loaded as a data source.
////	/// 
////	/// EXPLANATION: This service uses Microsoft's native System.Text.Json for parsing
////	/// and performs custom validation logic to ensure the JSON file structure matches
////	/// what we expect from an OpenRose export file. This approach:
////	/// - Uses only Microsoft built-in packages (no external dependencies)
////	/// - Provides clear, user-friendly error messages
////	/// - Validates the critical structure of the exported data
////	/// 
////	/// Validation checks:
////	/// 1. JSON is valid and can be parsed
////	/// 2. RepositoryId (GUID) is present
////	/// 3. At least one data type exists (Projects, ItemzTypes, Itemz, or Baselines)
////	/// 4. Required properties are present in expected locations
////	/// 5. IDs are valid GUIDs
////	/// </summary>
////	public class JsonFileSchemaValidationService
////	{
////		// ========================================================================
////		// CONSTANTS FOR VALIDATION
////		// ========================================================================

////		/// <summary>
////		/// Expected JSON property names in the export file (case-sensitive for JsonElement)
////		/// </summary>
////		private const string PROPERTY_NAME_REPOSITORY_ID = "RepositoryId";
////		private const string PROPERTY_NAME_PROJECTS = "Projects";
////		private const string PROPERTY_NAME_ITEMZ_TYPES = "ItemzTypes";
////		private const string PROPERTY_NAME_ITEMZ = "Itemz";
////		private const string PROPERTY_NAME_ITEMZ_TRACES = "ItemzTraces";
////		private const string PROPERTY_NAME_BASELINES = "Baselines";
////		private const string PROPERTY_NAME_BASELINE_ITEMZ_TYPES = "BaselineItemzTypes";
////		private const string PROPERTY_NAME_BASELINE_ITEMZ = "BaselineItemz";
////		private const string PROPERTY_NAME_BASELINE_ITEMZ_TRACES = "BaselineItemzTraces";

////		private const string PROPERTY_NAME_PROJECT = "Project";
////		private const string PROPERTY_NAME_BASELINE = "Baseline";
////		private const string PROPERTY_NAME_ID = "Id";
////		private const string PROPERTY_NAME_NAME = "Name";

////		// ========================================================================
////		// PRIVATE FIELDS
////		// ========================================================================

////		/// <summary>
////		/// JSON serializer options for consistent parsing across the service.
////		/// Uses default case-insensitive property matching.
////		/// </summary>
////		private JsonSerializerOptions _jsonDeserializationOptionsForValidation = new()
////		{
////			PropertyNameCaseInsensitive = true
////		};

////		// ========================================================================
////		// PUBLIC METHODS
////		// ========================================================================

////		/// <summary>
////		/// Validates that the provided JSON file content conforms to the OpenRose export structure.
////		/// This is called before loading the JSON file as a data source.
////		/// 
////		/// EXPLANATION: This method parses the JSON and performs structural validation
////		/// to ensure it's a valid OpenRose export file. If validation passes, the method
////		/// returns true. If it fails, it throws a JsonFileValidationException with a
////		/// user-friendly error message.
////		/// </summary>
////		/// <param name="jsonFileContentAsStringToValidate">
////		/// Complete JSON file content as a string read from the file.
////		/// </param>
////		/// <returns>
////		/// True if JSON file passes schema validation.
////		/// Throws JsonFileValidationException if validation fails.
////		/// </returns>
////		/// <exception cref="JsonFileValidationException">
////		/// Thrown when JSON file fails schema validation.
////		/// </exception>
////		public bool ValidateJsonFileContentAgainstSchema(string jsonFileContentAsStringToValidate)
////		{
////			try
////			{
////				// Validate input is not empty
////				if (string.IsNullOrWhiteSpace(jsonFileContentAsStringToValidate))
////				{
////					throw new JsonFileValidationException(
////						"JSON file content is empty. Please provide a valid OpenRose export file.");
////				}

////				// EXPLANATION: Parse the JSON content into a JsonDocument.
////				// JsonDocument provides DOM-like access to JSON structure using JsonElement.
////				// This is Microsoft's recommended approach for schema validation.
////				JsonDocument parsedJsonDocumentFromFile;
////				try
////				{
////					parsedJsonDocumentFromFile = JsonDocument.Parse(jsonFileContentAsStringToValidate);
////				}
////				catch (JsonException jsonParsingException)
////				{
////					throw new JsonFileValidationException(
////						"JSON file format is invalid. Unable to parse file as valid JSON. " +
////						"Please ensure the file is a valid OpenRose export file.",
////						jsonParsingException);
////				}

////				if (parsedJsonDocumentFromFile == null)
////				{
////					throw new JsonFileValidationException(
////						"JSON file content could not be parsed. File may be corrupted or empty.");
////				}

////				// Validate the root element structure
////				var rootElementFromParsedJson = parsedJsonDocumentFromFile.RootElement;

////				// STEP 1: Validate RepositoryId is present and is a valid GUID
////				if (!rootElementFromParsedJson.TryGetProperty(PROPERTY_NAME_REPOSITORY_ID, out var repositoryIdElementFromJson))
////				{
////					throw new JsonFileValidationException(
////						"JSON file is missing required 'RepositoryId' field. " +
////						"This is required for all OpenRose export files.");
////				}

////				string? repositoryIdAsString = repositoryIdElementFromJson.GetString();
////				if (string.IsNullOrWhiteSpace(repositoryIdAsString) || !Guid.TryParse(repositoryIdAsString, out _))
////				{
////					throw new JsonFileValidationException(
////						"JSON file contains invalid RepositoryId. " +
////						"RepositoryId must be a valid GUID.");
////				}

////				// STEP 2: Validate that at least one data type is present
////				bool hasProjectsDataType = JsonPropertyHasNonEmptyArray(rootElementFromParsedJson, PROPERTY_NAME_PROJECTS);
////				bool hasItemzTypesDataType = JsonPropertyHasNonEmptyArray(rootElementFromParsedJson, PROPERTY_NAME_ITEMZ_TYPES);
////				bool hasItemzDataType = JsonPropertyHasNonEmptyArray(rootElementFromParsedJson, PROPERTY_NAME_ITEMZ);
////				bool hasBaselinesDataType = JsonPropertyHasNonEmptyArray(rootElementFromParsedJson, PROPERTY_NAME_BASELINES);
////				bool hasBaselineItemzTypesDataType = JsonPropertyHasNonEmptyArray(rootElementFromParsedJson, PROPERTY_NAME_BASELINE_ITEMZ_TYPES);
////				bool hasBaselineItemzDataType = JsonPropertyHasNonEmptyArray(rootElementFromParsedJson, PROPERTY_NAME_BASELINE_ITEMZ);

////				if (!hasProjectsDataType && !hasItemzTypesDataType && !hasItemzDataType &&
////					!hasBaselinesDataType && !hasBaselineItemzTypesDataType && !hasBaselineItemzDataType)
////				{
////					throw new JsonFileValidationException(
////						"JSON file does not contain any valid OpenRose data. " +
////						"File should contain at least one of: Projects, ItemzTypes, Itemz, " +
////						"Baselines, BaselineItemzTypes, or BaselineItemz.");
////				}

////				// STEP 3: Validate structure based on what data types are present
////				ValidateProjectsStructureIfPresent(rootElementFromParsedJson);
////				ValidateBaselinesStructureIfPresent(rootElementFromParsedJson);

////				// STEP 4: Validate mutual exclusivity (only one export type per file)
////				int dataTypesPresent = 0;
////				if (hasProjectsDataType) dataTypesPresent++;
////				if (hasItemzTypesDataType) dataTypesPresent++;
////				if (hasItemzDataType) dataTypesPresent++;
////				if (hasBaselinesDataType) dataTypesPresent++;

////				// EXPLANATION: Per the schema, typically only one root data type should be exported
////				// However, we allow multiple as long as structure is valid.

////				return true;
////			}
////			catch (JsonFileValidationException jsonValidationException)
////			{
////				// Re-throw validation exceptions as-is
////				throw jsonValidationException;
////			}
////			catch (Exception generalException)
////			{
////				throw new JsonFileValidationException(
////					"An unexpected error occurred while validating the JSON file. " +
////					"Please contact your system administrator.",
////					generalException);
////			}
////		}

////		// ========================================================================
////		// PRIVATE HELPER METHODS
////		// ========================================================================

////		/// <summary>
////		/// Checks if a JSON property exists and contains a non-empty array.
////		/// </summary>
////		private bool JsonPropertyHasNonEmptyArray(JsonElement parentElementToCheck, string propertyNameToLookFor)
////		{
////			try
////			{
////				if (!parentElementToCheck.TryGetProperty(propertyNameToLookFor, out var propertyElementValue))
////				{
////					return false; // Property doesn't exist
////				}

////				// Property must be an array
////				if (propertyElementValue.ValueKind != JsonValueKind.Array)
////				{
////					return false;
////				}

////				// Array must not be empty
////				int arrayElementCount = 0;
////				foreach (var _ in propertyElementValue.EnumerateArray())
////				{
////					arrayElementCount++;
////				}

////				return arrayElementCount > 0;
////			}
////			catch
////			{
////				return false;
////			}
////		}

////		/// <summary>
////		/// Validates the structure of Projects if they are present in the JSON.
////		/// Checks that each Project has required properties.
////		/// </summary>
////		private void ValidateProjectsStructureIfPresent(JsonElement rootElement)
////		{
////			try
////			{
////				if (!rootElement.TryGetProperty(PROPERTY_NAME_PROJECTS, out var projectsArrayElement))
////				{
////					return; // Projects not present, nothing to validate
////				}

////				if (projectsArrayElement.ValueKind != JsonValueKind.Array)
////				{
////					throw new JsonFileValidationException(
////						"JSON file 'Projects' property must be an array.");
////				}

////				// Validate each project object
////				int projectIndexForErrorReporting = 0;
////				foreach (var projectNodeElement in projectsArrayElement.EnumerateArray())
////				{
////					if (projectNodeElement.ValueKind != JsonValueKind.Object)
////					{
////						throw new JsonFileValidationException(
////							$"JSON file Projects array contains non-object at index {projectIndexForErrorReporting}.");
////					}

////					// Each project node should have a "Project" property
////					if (!projectNodeElement.TryGetProperty(PROPERTY_NAME_PROJECT, out var projectObjectElement))
////					{
////						throw new JsonFileValidationException(
////							$"JSON file Projects[{projectIndexForErrorReporting}] is missing 'Project' property.");
////					}

////					ValidateProjectObjectProperties(projectObjectElement, projectIndexForErrorReporting);

////					projectIndexForErrorReporting++;
////				}
////			}
////			catch (JsonFileValidationException)
////			{
////				throw;
////			}
////			catch (Exception exceptionValidatingProjects)
////			{
////				throw new JsonFileValidationException(
////					"Error validating Projects structure in JSON file.",
////					exceptionValidatingProjects);
////			}
////		}

////		/// <summary>
////		/// Validates that a Project object has the required properties.
////		/// </summary>
////		private void ValidateProjectObjectProperties(JsonElement projectElement, int projectIndexForErrorReporting)
////		{
////			// Validate Id
////			if (!projectElement.TryGetProperty(PROPERTY_NAME_ID, out var idElement))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Projects[{projectIndexForErrorReporting}].Project is missing 'Id' property.");
////			}

////			string? projectIdAsString = idElement.GetString();
////			if (string.IsNullOrWhiteSpace(projectIdAsString) || !Guid.TryParse(projectIdAsString, out _))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Projects[{projectIndexForErrorReporting}].Project contains invalid 'Id'. " +
////					"Id must be a valid GUID.");
////			}

////			// Validate Name
////			if (!projectElement.TryGetProperty(PROPERTY_NAME_NAME, out var nameElement))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Projects[{projectIndexForErrorReporting}].Project is missing 'Name' property.");
////			}

////			string? projectName = nameElement.GetString();
////			if (string.IsNullOrWhiteSpace(projectName))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Projects[{projectIndexForErrorReporting}].Project 'Name' cannot be empty.");
////			}
////		}

////		/// <summary>
////		/// Validates the structure of Baselines if they are present in the JSON.
////		/// Checks that each Baseline has required properties.
////		/// </summary>
////		private void ValidateBaselinesStructureIfPresent(JsonElement rootElement)
////		{
////			try
////			{
////				if (!rootElement.TryGetProperty(PROPERTY_NAME_BASELINES, out var baselinesArrayElement))
////				{
////					return; // Baselines not present, nothing to validate
////				}

////				if (baselinesArrayElement.ValueKind != JsonValueKind.Array)
////				{
////					throw new JsonFileValidationException(
////						"JSON file 'Baselines' property must be an array.");
////				}

////				// Validate each baseline object
////				int baselineIndexForErrorReporting = 0;
////				foreach (var baselineNodeElement in baselinesArrayElement.EnumerateArray())
////				{
////					if (baselineNodeElement.ValueKind != JsonValueKind.Object)
////					{
////						throw new JsonFileValidationException(
////							$"JSON file Baselines array contains non-object at index {baselineIndexForErrorReporting}.");
////					}

////					// Each baseline node should have a "Baseline" property
////					if (!baselineNodeElement.TryGetProperty(PROPERTY_NAME_BASELINE, out var baselineObjectElement))
////					{
////						throw new JsonFileValidationException(
////							$"JSON file Baselines[{baselineIndexForErrorReporting}] is missing 'Baseline' property.");
////					}

////					ValidateBaselineObjectProperties(baselineObjectElement, baselineIndexForErrorReporting);

////					baselineIndexForErrorReporting++;
////				}
////			}
////			catch (JsonFileValidationException)
////			{
////				throw;
////			}
////			catch (Exception exceptionValidatingBaselines)
////			{
////				throw new JsonFileValidationException(
////					"Error validating Baselines structure in JSON file.",
////					exceptionValidatingBaselines);
////			}
////		}

////		/// <summary>
////		/// Validates that a Baseline object has the required properties.
////		/// </summary>
////		private void ValidateBaselineObjectProperties(JsonElement baselineElement, int baselineIndexForErrorReporting)
////		{
////			// Validate Id
////			if (!baselineElement.TryGetProperty(PROPERTY_NAME_ID, out var idElement))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Baselines[{baselineIndexForErrorReporting}].Baseline is missing 'Id' property.");
////			}

////			string? baselineIdAsString = idElement.GetString();
////			if (string.IsNullOrWhiteSpace(baselineIdAsString) || !Guid.TryParse(baselineIdAsString, out _))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Baselines[{baselineIndexForErrorReporting}].Baseline contains invalid 'Id'. " +
////					"Id must be a valid GUID.");
////			}

////			// Validate Name
////			if (!baselineElement.TryGetProperty(PROPERTY_NAME_NAME, out var nameElement))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Baselines[{baselineIndexForErrorReporting}].Baseline is missing 'Name' property.");
////			}

////			string? baselineName = nameElement.GetString();
////			if (string.IsNullOrWhiteSpace(baselineName))
////			{
////				throw new JsonFileValidationException(
////					$"JSON file Baselines[{baselineIndexForErrorReporting}].Baseline 'Name' cannot be empty.");
////			}
////		}
////	}

////	/// <summary>
////	/// Custom exception for JSON file schema validation errors.
////	/// This exception is thrown when a JSON file fails validation and includes
////	/// a user-friendly error message along with technical details for logging.
////	/// </summary>
////	public class JsonFileValidationException : Exception
////	{
////		/// <summary>
////		/// Creates a new validation exception with a user-friendly error message.
////		/// </summary>
////		public JsonFileValidationException(string userFriendlyErrorMessageToDisplay)
////			: base(userFriendlyErrorMessageToDisplay)
////		{
////			UserFriendlyErrorMessage = userFriendlyErrorMessageToDisplay;
////		}

////		/// <summary>
////		/// Creates a new validation exception with both error message and inner exception.
////		/// The inner exception is used for detailed logging and debugging.
////		/// </summary>
////		public JsonFileValidationException(string userFriendlyErrorMessageToDisplay, Exception innerExceptionForLogging)
////			: base(userFriendlyErrorMessageToDisplay, innerExceptionForLogging)
////		{
////			UserFriendlyErrorMessage = userFriendlyErrorMessageToDisplay;
////		}

////		/// <summary>
////		/// User-friendly error message to display in UI dialogs to the end user.
////		/// This should not contain technical jargon or stack traces.
////		/// </summary>
////		public string UserFriendlyErrorMessage { get; }
////	}
////}

