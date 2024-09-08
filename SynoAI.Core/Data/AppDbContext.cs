using Microsoft.EntityFrameworkCore;
using SynoAI.Core.Data;

public interface IAppDbContext
{
    DbSet<Camera> Cameras { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<Notifier> Notifiers { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<Zone> Zones { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    
    }
}