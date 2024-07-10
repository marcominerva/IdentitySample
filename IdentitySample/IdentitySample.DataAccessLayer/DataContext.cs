using System.Reflection;
using IdentitySample.Contracts;
using IdentitySample.DataAccessLayer.Entities;
using IdentitySample.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.DataAccessLayer;

public class DataContext(DbContextOptions<DataContext> options, IUserService userService) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }

    private readonly Guid tenantId = userService.GetTenantId();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => typeof(TenantEntity).IsAssignableFrom(e.Entity.GetType()));

        foreach (var entry in entries.Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            (entry.Entity as TenantEntity).TenantId = tenantId;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(TenantEntity).IsAssignableFrom(t.ClrType)).ToList();

        foreach (var type in entities.Select(t => t.ClrType))
        {
            var methods = SetGlobalQueryFiltersMethod(type);
            foreach (var method in methods)
            {
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static IEnumerable<MethodInfo> SetGlobalQueryFiltersMethod(Type type)
    {
        var result = new List<MethodInfo>();

        if (typeof(TenantEntity).IsAssignableFrom(type))
        {
            result.Add(setQueryFilterOnTenantEntity);
        }

        return result;
    }

    private static readonly MethodInfo setQueryFilterOnTenantEntity = typeof(DataContext)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .Single(t => t.IsGenericMethod && t.Name == nameof(SetQueryFilterOnTenantEntity));

    private void SetQueryFilterOnTenantEntity<T>(ModelBuilder builder) where T : TenantEntity
        => builder.Entity<T>().HasQueryFilter(e => e.TenantId == tenantId);
}
