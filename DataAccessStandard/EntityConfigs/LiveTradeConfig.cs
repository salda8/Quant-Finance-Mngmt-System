using Microsoft.EntityFrameworkCore;
using Common;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class LiveTradeConfig : IEntityTypeConfiguration<LiveTrade>
    {
        
        public void Configure(EntityTypeBuilder<LiveTrade> builder)
        {
            //builder.HasOne(typeof(Account)).WithMany().HasForeignKey("AccountID").IsRequired().OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(typeof(Instrument)).WithMany().HasForeignKey("InstrumentID").IsRequired().OnDelete(DeleteBehavior.SetNull);
           


        }
    }
}