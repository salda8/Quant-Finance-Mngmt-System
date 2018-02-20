
using Microsoft.EntityFrameworkCore;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class AccountSummaryConfig : IEntityTypeConfiguration<AccountSummary>
    {
        

        public void Configure(EntityTypeBuilder<AccountSummary> builder)
        {
            builder.HasOne(typeof(Account)).WithOne().HasForeignKey(typeof(AccountSummary),"AccountID").IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
