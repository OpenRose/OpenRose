// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenRose.WebUI.Client.SharedModels;
using OpenRose.WebUI.Client.SharedModels.ClientSideUIOnlyModel;
using OpenRose.WebUI.Client.SharedModels.JsonOnly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenRose.WebUI.Client.Services.JsonFile
{

	public enum JsonExportKind
	{
		Unknown,
		Project,
		ItemzType,
		Itemz,
		Baseline,
		BaselineItemzType,
		BaselineItemz
	}

	/// <summary>
	/// JSON-backed data source for OpenRose exports (LIVE or BASELINE, never both).
	/// It:
	/// - Loads a validated export JSON (matching RepositoryExport schema)
	/// - Detects whether it is LIVE or BASELINE mode
	/// - Builds a parent/child hierarchy graph
	/// - Exposes hierarchy DTOs for TreeView
	/// - Exposes record DTOs for read-only detail components
	/// - Exposes traceability data for Itemz / BaselineItemz
	/// </summary>
	/// 
	public class JsonFileDataSourceService
	{
		private enum RepositoryExportKind
		{
			Unknown = 0,
			Live = 1,
			Baseline = 2
		}




		private JObject? _root;
		private RepositoryExportKind _exportKind = RepositoryExportKind.Unknown;
		private bool _isLoaded;

		// Raw node lookup (wrapper nodes: ProjectExportNode, ItemzTypeExportNode, ItemzExportNode, etc.)
		private readonly Dictionary<Guid, JToken> _nodeById = new();

		// Hierarchy metadata
		private readonly Dictionary<Guid, HierarchyIdRecordDetailsDTO> _hierarchyById = new();
		private readonly Dictionary<Guid, Guid?> _parentById = new();
		private readonly Dictionary<Guid, List<Guid>> _childrenByParent = new();
		private readonly HashSet<Guid> _rootIds = new();

		// Traceability
		private readonly List<JToken> _itemzTraces = new();
		private readonly List<JToken> _baselineItemzTraces = new();

		public bool IsLoaded => _isLoaded;

		/// <summary>
		/// Indicates whether the loaded JSON represents LIVE or BASELINE export.
		/// </summary>
		public bool IsLiveExport => _exportKind == RepositoryExportKind.Live;
		public bool IsBaselineExport => _exportKind == RepositoryExportKind.Baseline;
		public bool IsJsonLoaded => _isLoaded;

		private Dictionary<Guid, bool> _baselineItemzIncluded = new();



		#region Load / Unload

		public async Task LoadJsonFileDataFromFileSystemAsync(string jsonFileFullPath)
		{
			if (string.IsNullOrWhiteSpace(jsonFileFullPath))
				throw new ArgumentException("JSON file path is null or empty.", nameof(jsonFileFullPath));

			if (!File.Exists(jsonFileFullPath))
				throw new FileNotFoundException("JSON file not found.", jsonFileFullPath);

			var content = await File.ReadAllTextAsync(jsonFileFullPath).ConfigureAwait(false);
			await LoadJsonFileDataFromStringAsync(content).ConfigureAwait(false);
		}

		public Task LoadJsonFileDataFromStringAsync(string jsonContent)
		{
			if (string.IsNullOrWhiteSpace(jsonContent))
				throw new InvalidOperationException("JSON content is empty or null.");

			try
			{
				_root = JObject.Parse(jsonContent);
			}
			catch (JsonException ex)
			{
				throw new InvalidOperationException("Error parsing JSON content. File may not be valid JSON.", ex);
			}

			BuildIndexes();
			_isLoaded = true;

			return Task.CompletedTask;
		}

		public void UnloadJsonFileData()
		{
			_root = null;
			_exportKind = RepositoryExportKind.Unknown;
			_isLoaded = false;

			_nodeById.Clear();
			_hierarchyById.Clear();
			_parentById.Clear();
			_childrenByParent.Clear();
			_rootIds.Clear();
			_itemzTraces.Clear();
			_baselineItemzTraces.Clear();
		}

		#endregion

		#region Hierarchy API

		private void EnsureLoaded()
		{
			if (!_isLoaded || _root == null)
				throw new InvalidOperationException("JSON file data is not loaded. Please load a JSON file before querying.");
		}


		public bool TryGetIsIncluded(Guid id, out bool isIncluded)
		{
			return _baselineItemzIncluded.TryGetValue(id, out isIncluded);
		}

		/// <summary>
		/// Returns all root record IDs detected from the JSON (Project, ItemzType, Itemz, Baseline, BaselineItemzType, BaselineItemz).
		/// </summary>
		public IReadOnlyCollection<Guid> GetRootRecordIds()
		{
			EnsureLoaded();
			return _rootIds.ToList().AsReadOnly();
		}

		public Task<HierarchyIdRecordDetailsDTO?> GetHierarchyRecordDetailsByGuidAsync(Guid id)
		{
			EnsureLoaded();
			_hierarchyById.TryGetValue(id, out var dto);
			return Task.FromResult(dto);
		}

		//public async Task<JsonBaselineHierarchyNodeDTO?> GetBaselineHierarchyJsonAsync(Guid rootId)
		//{
		//	EnsureLoaded();

		//	if (!_nodesById.TryGetValue(rootId, out var token))
		//		return null;

		//	var root = BuildJsonBaselineNode(rootId, token);
		//	await LoadJsonBaselineChildrenAsync(root);
		//	return root;
		//}


		public Task<ICollection<HierarchyIdRecordDetailsDTO>> GetImmediateChildrenHierarchyByGuidAsync(Guid parentId)
		{
			EnsureLoaded();

			var result = new List<HierarchyIdRecordDetailsDTO>();

			if (_childrenByParent.TryGetValue(parentId, out var children))
			{
				foreach (var childId in children)
				{
					if (_hierarchyById.TryGetValue(childId, out var dto))
						result.Add(dto);
				}
			}

			return Task.FromResult<ICollection<HierarchyIdRecordDetailsDTO>>(result);
		}

		public Task<ICollection<NestedHierarchyIdRecordDetailsDTO>> GetAllChildrenHierarchyByGuidAsync(Guid rootId)
		{
			EnsureLoaded();

			var result = new List<NestedHierarchyIdRecordDetailsDTO>();

			if (!_hierarchyById.TryGetValue(rootId, out var rootMeta))
				return Task.FromResult<ICollection<NestedHierarchyIdRecordDetailsDTO>>(result);

			var rootNode = new NestedHierarchyIdRecordDetailsDTO
			{
				RecordId = rootMeta.RecordId,
				HierarchyId = rootMeta.HierarchyId,
				Level = rootMeta.Level,
				RecordType = rootMeta.RecordType,
				Name = rootMeta.Name,
				Children = new List<NestedHierarchyIdRecordDetailsDTO>()
			};

			BuildNestedChildren(rootNode);
			result.Add(rootNode);

			return Task.FromResult<ICollection<NestedHierarchyIdRecordDetailsDTO>>(result);
		}

		private void BuildNestedChildren(NestedHierarchyIdRecordDetailsDTO parent)
		{
			if (!_childrenByParent.TryGetValue(parent.RecordId, out var children))
				return;

			foreach (var childId in children)
			{
				if (!_hierarchyById.TryGetValue(childId, out var meta))
					continue;

				var childNode = new NestedHierarchyIdRecordDetailsDTO
				{
					RecordId = meta.RecordId,
					HierarchyId = meta.HierarchyId,
					Level = meta.Level,
					RecordType = meta.RecordType,
					Name = meta.Name,
					Children = new List<NestedHierarchyIdRecordDetailsDTO>()
				};

				BuildNestedChildren(childNode);
				parent.Children!.Add(childNode);
			}
		}

		#endregion

		#region Record details (LIVE)

		public Task<GetProjectDTO> GetProjectDetailsAsync(Guid projectId)
		{
			EnsureLoaded();
			if (_exportKind != RepositoryExportKind.Live)
				throw new InvalidOperationException("Project details are only available for LIVE exports.");

			if (!_nodeById.TryGetValue(projectId, out var node))
				throw new KeyNotFoundException($"Project with Id {projectId} not found in JSON.");

			var projectToken = node["Project"] ?? node;
			var dto = projectToken.ToObject<GetProjectDTO>() ?? new GetProjectDTO();
			return Task.FromResult(dto);
		}

		public Task<GetItemzTypeDTO> GetItemzTypeDetailsAsync(Guid itemzTypeId)
		{
			EnsureLoaded();
			if (_exportKind != RepositoryExportKind.Live)
				throw new InvalidOperationException("ItemzType details are only available for LIVE exports.");

			if (!_nodeById.TryGetValue(itemzTypeId, out var node))
				throw new KeyNotFoundException($"ItemzType with Id {itemzTypeId} not found in JSON.");

			var token = node["ItemzType"] ?? node;
			var dto = token.ToObject<GetItemzTypeDTO>() ?? new GetItemzTypeDTO();
			return Task.FromResult(dto);
		}

		public Task<GetItemzDTO> GetItemzDetailsAsync(Guid itemzId)
		{
			EnsureLoaded();
			if (_exportKind != RepositoryExportKind.Live)
				throw new InvalidOperationException("Itemz details are only available for LIVE exports.");

			if (!_nodeById.TryGetValue(itemzId, out var node))
				throw new KeyNotFoundException($"Itemz with Id {itemzId} not found in JSON.");

			var token = node["Itemz"] ?? node;
			var dto = token.ToObject<GetItemzDTO>() ?? new GetItemzDTO();
			return Task.FromResult(dto);
		}

		#endregion

		#region Traceability

		/// <summary>
		/// Returns parent (From) and child (To) traces for a given record.
		/// For LIVE exports, this uses ItemzTraces and Itemz nodes.
		/// For BASELINE exports, this uses BaselineItemzTraces and BaselineItemz nodes.
		/// </summary>
		public Task<(ICollection<TraceComponentViewModel> Parents, ICollection<TraceComponentViewModel> Children)>
			GetTracesForRecordAsync(Guid recordId)
		{
			EnsureLoaded();

			var parents = new List<TraceComponentViewModel>();
			var children = new List<TraceComponentViewModel>();

			if (_exportKind == RepositoryExportKind.Live)
			{
				foreach (var trace in _itemzTraces)
				{
					var fromId = trace.Value<string>("FromTraceItemzId");
					var toId = trace.Value<string>("ToTraceItemzId");
					var label = trace.Value<string>("TraceLabel");

					if (!Guid.TryParse(fromId, out var fromGuid) ||
						!Guid.TryParse(toId, out var toGuid))
						continue;

					if (fromGuid == recordId)
					{
						// Child / To trace
						if (_nodeById.TryGetValue(toGuid, out var targetNode))
						{
							var itemToken = targetNode["Itemz"] ?? targetNode;
							var dto = itemToken.ToObject<GetItemzDTO>();
							if (dto != null)
							{
								children.Add(new TraceComponentViewModel
								{
									Id = dto.Id,
									Name = dto.Name,
									Status = dto.Status,
									Priority = dto.Priority,
									TraceLabel = label
								});
							}
						}
					}
					else if (toGuid == recordId)
					{
						// Parent / From trace
						if (_nodeById.TryGetValue(fromGuid, out var sourceNode))
						{
							var itemToken = sourceNode["Itemz"] ?? sourceNode;
							var dto = itemToken.ToObject<GetItemzDTO>();
							if (dto != null)
							{
								parents.Add(new TraceComponentViewModel
								{
									Id = dto.Id,
									Name = dto.Name,
									Status = dto.Status,
									Priority = dto.Priority,
									TraceLabel = label
								});
							}
						}
					}
				}
			}
			else if (_exportKind == RepositoryExportKind.Baseline)
			{
				foreach (var trace in _baselineItemzTraces)
				{
					var fromId = trace.Value<string>("FromTraceBaselineItemzId");
					var toId = trace.Value<string>("ToTraceBaselineItemzId");
					var label = trace.Value<string>("TraceLabel");

					if (!Guid.TryParse(fromId, out var fromGuid) ||
						!Guid.TryParse(toId, out var toGuid))
						continue;

					if (fromGuid == recordId)
					{
						// Child / To baseline trace
						if (_nodeById.TryGetValue(toGuid, out var targetNode))
						{
							var itemToken = targetNode["BaselineItemz"] ?? targetNode;
							// If you have a GetBaselineItemzDTO, map it here.
							var dto = itemToken.ToObject<GetItemzDTO>(); // or GetBaselineItemzDTO
							if (dto != null)
							{
								children.Add(new TraceComponentViewModel
								{
									Id = dto.Id,
									Name = dto.Name,
									Status = dto.Status,
									Priority = dto.Priority,
									TraceLabel = label
								});
							}
						}
					}
					else if (toGuid == recordId)
					{
						// Parent / From baseline trace
						if (_nodeById.TryGetValue(fromGuid, out var sourceNode))
						{
							var itemToken = sourceNode["BaselineItemz"] ?? sourceNode;
							var dto = itemToken.ToObject<GetItemzDTO>(); // or GetBaselineItemzDTO
							if (dto != null)
							{
								parents.Add(new TraceComponentViewModel
								{
									Id = dto.Id,
									Name = dto.Name,
									Status = dto.Status,
									Priority = dto.Priority,
									TraceLabel = label
								});
							}
						}
					}
				}
			}

			return Task.FromResult<(ICollection<TraceComponentViewModel>, ICollection<TraceComponentViewModel>)>((parents, children));
		}

		#endregion

		#region Index building

		private void BuildIndexes()
		{
			_nodeById.Clear();
			_hierarchyById.Clear();
			_parentById.Clear();
			_childrenByParent.Clear();
			_rootIds.Clear();
			_itemzTraces.Clear();
			_baselineItemzTraces.Clear();
			_exportKind = RepositoryExportKind.Unknown;

			if (_root == null)
				return;

			// Detect export kind (LIVE vs BASELINE) based on which root arrays are present.
			var projects = GetPropertyIgnoreCase("Projects") as JArray;
			var itemzTypes = GetPropertyIgnoreCase("ItemzTypes") as JArray;
			var itemzArray = GetPropertyIgnoreCase("Itemz") as JArray;

			var baselines = GetPropertyIgnoreCase("Baselines") as JArray;
			var baselineItemzTypes = GetPropertyIgnoreCase("BaselineItemzTypes") as JArray;
			var baselineItemz = GetPropertyIgnoreCase("BaselineItemz") as JArray;


			if (projects != null || itemzTypes != null || itemzArray != null)
				_exportKind = RepositoryExportKind.Live;
			else if (baselines != null || baselineItemzTypes != null || baselineItemz != null)
				_exportKind = RepositoryExportKind.Baseline;
			else
				_exportKind = RepositoryExportKind.Unknown;

			// LIVE hierarchy
			if (projects != null)
			{
				foreach (var projNode in projects)
					ProcessProjectExportNode(projNode, level: 0, parentId: null);
			}

			if (itemzTypes != null)
			{
				foreach (var itNode in itemzTypes)
					ProcessItemzTypeExportNode(itNode, level: 0, parentId: null, isRoot: true);
			}

			if (itemzArray != null)
			{
				foreach (var itemNode in itemzArray)
					ProcessItemzExportNode(itemNode, level: 0, parentId: null, isRoot: true, recordType: "Itemz");
			}

			// BASELINE hierarchy
			if (baselines != null)
			{
				foreach (var baselineNode in baselines)
					ProcessBaselineExportNode(baselineNode, level: 0, parentId: null);
			}

			if (baselineItemzTypes != null)
			{
				foreach (var bitNode in baselineItemzTypes)
					ProcessBaselineItemzTypeExportNode(bitNode, level: 0, parentId: null, isRoot: true);
			}

			if (baselineItemz != null)
			{
				foreach (var biNode in baselineItemz)
					ProcessBaselineItemzExportNode(biNode, level: 0, parentId: null, isRoot: true, recordType: "BaselineItemz");
			}


			// -----------------------------------------------------------------------------
			// Extract BaselineItemz "isIncluded" values from the JSON hierarchy
			// -----------------------------------------------------------------------------
			//
			// Why this logic exists:
			// ----------------------
			// The JSON export format for BaselineItemz is not uniform. Depending on the
			// structure of the export, a BaselineItemz node may appear as:
			//
			//   1) An OBJECT:
			//        "BaselineItemz": { "Id": "...", "isIncluded": false, ... }
			//
			//   2) An ARRAY containing one or more objects:
			//        "BaselineItemz": [ { "Id": "...", "isIncluded": false }, ... ]
			//
			// Additionally, _nodeById contains *all* nodes in the hierarchy (Baseline,
			// BaselineItemzType, BaselineItemz, SubItemz, etc.). We must therefore:
			//
			//   • Filter only nodes whose RecordType == "BaselineItemz"
			//   • Handle both JObject and JArray shapes safely
			//   • Extract the "isIncluded" flag and store it in a lookup dictionary
			//
			// This dictionary (_baselineItemzIncluded) is later used by the TreeView
			// builder to visually mark excluded BaselineItemz nodes.
			//
			// -----------------------------------------------------------------------------

			_baselineItemzIncluded = new Dictionary<Guid, bool>();

			foreach (var kvp in _nodeById)
			{
				var id = kvp.Key;
				var node = kvp.Value;

				// Only process nodes that are actually BaselineItemz
				if (!_hierarchyById.TryGetValue(id, out var meta))
					continue;

				if (!string.Equals(meta.RecordType, "BaselineItemz", StringComparison.OrdinalIgnoreCase))
					continue;

				// Retrieve the BaselineItemz token from the wrapper node
				var itemToken = node["BaselineItemz"];
				if (itemToken == null)
					continue;

				// -------------------------------------------------------------------------
				// Case 1: BaselineItemz is a single OBJECT
				// -------------------------------------------------------------------------
				if (itemToken is JObject obj)
				{
					var isIncludedProp = obj["isIncluded"];
					if (isIncludedProp != null)
					{
						bool isIncluded = isIncludedProp.Value<bool>();
						_baselineItemzIncluded[id] = isIncluded;
					}
				}

				// -------------------------------------------------------------------------
				// Case 2: BaselineItemz is an ARRAY of objects
				// -------------------------------------------------------------------------
				else if (itemToken is JArray arr)
				{
					foreach (var element in arr.OfType<JObject>())
					{
						var isIncludedProp = element["isIncluded"];
						if (isIncludedProp != null)
						{
							bool isIncluded = isIncludedProp.Value<bool>();
							_baselineItemzIncluded[id] = isIncluded;
						}
					}
				}
			}


			// Traces
			var itemzTraces = GetPropertyIgnoreCase("ItemzTraces") as JArray;
			if (itemzTraces != null)
				_itemzTraces.AddRange(itemzTraces);

			var baselineItemzTraces = GetPropertyIgnoreCase("BaselineItemzTraces") as JArray;
			if (baselineItemzTraces != null)
				_baselineItemzTraces.AddRange(baselineItemzTraces);

		}

		private void RegisterNode(Guid id, JToken wrapperNode, Guid? parentId, int level, string recordType, string name, bool isRoot)
		{
			_nodeById[id] = wrapperNode;
			_parentById[id] = parentId;

			if (parentId.HasValue)
			{
				if (!_childrenByParent.TryGetValue(parentId.Value, out var list))
				{
					list = new List<Guid>();
					_childrenByParent[parentId.Value] = list;
				}
				list.Add(id);
			}

			if (isRoot || !parentId.HasValue)
				_rootIds.Add(id);

			_hierarchyById[id] = new HierarchyIdRecordDetailsDTO
			{
				RecordId = id,
				Name = name,
				RecordType = recordType,
				Level = level,
				ParentRecordId = parentId ?? Guid.Empty
			};
		}

		#region LIVE processors

		private void ProcessProjectExportNode(JToken projNode, int level, Guid? parentId)
		{
			var projectToken = projNode["Project"];
			if (projectToken == null)
				return;

			var id = ParseGuid(projectToken, "Id");
			if (id == Guid.Empty)
				return;

			var name = projectToken.Value<string>("Name") ?? string.Empty;

			RegisterNode(id, projNode, parentId, level, "Project", name, isRoot: parentId == null);

			var itemzTypes = projNode["ItemzTypes"] as JArray;
			if (itemzTypes != null)
			{
				foreach (var itNode in itemzTypes)
					ProcessItemzTypeExportNode(itNode, level + 1, id, isRoot: false);
			}
		}


		private void ProcessItemzTypeExportNode(JToken itNode, int level, Guid? parentId, bool isRoot)
		{
			var itemzTypeToken = itNode["ItemzType"];
			if (itemzTypeToken == null)
				return;

			var id = ParseGuid(itemzTypeToken, "Id");
			if (id == Guid.Empty)
				return;

			var name = itemzTypeToken.Value<string>("Name") ?? string.Empty;

			RegisterNode(id, itNode, parentId, level, "ItemzType", name, isRoot);

			var itemzArray = itNode["Itemz"] as JArray;
			if (itemzArray != null)
			{
				foreach (var itemNode in itemzArray)
					ProcessItemzExportNode(itemNode, level + 1, id, isRoot: false, recordType: "Itemz");
			}
		}

		private void ProcessItemzExportNode(JToken itemNode, int level, Guid? parentId, bool isRoot, string recordType)
		{
			var itemToken = itemNode["Itemz"];
			if (itemToken == null)
				return;

			var id = ParseGuid(itemToken, "Id");
			if (id == Guid.Empty)
				return;

			var name = itemToken.Value<string>("Name") ?? string.Empty;

			RegisterNode(id, itemNode, parentId, level, recordType, name, isRoot);

			var subItemz = itemNode["SubItemz"] as JArray;
			if (subItemz != null)
			{
				foreach (var subNode in subItemz)
					ProcessItemzExportNode(subNode, level + 1, id, isRoot: false, recordType: "Itemz");
			}
		}

		#endregion

		#region BASELINE processors

		private void ProcessBaselineExportNode(JToken baselineNode, int level, Guid? parentId)
		{
			var baselineToken = baselineNode["Baseline"];
			if (baselineToken == null)
				return;

			var id = ParseGuid(baselineToken, "Id");
			if (id == Guid.Empty)
				return;

			var name = baselineToken.Value<string>("Name") ?? string.Empty;

			RegisterNode(id, baselineNode, parentId, level, "Baseline", name, isRoot: parentId == null);

			var baselineItemzTypes = baselineNode["BaselineItemzTypes"] as JArray;
			if (baselineItemzTypes != null)
			{
				foreach (var bitNode in baselineItemzTypes)
					ProcessBaselineItemzTypeExportNode(bitNode, level + 1, id, isRoot: false);
			}
		}


		private void ProcessBaselineItemzTypeExportNode(JToken bitNode, int level, Guid? parentId, bool isRoot)
		{
			var bitToken = bitNode["BaselineItemzType"];
			if (bitToken == null)
				return;

			var id = ParseGuid(bitToken, "Id");
			if (id == Guid.Empty)
				return;

			var name = bitToken.Value<string>("Name") ?? string.Empty;

			RegisterNode(id, bitNode, parentId, level, "BaselineItemzType", name, isRoot);

			var baselineItemz = bitNode["BaselineItemz"] as JArray;
			if (baselineItemz != null)
			{
				foreach (var biNode in baselineItemz)
					ProcessBaselineItemzExportNode(biNode, level + 1, id, isRoot: false, recordType: "BaselineItemz");
			}
		}


		private void ProcessBaselineItemzExportNode(JToken biNode, int level, Guid? parentId, bool isRoot, string recordType)
		{
			var biToken = biNode["BaselineItemz"];
			if (biToken == null)
				return;

			var id = ParseGuid(biToken, "Id");
			if (id == Guid.Empty)
				return;

			var name = biToken.Value<string>("Name") ?? string.Empty;

			RegisterNode(id, biNode, parentId, level, recordType, name, isRoot);

			var baselineSubItemz = biNode["BaselineSubItemz"] as JArray;
			if (baselineSubItemz != null)
			{
				foreach (var subNode in baselineSubItemz)
					ProcessBaselineItemzExportNode(subNode, level + 1, id, isRoot: false, recordType: "BaselineItemz");
			}
		}


		#endregion

		#endregion

		public JsonExportKind DetectExportKind()
		{
			EnsureLoaded();

			// Case-insensitive lookup
			var root = _root!.ToObject<Dictionary<string, object>>(new JsonSerializer
			{
				MissingMemberHandling = MissingMemberHandling.Ignore
			});

			bool HasArray(string key) =>
				_root!.Properties()
					  .Any(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase)
							 && p.Value is JArray arr
							 && arr.Count > 0);

			if (HasArray("Projects"))
				return JsonExportKind.Project;

			if (HasArray("ItemzTypes"))
				return JsonExportKind.ItemzType;

			if (HasArray("Itemz"))
				return JsonExportKind.Itemz;

			if (HasArray("Baselines"))
				return JsonExportKind.Baseline;

			if (HasArray("BaselineItemzTypes"))
				return JsonExportKind.BaselineItemzType;

			if (HasArray("BaselineItemz"))
				return JsonExportKind.BaselineItemz;

			return JsonExportKind.Unknown;
		}

		public Guid GetRootRecordId()
		{
			EnsureLoaded();

			JToken? arr;

			switch (DetectExportKind())
			{
				case JsonExportKind.Project:
					arr = GetPropertyIgnoreCase("Projects");
					return Guid.Parse(arr![0]!["Project"]!["Id"]!.ToString());

				case JsonExportKind.ItemzType:
					arr = GetPropertyIgnoreCase("ItemzTypes");
					return Guid.Parse(arr![0]!["ItemzType"]!["Id"]!.ToString());

				case JsonExportKind.Itemz:
					arr = GetPropertyIgnoreCase("Itemz");
					return Guid.Parse(arr![0]!["Itemz"]!["Id"]!.ToString());

				case JsonExportKind.Baseline:
					arr = GetPropertyIgnoreCase("Baselines");
					return Guid.Parse(arr![0]!["Baseline"]!["Id"]!.ToString());

				case JsonExportKind.BaselineItemzType:
					arr = GetPropertyIgnoreCase("BaselineItemzTypes");
					return Guid.Parse(arr![0]!["BaselineItemzType"]!["Id"]!.ToString());

				case JsonExportKind.BaselineItemz:
					arr = GetPropertyIgnoreCase("BaselineItemz");
					return Guid.Parse(arr![0]!["BaselineItemz"]!["Id"]!.ToString());
			}

			return Guid.Empty;
		}


		private JToken? GetPropertyIgnoreCase(string name)
		{
			return _root!.Properties()
						 .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
						 ?.Value;
		}

		private static Guid ParseGuid(JToken? token, string propertyName)
		{
			var value = token?[propertyName]?.ToString();
			return Guid.TryParse(value, out var guid)
				? guid
				: Guid.Empty;
		}

		#region Record details (BASELINE)

		public Task<GetBaselineDTO> GetBaselineDetailsAsync(Guid baselineId)
		{
			EnsureLoaded();
			if (_exportKind != RepositoryExportKind.Baseline)
				throw new InvalidOperationException("Baseline details are only available for BASELINE exports.");

			if (!_nodeById.TryGetValue(baselineId, out var node))
				throw new KeyNotFoundException($"Baseline with Id {baselineId} not found in JSON.");

			var token = node["Baseline"] ?? node;
			var dto = token.ToObject<GetBaselineDTO>() ?? new GetBaselineDTO();
			return Task.FromResult(dto);
		}

		public Task<GetBaselineItemzTypeDTO> GetBaselineItemzTypeDetailsAsync(Guid baselineItemzTypeId)
		{
			EnsureLoaded();
			if (_exportKind != RepositoryExportKind.Baseline)
				throw new InvalidOperationException("BaselineItemzType details are only available for BASELINE exports.");

			if (!_nodeById.TryGetValue(baselineItemzTypeId, out var node))
				throw new KeyNotFoundException($"BaselineItemzType with Id {baselineItemzTypeId} not found in JSON.");

			var token = node["BaselineItemzType"] ?? node;
			var dto = token.ToObject<GetBaselineItemzTypeDTO>() ?? new GetBaselineItemzTypeDTO();
			return Task.FromResult(dto);
		}

		public Task<GetBaselineItemzDTO> GetBaselineItemzDetailsAsync(Guid baselineItemzId)
		{
			EnsureLoaded();
			if (_exportKind != RepositoryExportKind.Baseline)
				throw new InvalidOperationException("BaselineItemz details are only available for BASELINE exports.");

			if (!_nodeById.TryGetValue(baselineItemzId, out var node))
				throw new KeyNotFoundException($"BaselineItemz with Id {baselineItemzId} not found in JSON.");

			var token = node["BaselineItemz"] ?? node;
			var dto = token.ToObject<GetBaselineItemzDTO>() ?? new GetBaselineItemzDTO();
			return Task.FromResult(dto);
		}

		#endregion


	}
}
