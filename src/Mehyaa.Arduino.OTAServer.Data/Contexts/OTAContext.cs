using Mehyaa.Arduino.OTAServer.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mehyaa.Arduino.OTAServer.Data.Contexts
{
    public class OTAContext : DbContext
    {
        public OTAContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            #region Device

            modelBuilder.Entity<Device>(b =>
            {
                b.ToTable("Devices");

                b.Property(e => e.MacAddress)
                    .HasMaxLength(17)
                    .IsRequired();

                b.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            #endregion
            
            #region Device

            modelBuilder.Entity<Firmware>(b =>
            {
                b.ToTable("Firmwares");

                b.Property(e => e.Path)
                    .IsRequired();

                b.Property(e => e.Filename)
                    .HasMaxLength(50)
                    .IsRequired();

                b.Property(e => e.Hash)
                    .HasMaxLength(100)
                    .IsRequired();

                b.HasOne(e => e.Device)
                    .WithMany(e => e.Firmwares)
                    .HasForeignKey(e => e.DeviceId)
                    .IsRequired();
            });

            #endregion

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}