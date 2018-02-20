using Common.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class TagConfig : IEntityTypeConfiguration<Tag>
    {
        
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            throw new System.NotImplementedException();
        }
    }
}