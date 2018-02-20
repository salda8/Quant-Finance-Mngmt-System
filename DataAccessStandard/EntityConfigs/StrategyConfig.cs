using Microsoft.EntityFrameworkCore;
using Common;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class StrategyConfig : IEntityTypeConfiguration<Strategy>
    {
        
        public void Configure(EntityTypeBuilder<Strategy> builder)
        {
            //builder.HasOne(typeof(Instrument)).WithMany().HasForeignKey("InstrumentID").IsRequired()
            //    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}