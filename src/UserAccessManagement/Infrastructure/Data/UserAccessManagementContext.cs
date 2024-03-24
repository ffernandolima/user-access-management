using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UserAccessManagement.Infrastructure.Data.Extensions;

namespace UserAccessManagement.Infrastructure.Data
{
    public class UserAccessManagementContext : DbContext
    {
        public UserAccessManagementContext(DbContextOptions<UserAccessManagementContext> options)
           : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly<UserAccessManagementContext>();
        }
    }
}
