using Microsoft.EntityFrameworkCore;
using Common;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessStandard.EntityConfigs
{
    public class OHLCBarConfig : IEntityTypeConfiguration<OHLCBar>
    {
        public void Configure(EntityTypeBuilder<OHLCBar> modelBuilder)
        {
            modelBuilder.ToTable("data");

            //modelBuilder.Property(x=>x.DateTimeClose).HasColumnType()HasPrecision(3);
            //modelBuilder.Property(x => x.DateTimeOpen).HasPrecision(3);
            modelBuilder.Property(x => x.Open).HasColumnType("decimal(16, 8)");
            modelBuilder.Property(x => x.High).HasColumnType("decimal(16, 8)");
            modelBuilder.Property(x => x.Low).HasColumnType("decimal(16, 8)");
            modelBuilder.Property(x => x.Close).HasColumnType("decimal(16, 8)");

            modelBuilder.HasKey(x => new { x.InstrumentID, x.Frequency, x.DateTimeClose });
        }
    }

    public class StoredDataInfoConfig : IEntityTypeConfiguration<StoredDataInfoConfig>
    {
        public void Configure(EntityTypeBuilder<StoredDataInfoConfig> modelBuilder)
        {
            modelBuilder.ToTable("instrumentinfo");
            //modelBuilder.HasKey(x => new { x.InstrumentID, x.Frequency });
            //modelBuilder.Property(x => x.EarliestDate).HasPrecision(3);
            //modelBuilder.Entity<StoredDataInfo>().Property(x => x.LatestDate).HasPrecision(3);

        }
    }

}
