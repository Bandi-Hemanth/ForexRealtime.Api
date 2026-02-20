using Microsoft.EntityFrameworkCore;
using ForexRealtime.Api.Domain;

namespace ForexRealtime.Api.Infrastructure.Persistence;

public class ForexDbContext : DbContext
{
    public ForexDbContext(DbContextOptions<ForexDbContext> options) : base(options) { }

    public DbSet<PriceTick> PriceTicks => Set<PriceTick>();
    public DbSet<SubscriptionAudit> SubscriptionAudits => Set<SubscriptionAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceTick>(e =>
        {
            e.ToTable("price_tick");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.Symbol).HasMaxLength(32).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 6);
            e.Property(x => x.Bid).HasPrecision(18, 6);
            e.Property(x => x.Ask).HasPrecision(18, 6);
            e.Property(x => x.Ts).IsRequired();
            e.HasIndex(x => new { x.Symbol, x.Ts });
        });

        modelBuilder.Entity<SubscriptionAudit>(e =>
        {
            e.ToTable("subscription_audit");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityByDefaultColumn();
            e.Property(x => x.UserId).HasMaxLength(256).IsRequired();
            e.Property(x => x.Symbol).HasMaxLength(32).IsRequired();
            e.Property(x => x.Action).HasMaxLength(32).IsRequired();
            e.Property(x => x.At).IsRequired();
            e.HasIndex(x => new { x.UserId, x.At });
        });
    }
}
