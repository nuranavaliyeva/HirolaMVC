using HirolaMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HirolaMVC.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {

            builder.Property(o => o.Count).IsRequired();
            builder.Property(o => o.Price).IsRequired().HasColumnType("decimal(6,2)");
            builder.HasIndex(o => new { o.ProductId, o.AppUserId });
           //builder.HasOne(o => o.Order).WithMany(o=>o.OrderItems).HasForeignKey(o=>o.OrderId).OnDelete(DeleteBehavior.SetNull);
            
           
        }
    }
}
