using System.Collections.Generic;
using System.Linq;
using Ginventory.Functions.Data;
using Ginventory.Functions.Models;
using Ginventory.Functions.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Security.Cryptography.X509Certificates;

namespace Ginventory.Functions
{
	public class GetGins
	{
		private readonly ApplicationDbContext _context;

		public GetGins(ApplicationDbContext context)
		{
			_context = context;
		}

		[FunctionName("GetGinBrands")]
		public IActionResult GetGinBrands(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
			HttpRequest req, ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			var ginBrands = _context
				.GinBrands
				.Select(g => new { g.Name, g.Id }).ToArray();

			return new JsonResult(ginBrands);
		}


		[FunctionName("GetGinSubBrands")]
		public IActionResult GetGinSubBrands(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
			HttpRequest req, ILogger log)
		{
			log.LogInformation("Requesting gin sub brands");


			if (!int.TryParse(req.Query["id"], out var id))
			{
				return new BadRequestResult();
			}

			var ginSubBrands = _context
				.Gins
				.Where(g => g.GinBrandId == id)
				.Select(g => new { g.Name, g.Id }).ToArray();

			return new JsonResult(ginSubBrands);
		}

		[FunctionName("GetMixers")]
		public IActionResult GetBotanicalsAndTonics(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
			HttpRequest req, ILogger log)
		{
			log.LogInformation("Requesting Botanicals and tonics");


			if (!int.TryParse(req.Query["id"], out var ginId))
			{
				return new BadRequestResult();
			}

			var ginpairing = new GinPairingViewModel();

			//#region Method Linq
			//var botanicalPairingIds = _context
			//	.BotanicalPairings
			//	.Where(g => g.GinId == ginId)
			//	.ToList();

			//var tonicPairingIds = _context
			//	.TonicPairings
			//	.Where(g => g.GinId == ginId)
			//	.ToList();

			//var botanicalList = _context
			//	.Botanicals
			//	.ToList();

			//ginpairing.Botanicals = botanicalList
			//	.Where(botanical =>
			//		botanicalPairingIds
			//			.Exists(p => p.BotanicalId == botanical.Id)
			//	)
			//	.Select(b => b.Name)
			//	.ToList();

			//var tonicList = _context
			//	.Tonics
			//	.ToList();

			//ginpairing.Tonics = tonicList
			//	.Where(tonic => tonicPairingIds
			//		.Exists(t => t.TonicId == tonic.Id))
			//	.Select(t => t.Name)
			//	.ToList();

			//ginpairing.GinName = _context
			//	.Gins
			//	.First(g => g.Id == ginId).Name;
			//#endregion


			#region Query Linq
			var ginsQ = _context.Gins.AsQueryable();
			var botpairQ = _context.BotanicalPairings.AsQueryable();
			var tonicpairQ = _context.TonicPairings.AsQueryable();
			var botanicalsQ = _context.Botanicals.AsQueryable();
			var tonicsQ = _context.Tonics.AsQueryable();

			var botanicalsOut = from bot in botanicalsQ
								from bp in botpairQ
								from gin in ginsQ
								where bp.BotanicalId == bot.Id && gin.Id == bp.GinId && gin.Id == ginId
								select bot.Name;

			var tonicsOut = from tonic in tonicsQ
							from tp in tonicpairQ
							from gin in ginsQ
							where tp.TonicId == tonic.Id && gin.Id == tp.GinId && gin.Id == ginId
							select tonic.Name;


			ginpairing.GinName = ginsQ.Where(gin => gin.Id == ginId).First().Name;
			ginpairing.Botanicals = botanicalsOut.ToList();
			ginpairing.Tonics = tonicsOut.ToList();
			#endregion







			return new JsonResult(ginpairing);
		}
	}
}
