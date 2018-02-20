using System;
using Microsoft.EntityFrameworkCore;
using Common;
using Common.EntityModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class MqServerConfig : IEntityTypeConfiguration<MqServer>
    {
        public void Configure(EntityTypeBuilder<MqServer> builder)
        {
            
        }
    }
}