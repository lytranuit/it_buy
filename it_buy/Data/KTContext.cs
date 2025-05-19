using Vue.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Vue.Data
{
    public class KTContext : DbContext
    {
        public KTContext(DbContextOptions<KTContext> options) : base(options)
        {
        }


        public DbSet<TBL_DANHMUCPLHANGHOA> TBL_DANHMUCPLHANGHOA { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
        }
    }
}
