using Bank.EventBus.Worker.Models;
using Bank.EventBus.Worker.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Bank.EventBus.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<Collections> Collections => Set<Collections>();
    
    public DbSet<BusOperations> Operations => Set<BusOperations>();
    
    public DbSet<BusCollectionsOperations> BusCollectionsOperations => Set<BusCollectionsOperations>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusCollectionsOperations>()
            .HasOne(c => c.Collection)
            .WithMany(c => c.Operations)
            .HasForeignKey(c => c.CollectionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<BusCollectionsOperations>()
            .HasOne(o => o.Operation)
            .WithMany(c => c.Collections)
            .HasForeignKey(c => c.BusOperationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}