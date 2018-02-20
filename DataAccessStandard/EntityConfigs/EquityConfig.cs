using Microsoft.EntityFrameworkCore;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class EquityConfig : IEntityTypeConfiguration<Equity>
    {
        public void Configure(EntityTypeBuilder<Equity> builder)
        {
            
        }
    }

    

    
}