namespace UnfathomablePayLink.Context;

using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Models;

/// <summary>
/// Entity Framework Core database context for the PayLink payment gateway.
/// </summary>
/// <param name="options">The options used to configure this context.</param>
public class PayLinkContext(DbContextOptions<PayLinkContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="Payment"/> entities.
    /// </summary>
    public required DbSet<Payment> Payments { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="Business"/> entities.
    /// </summary>
    public required DbSet<Business> Businesses { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Business>().HasIndex(b => b.Document).IsUnique();

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.State).HasConversion<string>();
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Business)
                .WithMany()
                .HasForeignKey("BusinessId")
                .IsRequired();
        });
    }
}
