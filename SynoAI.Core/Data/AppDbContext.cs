using Microsoft.EntityFrameworkCore;
using SynoAI.Core.Data;

public class AppDbContext : DbContext
{
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<DetectionArea> DetectionAreas { get; set; }
    public DbSet<DetectionPoint> DetectionPoints { get; set; }
    public DbSet<DetectionTimeRange> DetectionTimeRanges { get; set; }
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