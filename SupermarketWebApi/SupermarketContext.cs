using Microsoft.EntityFrameworkCore;
using SupermarketWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi
{
    public class SupermarketContext : DbContext
    {
        public SupermarketContext(DbContextOptions options) : base(options) { }

        public DbSet<Supermarket> Supermarkets { get; set; }
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SupermarketStock> SupermarketStocks { get; set; }
    }
}
