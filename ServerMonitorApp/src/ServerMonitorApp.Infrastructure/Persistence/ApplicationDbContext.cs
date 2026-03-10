using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Infrastructure.Persistence
{
    public partial class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Alert> Alerts { get; set; }

        public virtual DbSet<Device> Devices { get; set; }

        public virtual DbSet<Room> Rooms { get; set; }

        public virtual DbSet<SensorData> SensorDatas { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserRoomAccess> UserRoomAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("pgcrypto");

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("alerts_pkey");

                entity.ToTable("alerts");

                entity.HasIndex(e => new { e.RoomId, e.IsResolved }, "idx_alerts_room_resolved");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_at");
                entity.Property(e => e.DeviceId).HasColumnName("device_id");
                entity.Property(e => e.IsResolved)
                    .HasDefaultValue(false)
                    .HasColumnName("is_resolved");
                entity.Property(e => e.Message).HasColumnName("message");
                entity.Property(e => e.RoomId).HasColumnName("room_id");
                entity.Property(e => e.SensorDataId).HasColumnName("sensor_data_id");
                entity.Property(e => e.Severity)
                    .HasMaxLength(20)
                    .HasColumnName("severity");

                entity.HasOne(d => d.Device).WithMany(p => p.Alerts)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("alerts_device_id_fkey");

                entity.HasOne(d => d.Room).WithMany(p => p.Alerts)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("alerts_room_id_fkey");
            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("devices_pkey");

                entity.ToTable("devices");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()")
                    .HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_at");
                entity.Property(e => e.CriticalTemp)
                    .HasPrecision(5, 2)
                    .HasColumnName("critical_temp");
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true)
                    .HasColumnName("is_active");
                entity.Property(e => e.LastSeen)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("last_seen");
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
                entity.Property(e => e.RoomId).HasColumnName("room_id");
                entity.Property(e => e.WarningTemp)
                    .HasPrecision(5, 2)
                    .HasColumnName("warning_temp");

                entity.HasOne(d => d.Room).WithMany(p => p.Devices)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("devices_room_id_fkey");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("rooms_pkey");

                entity.ToTable("rooms");

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()")
                    .HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_at");
                entity.Property(e => e.Location)
                    .HasMaxLength(255)
                    .HasColumnName("location");
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<SensorData>(entity =>
            {
                entity.ToTable("sensor_data");

                entity.HasKey(e => new { e.Id, e.Timestamp }).HasName("sensor_data_pkey1");

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");
                entity.Property(e => e.Temperature)
                    .HasPrecision(5, 2)
                    .HasColumnName("temperature");
                entity.Property(e => e.Humidity)
                    .HasPrecision(5, 2)
                    .HasColumnName("humidity");
                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("timestamp");
                entity.HasOne(d => d.Device)
                    .WithMany(d => d.SensorDatas)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_sensor_device");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("users_pkey");

                entity.ToTable("users");

                entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

                entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()")
                    .HasColumnName("id");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("created_at");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .HasColumnName("password_hash");
                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .HasColumnName("role");
                entity.Property(e => e.Username)
                    .HasMaxLength(50)
                    .HasColumnName("username");
                entity.Property(e => e.RefreshToken)
                    .HasColumnName("refresh_token");
                entity.Property(e => e.RefreshTokenExpiryTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("refresh_token_expiry_time");
            });

            modelBuilder.Entity<UserRoomAccess>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoomId }).HasName("user_room_access_pkey");

                entity.ToTable("user_room_access");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.RoomId).HasColumnName("room_id");
                entity.Property(e => e.ReceiveAlerts)
                    .HasDefaultValue(true)
                    .HasColumnName("receive_alerts");

                entity.HasOne(d => d.Room).WithMany(p => p.UserRoomAccesses)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("user_room_access_room_id_fkey");

                entity.HasOne(d => d.User).WithMany(p => p.UserRoomAccesses)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_room_access_user_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
