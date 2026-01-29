// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using AutoMapper;
using ItemzApp.API.Helper;
using ItemzApp.API.Models;
using ItemzApp.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Controllers
{
	[ApiController]
	[Route("api/itemzcollection")] // e.g. http://HOST:PORT/api/itemzcollection
								   //[ProducesResponseType(StatusCodes.Status400BadRequest)]
								   //[ProducesResponseType(StatusCodes.Status406NotAcceptable)]
								   //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
								   //[Produces("application/json")]

	public class ItemzCollectionController : ControllerBase
	{
		private readonly IItemzRepository _itemzRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<ItemzCollectionController> _logger;
		public ItemzCollectionController(IItemzRepository itemzRepository,
			IMapper mapper,
			ILogger<ItemzCollectionController> logger)
		{
			_itemzRepository = itemzRepository ?? throw new ArgumentNullException(nameof(itemzRepository));
			_mapper = mapper ??
				throw new ArgumentNullException(nameof(mapper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		///// <summary>
		///// Gets collection of Itemzs without any Pagination (DEPRECATED: use POST /api/itemzcollection/by-ids)
		///// </summary>
		///// <param name="ids">Array of Itemz Id (in GUID form) for which details has to be returned to the caller</param>
		///// <returns>Collection of Itemz that are requested via Array of Itemz Id</returns>
		///// <response code="200">Collection of Itemzs property details based on Itemz Ids that were passed in as parameter</response>
		///// <response code="500">Bad Request - Itemz Ids should be passed in as parameter</response>
		///// <response code="404">No Itemzs were found based on provided list of Itemz Ids</response>
		///// <remarks>
		///// Sample request (this request will get itemz by Ids) \
		///// GET api/ItemzCollection/(9153a516-d69e-4364-b17e-03b87442e21c,5e76f8e8-d3e7-41db-b084-f64c107c6783) 
		///// </remarks>
		//[Obsolete("This GET endpoint is deprecated. Use POST /api/itemzcollection/by-ids with JSON body of GUIDs instead.", false)]
		//[HttpGet("({ids})", Name = "__GET_Itemz_Collection_By_GUID_IDS__")]
		//[HttpHead("({ids})", Name = "__HEAD_Itemz_Collection_By_GUID_IDS__")]
		//[ProducesResponseType(StatusCodes.Status200OK)]
		//[ProducesResponseType(StatusCodes.Status404NotFound)]
		//[ProducesDefaultResponseType]
		//public async Task<ActionResult<IEnumerable<GetItemzDTO>>> GetItemzCollectionAsync(
		//	[FromRoute]
		//	[ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
		//{
		//	// Emit deprecation warning headers so callers get runtime notice to move to POST
		//	try
		//	{
		//		// RFC-style lightweight deprecation info for clients:
		//		// A Warning header (299 is commonly used for misc warnings)
		//		// A Deprecation header (RFC 8594) set to true, and a Link header pointing to the new resource.
		//		Response.Headers.Remove("Warning");
		//		Response.Headers.Remove("Deprecation");
		//		Response.Headers.Remove("Link");

		//		Response.Headers.Append("Warning", "299 - \"Deprecated API: This GET endpoint is deprecated. Use POST /api/itemzcollection/by-ids with JSON body of GUIDs.\"");
		//		Response.Headers.Append("Deprecation", "true");
		//		Response.Headers.Append("Link", "</api/itemzcollection/by-ids>; rel=\"deprecation\"");
		//	}
		//	catch
		//	{
		//		// ignore header failures (some hosting environments restrict header modifications)
		//	}

		//	if (ids == null)
		//	{
		//		_logger.LogDebug("{FormattedControllerAndActionNames}Get multiple Itemz request cannot be processed as required parameter of IDs is NULL",
		//			ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
		//			);
		//		return BadRequest();
		//	}

		//	var itemzEntities = await _itemzRepository.GetItemzsAsync(ids);

		//	if (ids.Count() != itemzEntities.Count())
		//	{
		//		_logger.LogDebug("{FormattedControllerAndActionNames}One or More Itemz are not found while processing request to get multiple items",
		//			ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
		//			);
		//		return NotFound();
		//	}

		//	var itemzsToReturn = _mapper.Map<IEnumerable<GetItemzDTO>>(itemzEntities);

		//	_logger.LogDebug("{FormattedControllerAndActionNames}Returning response with {NumberOfItemz} number of Itemz to the requestor",
		//		ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
		//		itemzEntities.Count());
		//	return Ok(itemzsToReturn);
		//}


		/// <summary>
		/// Gets collection of Itemzs by receiving list of IDs in JSON body.
		/// Use this endpoint instead of the long GET route when passing many IDs.
		/// </summary>
		/// <param name="ids">Array of Itemz Id (in GUID form) passed in the request body as JSON</param>
		/// <returns>Collection of Itemz that are requested via Array of Itemz Id</returns>
		/// <response code="200">Collection of Itemzs property details based on Itemz Ids that were passed in as parameter</response>
		/// <response code="400">Bad Request - Itemz Ids should be passed in as parameter</response>
		/// <response code="404">No Itemzs were found based on provided list of Itemz Ids</response>
		[HttpPost("by-ids", Name = "__POST_GET_Itemz_Collection_By_GUID_IDS__")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult<IEnumerable<GetItemzDTO>>> GetItemzCollectionByIdsAsync([FromBody] IEnumerable<Guid> ids)
		{
			if (ids == null || !ids.Any())
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}Get multiple Itemz (POST) request cannot be processed as required parameter of IDs is NULL or empty",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return BadRequest();
			}

			var itemzEntities = await _itemzRepository.GetItemzsAsync(ids);

			if (ids.Count() != itemzEntities.Count())
			{
				_logger.LogDebug("{FormattedControllerAndActionNames}One or More Itemz are not found while processing POST request to get multiple items",
					ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext)
					);
				return NotFound();
			}

			var itemzsToReturn = _mapper.Map<IEnumerable<GetItemzDTO>>(itemzEntities);

			_logger.LogDebug("{FormattedControllerAndActionNames}Returning response with {NumberOfItemz} number of Itemz to the requestor (POST by-ids)",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzEntities.Count());
			return Ok(itemzsToReturn);
		}


		/// <summary>
		/// Used for creating new multiple Itemz record in the database
		/// </summary>
		/// <param name="itemzCollection">Array of CreateItemzDTO Used for populating information in the newly created itemzs in the database</param>
		/// <returns>Collection of Newly created Itemzs property details</returns>
		/// <response code="201">Collection of Newly created Itemzs property details</response>
		[HttpPost(Name = "__POST_Create_Itemz_Collection__")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesDefaultResponseType]
		public async Task<ActionResult<IEnumerable<GetItemzDTO>>> CreateItemzCollectionAsync(
			IEnumerable<CreateItemzDTO> itemzCollection)
		{
			var itemzEntities = _mapper.Map<IEnumerable<Entities.Itemz>>(itemzCollection);
			foreach (var itemz in itemzEntities)
			{
				_itemzRepository.AddItemz(itemz);
			}
			await _itemzRepository.SaveAsync();

			var itemzCollectionToReturn = _mapper.Map<IEnumerable<GetItemzDTO>>(itemzEntities);
			var idConvertedToString = string.Join(",", itemzCollectionToReturn.Select(a => a.Id));

			_logger.LogDebug("{FormattedControllerAndActionNames}Created {NumberOfItemzCreated} number of new Itemz",
				ControllerAndActionNames.GetFormattedControllerAndActionNames(ControllerContext),
				itemzCollectionToReturn.Count());
			return CreatedAtRoute("__POST_GET_Itemz_Collection_By_GUID_IDS__",
				new { ids = idConvertedToString }, itemzCollectionToReturn);

		}

		/// <summary>
		/// Get list of supported HTTP Options for the ItemzCollection controller.
		/// </summary>
		/// <returns>Custom response header with key as "Allow" and value as different HTTP options that are allowed</returns>
		/// <response code="200">Custom response header with key as "Allow" and value as different HTTP options that are allowed</response>

		[HttpOptions(Name = "__OPTIONS_Itemz_Collection__")]
		public IActionResult GetItemzCollectionOptions()
		{
			Response.Headers.Add("Allow", "HEAD,OPTIONS,POST");
			return Ok();
		}

	}

}
