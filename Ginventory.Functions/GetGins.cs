using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ginventory.Functions.Data;
using Ginventory.Functions.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            var ginBrands = _context.GinBrands.Select(g => new {g.Name, g.Id }).ToArray();
 
            return new JsonResult(ginBrands);
        }
        
        
        [FunctionName("GetGinSubBrands")]
        public IActionResult GetGinSubBrands(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            if (!int.TryParse(req.Query["id"], out var id))
            {
                return null;
            }
          
            var ginSubBrands = _context
                .Gins
                .Where(g=>g.GinBrandId == id)
                .Select(g => g.Name).ToArray();
 
            return new JsonResult(ginSubBrands);
        }
        
    }
}