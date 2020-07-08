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


			#region lambda syntax Multiple DB Calls and unneeded Data pulled into memory.

			var botanicalPairingIds = _context
				.BotanicalPairings
				.Where(g => g.GinId == ginId)
				.ToList();

			//Pulls the entire Botanicals table into memory regardless that we only need the Botanicals linked to the Gin we selected. 
			var botanicalList = _context
				.Botanicals
				.ToList();

			ginpairing.Botanicals = botanicalList
				.Where(botanical =>
					botanicalPairingIds
						.Exists(p => p.BotanicalId == botanical.Id)
				)
				.Select(b => b.Name)
				.ToList();

			var tonicPairingIds = _context
				.TonicPairings
				.Where(g => g.GinId == ginId)
				.ToList();

			//Pulls the entire Tonics table into memory regardless that we only need the Tonics linked to the Gin we selected.
			var tonicList = _context
				.Tonics
				.ToList();

			ginpairing.Tonics = tonicList
				.Where(tonic => tonicPairingIds
					.Exists(t => t.TonicId == tonic.Id))
				.Select(t => t.Name)
				.ToList();

			ginpairing.GinName = _context
				.Gins
				.First(g => g.Id == ginId).Name;

			//This lambda  syntax above produces 5 calls to the DB with unneeded data being returned and going into memory before being proccessed and garbage collected.
			#endregion


			//#region lambda  syntax 2 Minimising DB calls and the amount of data retrieved from the DB.

			////These first 2 variables setup IQueryables which defer running against the DB until evaluation is called.
			//var botanicalPairingIds = _context
			//	.BotanicalPairings
			//	.Where(g => g.GinId == ginId);

			//var botanicalList = _context
			//	.Botanicals.AsQueryable();

			////Evaluation is called when instantiating to the List required.
			//ginpairing.Botanicals = botanicalList
			//	.Where(botanical =>
			//		botanicalPairingIds
			//			.Any(p => p.BotanicalId == botanical.Id)
			//	)
			//	.Select(b => b.Name)
			//	.ToList();


			////These next 2 variables setup IQueryables wich allow for defured running against the DB.
			//var tonicPairingIds = _context
			//	.TonicPairings
			//	.Where(g => g.GinId == ginId);


			//var tonicList = _context
			//	.Tonics.AsQueryable();


			////Both these variables call evaluation against the DB.
			//ginpairing.Tonics = tonicList
			//	.Where(tonic => tonicPairingIds
			//		.Any(t => t.TonicId == tonic.Id))
			//	.Select(t => t.Name)
			//	.ToList();

			//ginpairing.GinName = _context
			//	.Gins
			//	.First(g => g.Id == ginId).Name;

			////This lambda  syntax above produces 3 calls to the DB with minimal returned data going into memory as the IQueryables allow for EF to generate a more efficient SQL call to the DB.
			//#endregion


			//#region Query Linq
			//var ginsQ = _context.Gins.AsQueryable();
			//var botpairQ = _context.BotanicalPairings.AsQueryable();
			//var tonicpairQ = _context.TonicPairings.AsQueryable();
			//var botanicalsQ = _context.Botanicals.AsQueryable();
			//var tonicsQ = _context.Tonics.AsQueryable();

			////The IQueryables returned from the bellow Variabels do not get run against the DB but mearly pre-arange the SQL query to be run.
			//var botanicalsOut = from bot in botanicalsQ
			//					from bp in botpairQ
			//					from gin in ginsQ
			//					where bp.BotanicalId == bot.Id && gin.Id == bp.GinId && gin.Id == ginId
			//					select bot.Name;

			//var tonicsOut = from tonic in tonicsQ
			//				from tp in tonicpairQ
			//				from gin in ginsQ
			//				where tp.TonicId == tonic.Id && gin.Id == tp.GinId && gin.Id == ginId
			//				select tonic.Name;

			//var ginOut = from gin in ginsQ
			//			 where (gin.Id == ginId)
			//			 select (gin);

			////The only times a query is run against the DB is when the IQuerable is evaluated as in the ToList() called bellow. 
			//ginpairing.GinName = ginOut.First().Name;
			//ginpairing.Botanicals = botanicalsOut.ToList();
			//ginpairing.Tonics = tonicsOut.ToList();


			////This Query synatx produces 3 calls to the DB with minimal returned data going into memory and is the functional equivalent of the lambda syntax 2 above.
			//#endregion

			return new JsonResult(ginpairing);
		}
	}
}
