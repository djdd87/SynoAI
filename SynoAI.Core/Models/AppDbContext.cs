using Microsoft.EntityFrameworkCore;

namespace SynoAI.Core.Models;

public class AppDbContext : DbContext
{
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<ZonePoint> ZonePoints { get; set; }
    public DbSet<ZoneTimeRange> ZoneTimeRanges { get; set; }
    public DbSet<Notifier> Notifiers { get; set; }
    public DbSet<Setting> Settings { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}