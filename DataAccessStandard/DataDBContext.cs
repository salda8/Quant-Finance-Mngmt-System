using System.Collections.Generic;
using Common;
using Common.EntityModels;
using DataAccessStandard.EntityConfigs;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;


namespace DataAccess
{
    public class DataDBContext : DbContext
    {
        public DataDBContext(DbContextOptions options) : base(options)
        {
        }

        //[InjectionConstructor]
        //public DataDBContext()
        //    : base("Name=data")
        //{
        //    this.Configuration.ProxyCreationEnabled = false;
        //}
        //[InjectionConstructor]
        public DataDBContext()
        {
            //this.Configuration.ProxyCreationEnabled = false;
        }

       



        public DbSet<OHLCBar> Data { get; set; }
        public DbSet<StoredDataInfo> StoredDataInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Source = (localdb)\\MSSQLLocalDB; Initial Catalog = data; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = True; ApplicationIntent = ReadWrite; MultiSubnetFailover = False");
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataDBContext, DataDBContextConfiguration>());

            modelBuilder.Entity<OHLCBar>().ToTable("data");
            

            modelBuilder.ApplyConfiguration(new OHLCBarConfig());
            
        }
    }

    public interface IDataDBContext
    {
        DbSet<OHLCBar> Data { get; set; }
        DbSet<StoredDataInfo> StoredDataInfo { get; set; }
    }

    
}