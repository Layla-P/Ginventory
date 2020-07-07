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
                .Select(g => new {g.Name, g.Id}).ToArray();

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
                .Select(g => new {g.Name, g.Id}).ToArray();

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
            var botanicalPairingIds = _context
                .BotanicalPairings
                .Where(g => g.GinId == ginId);


            var tonicPairingIds = _context
                .TonicPairings
                .Where(g => g.GinId == ginId);


            var botanicalList = _context
                .Botanicals;
                

            ginpairing.Botanicals = botanicalList
                .Where(botanical =>
                    botanicalPairingIds
                        .Any(p => p.BotanicalId == botanical.Id)
                )
                .Select(b => b.Name)
                .ToList();

            var tonicList = _context
                .Tonics;


            ginpairing.Tonics = tonicList
                .Where(tonic => tonicPairingIds
                    .Any(t => t.TonicId == tonic.Id))
                .Select(t => t.Name).ToList();

            ginpairing.GinName = _context
                .Gins
                .First(g => g.Id == ginId).Name;


            /////-----------------///
            var gins = _context.Gins.AsQueryable();
            var botpair = _context
                    .BotanicalPairings.AsQueryable();
            var tonicpair = _context.TonicPairings.AsQueryable();
            var botanicalsQ = _context.Botanicals.AsQueryable();
            var tonicsQ = _context.Tonics.AsQueryable();
            // from s in db.Student
            //     join scg in db.StudentCourseGroup on s.StudentID equals scg.StudentID
            //     join c in db.Course on scg.CourseID equals c.CourseID
            //     join g in db.Group on scg.GroupID equals g.GroupID
            //     where c.CourseName == "xyz"
            //     select new { s, g } into x
            //     group x by x.s into studentGroups
            //     select new MyStudent {
            //         StudentName = studentGroups.Key.StudentName,
            //         Groups = studentGroups.Select(sg => sg.g.GroupName)
            //     };

            //https://stackoverflow.com/questions/10505595/linq-many-to-many-relationship-how-to-write-a-correct-where-clause
            //https://www.linqpad.net/
            //var test = from gin in gins
            //		   join bp in botpair on gin.Id equals bp.GinId
            //		   join tp in tonicpair on gin.Id equals tp.GinId into table1
            //		   join b in botanicalsQ on bp.Id equals b.Id
            //		   join t in tonicsQ on table1. equals t.Id
            //		   where gin.Id == ginId
            //		   select new { gin, b, t } into x
            //		   group x by x.b into pairings
            //		   select new GinPairingViewModel
            //		   {
            //			   GinName = pairings.,
            //			   Tonics =


            //	 };

            //from s in dc.Students
            //from e in s.StudentCourseEnrollments
            //where e.Course.CourseID == courseID
            //select s;

           

            var botanicalsOut = from bot in botanicalsQ
                         from bp in botpair
                         from gin in gins
                         where bp.BotanicalId == bot.Id && gin.Id == bp.GinId && gin.Id == ginId
                         select bot.Name;

            var tonicsOut = from tonic in tonicsQ
                            from tp in tonicpair
                            from gin in gins
                            where tp.TonicId == tonic.Id && gin.Id == tp.GinId && gin.Id == ginId
                            select tonic.Name;


            ginpairing.GinName =  gins.Where(gin => gin.Id == ginId).First().Name;
            ginpairing.Botanicals = botanicalsOut.ToList();
            ginpairing.Tonics = tonicsOut.ToList();

            


            



            return new JsonResult(ginpairing);
        }
    }
}














