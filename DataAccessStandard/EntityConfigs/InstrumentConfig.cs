using Common.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class InstrumentConfig : IEntityTypeConfiguration<Instrument>
    {
        
        public void Configure(EntityTypeBuilder<Instrument> builder)
        {
            //builder.HasOne(typeof(Exchange)).WithMany().HasForeignKey("ExchangeID").IsRequired().OnDelete(DeleteBehavior.SetNull);
            //builder.HasOne(typeof(Datasource)).WithMany().HasForeignKey("DataSourceID").IsRequired()
            //    .OnDelete(DeleteBehavior.SetNull);
            //builder.HasMany(typeof(InstrumentSession)).
            //builder.HasOne(typeof(ExpirationRule)).WithMany().HasForeignKey("ExpirationRuleID").IsRequired()
            //    .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.Expiration).IsUnique();
            builder.HasIndex(x => x.ExchangeID).IsUnique();
            //builder.HasIndex(x => x.DatasourceID).IsUnique();
            builder.HasIndex(x => x.Type).IsUnique();
            builder.HasIndex(x => x.Symbol).IsUnique();
        }
    }

    //public class InstrumentInstrumentSessionConfig : IEntityTypeConfiguration<InstrumentInstrumentSession>
    //{
    //    public void Configure(EntityTypeBuilder<InstrumentInstrumentSession> builder)
    //    {
    //        builder.HasKey(x => new {x.InstrumentID, x.InstrumentSessionID});

    //    }
    //}
}