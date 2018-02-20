using Microsoft.EntityFrameworkCore;
using Common;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class ExecutionMessageCfg : IEntityTypeConfiguration<ExecutionMessage>
    {
        

        public void Configure(EntityTypeBuilder<ExecutionMessage> builder)
        {
            //builder.HasOne(typeof(Account)).WithMany().HasForeignKey("AccountID").IsRequired().OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(typeof(Instrument)).WithMany().HasForeignKey("InstrumentID").IsRequired().IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    
   
}
