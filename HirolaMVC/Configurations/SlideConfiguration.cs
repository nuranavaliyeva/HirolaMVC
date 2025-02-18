using HirolaMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HirolaMVC.Configurations
{
    public class SlideConfiguration : IEntityTypeConfiguration<Slide>
    {
        public void Configure(EntityTypeBuilder<Slide> builder)
        {
           builder.Property(s => s.Title).IsRequired().HasMaxLength(50);
            builder.Property(s=>s.SubTitle).IsRequired().HasMaxLength(50);
            builder.Property(s=>s.Description).IsRequired().HasMaxLength(50);
            builder.Property(s=>s.Image).IsRequired();
           
        }
    }
}
