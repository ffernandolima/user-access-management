using Microsoft.EntityFrameworkCore;
using System;

namespace UserAccessManagement.Infrastructure.Data.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder ApplyConfigurationsFromAssembly<T>(this ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(T).Assembly);

            return modelBuilder;
        }
    }
}
