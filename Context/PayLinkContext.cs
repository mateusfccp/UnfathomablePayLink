using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Models;

namespace unfathomable_pay_link.Context;

public class PayLinkContext(DbContextOptions<PayLinkContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Business> Businesses { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Payment>().Property(e => e.state).HasConversion<string>();//Para hacer legible el estado en base de datos
    }
}