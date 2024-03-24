using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccessManagement.Domain;

namespace UserAccessManagement.Infrastructure.Data.Mappings
{
    public class BenefitsEnrollmentMap : IEntityTypeConfiguration<BenefitsEnrollment>
    {
        public void Configure(EntityTypeBuilder<BenefitsEnrollment> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Id)
                   .IsRequired();

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(320);

            builder.Property(x => x.FullName)
                   .HasMaxLength(300);

            builder.Property(x => x.Country)
                   .IsRequired()
                   .HasMaxLength(2);

            builder.Property(x => x.BirthDate);

            builder.Property(x => x.Salary)
                .HasPrecision(18, 2);

            // Table & Column Mappings
            builder.ToTable("BenefitsEnrollments");
            builder.Property(x => x.Id).HasColumnName("Id");
            builder.Property(x => x.Email).HasColumnName("Email");
            builder.Property(x => x.FullName).HasColumnName("FullName");
            builder.Property(x => x.Country).HasColumnName("Country");
            builder.Property(x => x.BirthDate).HasColumnName("BirthDate");
            builder.Property(x => x.Salary).HasColumnName("Salary");

            // Relationships
        }
    }
}
