namespace UnfathomablePayLink.Context;

using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Models;

public class PayLinkContext(DbContextOptions<PayLinkContext> options) : DbContext(options)
{
    public required DbSet<Payment> Payments { get; set; }
    public required DbSet<Business> Businesses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Business>().HasIndex(b => b.Document).IsUnique();

        modelBuilder.Entity<Payment>(entity =>
	{
	    entity.Property(e => e.state).HasConversion<string>();
	    entity.HasKey(p => p.Id);

	    entity.HasOne(p => p.Business)
		.WithMany()
		.HasForeignKey("BusinessId")
		.IsRequired();
	});
    }
}
