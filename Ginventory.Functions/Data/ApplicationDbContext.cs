using System;
using Ginventory.Functions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace Ginventory.Functions.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }
        
        public DbSet<GinBrand> GinBrands { get; set; }
        public DbSet<Botanical> Botanicals { get; set; }
        public DbSet<Tonic> Tonics { get; set; }
        public DbSet<Gin> Gins { get; set; }
        public DbSet<TonicPairing> TonicPairings { get; set; }
        public DbSet<BotanicalPairing> BotanicalPairings { get; set; }
    }
    
    
}