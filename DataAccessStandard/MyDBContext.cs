using System;
using Common.EntityModels;
using Common.Interfaces;
using DataAccess.EntityConfigs;

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using CommonStandard.Interfaces;
using Microsoft.EntityFrameworkCore;
using JetBrains.Annotations;

namespace DataAccess
{
    public partial class MyDBContext : DbContext, IMyDbContext
    {


        ////if you want to add migration you need to specify connection string either uncomment this and comment comment constructor on upper.
        ////Or use:add-migration RenamedFewProperties -connectionstring "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=server;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False" -config MyDbContextConfiguration -connectionprovidername "System.Data.SqlClient"
        //public MyDBContext()
        //    : base("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=server;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
        //{
        //    this.Configuration.ProxyCreationEnabled = false;
        //}

        //public MyDBContext(string connectionString)
        //{
        //    Database.Connection.ConnectionString = connectionString;
        //    this.Configuration.ProxyCreationEnabled = false;
        //}

        public MyDBContext()
        {
            //DbContext = this;
        }

        public MyDBContext(DbContextOptions options) : base(options)
        {

            
            //DbContext = this;
        }

        public DbContext DbContext { get; set; }



        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<Datasource> Datasources { get; set; }
        public DbSet<SessionTemplate> SessionTemplates { get; set; }

        public DbSet<InstrumentSession> InstrumentSessions { get; set; }
        public DbSet<TemplateSession> TemplateSessions { get; set; }

        public DbSet<LiveTrade> LiveTrade { get; set; }
        public DbSet<TradeHistory> TradeHistory { get; set; }

        public DbSet<Account> Account { get; set; }
        public DbSet<OpenOrder> OpenOrder { get; set; }
        public DbSet<Strategy> Strategy { get; set; }
        public DbSet<Equity> Equity { get; set; }
        public DbSet<CommissionMessage> CommissionMessage { get; set; }
        public DbSet<ExecutionMessage> ExecutionMessage { get; set; }
        public DbSet<OrderStatusMessage> OrderStatusMessage { get; set; }
        public DbSet<AccountSummary> AccountSummary { get; set; }
        public DbSet<ExpirationRule> ExpirationRule { get; set; }
        public DbSet<MqServer> MqServers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = data1; Integrated Security = True; Connect Timeout = 30; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            
        }



        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            //modelBuilder.Model.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<ExecutionMessage>().HasOne(x => x.Account).WithMany().HasForeignKey(x=>x.AccountID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ExecutionMessage>().HasOne(x => x.Instrument).WithMany().HasForeignKey(x=>x.InstrumentID)
               .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.ApplyConfiguration(new InstrumentConfig());
            

            modelBuilder.ApplyConfiguration(new DatasourceConfig());

            // modelBuilder.Entity<InstrumentSession>().ToTable("instrumentsession");
            // modelBuilder.Entity<TemplateSession>().ToTable("templatesession");

            //modelBuilder.Entity<Instrument>()
            //.HasMany(c => c.Tags)
            //.WithMany()
            //.Map(x =>
            //{
            //    x.MapLeftKey("InstrumentID");
            //    x.MapRightKey("TagID");
            //    x.ToTable("tag_map");
            //});

            
            //Model.GetEntityTypes().Where(x => x.DefiningEntityType.ClrType == typeof(SessionTemplateConfig))
            //    .SelectMany(x => x.GetProperties()).Where(x => x.ClrType == typeof(DateTime));

          
            //modelBuilder.Entity<InstrumentSession>().Property(x => x.OpeningTime).HasPrecision(0);
            //modelBuilder.Entity<InstrumentSession>().Property(x => x.ClosingTime).HasPrecision(0);

            //modelBuilder.Entity<TemplateSession>().Property(x => x.OpeningTime).HasPrecision(0);
            //modelBuilder.Entity<TemplateSession>().Property(x => x.ClosingTime).HasPrecision(0);

            // Instrument

            modelBuilder.ApplyConfiguration(new EquityConfig());
            modelBuilder.Entity<LiveTrade>().HasOne(x => x.Instrument).WithMany().HasForeignKey(x => x.InstrumentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LiveTrade>().HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountID)
                .OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.ApplyConfiguration(new LiveTradeConfig());
            modelBuilder.ApplyConfiguration(new TradeHistoryConfig());
            modelBuilder.Entity<TradeHistory>().HasOne(x => x.Instrument).WithMany().HasForeignKey(x => x.InstrumentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TradeHistory>().HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.ApplyConfiguration(new OpenOrderConfig());
            modelBuilder.Entity<OpenOrder>().HasOne(x => x.Instrument).WithMany().HasForeignKey(x => x.InstrumentID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OpenOrder>().HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.ApplyConfiguration(new StrategyConfig());
            modelBuilder.Entity<Strategy>().HasOne(x=>x.Instrument).WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.ApplyConfiguration(new AccountSummaryConfig());
            modelBuilder.Entity<AccountSummary>().HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.ApplyConfiguration(new ExecutionMessageCfg());
            modelBuilder.ApplyConfiguration(new MqServerConfig());





            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<MyDBContext, MyDbContextConfiguration>());
        }


        public void SetEntryState(object entity, EntityState entityState)
        {
            Entry(entity).State = entityState;
        }

        public void UpdateEntryValues(IEntity entity, IEntity newValues)
        {
            var entityEntry = Entry(entity);
            var id = (int)entityEntry.OriginalValues["ID"];
            newValues.ID = id;
            entityEntry.CurrentValues.SetValues(newValues);

        }








    }
}