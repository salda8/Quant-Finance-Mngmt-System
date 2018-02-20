using Microsoft.EntityFrameworkCore;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class DatasourceConfig : IEntityTypeConfiguration<Datasource>
    {
        public void Configure(EntityTypeBuilder<Datasource> builder)
        {
            
        }
    }
}