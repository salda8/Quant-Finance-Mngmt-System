using Common.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.EntityConfigs
{
    public class SessionTemplateConfig : IEntityTypeConfiguration<TemplateSession>
    {
        //public SessionTemplateConfig()
        //{
        //    this.HasMany(x => x.Sessions)
        //    .WithRequired()
        //    .HasForeignKey(x => x.TemplateID)
        //    .WillCascadeOnDelete(true);
        //}

        //public void Configure(EntityTypeBuilder<SessionTemplate> builder)
        //{
        //    builder.HasMany(x=>x.Sessions).WithOne(x=>x.Template).HasForeignKey()
        //}

        public void Configure(EntityTypeBuilder<TemplateSession> builder)
        {
            builder.HasOne(typeof(SessionTemplate)).WithMany().HasForeignKey("TemplateID").IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
           

        }
    }
}