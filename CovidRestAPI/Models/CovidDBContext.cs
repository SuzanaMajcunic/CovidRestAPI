using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CovidRestAPI.Models
{
    public partial class CovidDBContext : DbContext
    {
        public CovidDBContext(DbContextOptions<CovidDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CovidData> CovidData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CovidData>(entity =>
            {
                entity.HasKey(e => new { e.CountryCode, e.Date });

                entity.ToTable("covidData");

                entity.Property(e => e.CountryCode)
                    .HasColumnName("countryCode")
                    .HasMaxLength(3);

                entity.Property(e => e.Date)
                    .HasColumnName("caseDate")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(100);

                entity.Property(e => e.CityCode)
                    .HasColumnName("cityCode")
                    .HasMaxLength(12);

                entity.Property(e => e.Confirmed).HasColumnName("confirmed");

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasColumnName("country")
                    .HasMaxLength(100);

                entity.Property(e => e.Deaths).HasColumnName("deaths");

                entity.Property(e => e.Lat)
                    .HasColumnName("lat")
                    .HasColumnType("decimal(9, 6)");

                entity.Property(e => e.Lon)
                    .HasColumnName("lon")
                    .HasColumnType("decimal(9, 6)");

                entity.Property(e => e.Province)
                    .HasColumnName("province")
                    .HasMaxLength(100);

                entity.Property(e => e.Recovered).HasColumnName("recovered");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
