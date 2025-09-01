using Microsoft.EntityFrameworkCore;
using SystemMonitor.Shared.Models;

namespace SystemMonitor.DataAccess.Context;

public class SystemMonitorContext : DbContext
{
    public SystemMonitorContext(DbContextOptions<SystemMonitorContext> options) : base(options) { }

    public DbSet<SystemMetrics> SystemMetrics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.MachineName).HasMaxLength(100);

            entity.HasIndex(e => e.Timestamp);
        });
    }
}