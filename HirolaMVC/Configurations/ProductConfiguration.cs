using HirolaMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HirolaMVC.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p=> p.Name).IsRequired().HasMaxLength(50);
            builder.Property(p=>p.Price).IsRequired().HasColumnType("decimal(6,2)");
            builder.Property(p=>p.Description).IsRequired().HasMaxLength(500);
            builder.Property(p=>p.ProductCode).IsRequired().HasMaxLength(50);
           
        }
    }
}
