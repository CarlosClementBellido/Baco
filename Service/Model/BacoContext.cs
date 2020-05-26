using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Service.Model
{
    public partial class BacoContext : DbContext
    {
        public BacoContext()
        {
        }

        public BacoContext(DbContextOptions<BacoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Friends> Friends { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<GroupsRelations> GroupsRelations { get; set; }
        public virtual DbSet<RsschannelSubscriptions> RsschannelSubscriptions { get; set; }
        public virtual DbSet<Rsschannels> Rsschannels { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=dbcarlosclement.database.windows.net;Database=BacoDB;User Id=administrador;Password=#admin123");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Friends>(entity =>
            {
                entity.HasOne(d => d.IdAcceptorNavigation)
                    .WithMany(p => p.FriendsIdAcceptorNavigation)
                    .HasForeignKey(d => d.IdAcceptor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Friends__id_acce__72C60C4A");

                entity.HasOne(d => d.IdPetitionerNavigation)
                    .WithMany(p => p.FriendsIdPetitionerNavigation)
                    .HasForeignKey(d => d.IdPetitioner)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Friends__id_peti__71D1E811");
            });

            modelBuilder.Entity<Groups>(entity =>
            {
                entity.Property(e => e.Name).IsUnicode(false);
            });

            modelBuilder.Entity<GroupsRelations>(entity =>
            {
                entity.HasOne(d => d.IdGroupNavigation)
                    .WithMany(p => p.GroupsRelations)
                    .HasForeignKey(d => d.IdGroup)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GroupsRel__id_gr__17F790F9");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.GroupsRelations)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GroupsRel__id_us__18EBB532");
            });

            modelBuilder.Entity<RsschannelSubscriptions>(entity =>
            {
                entity.HasOne(d => d.IdRsschannelNavigation)
                    .WithMany(p => p.RsschannelSubscriptions)
                    .HasForeignKey(d => d.IdRsschannel)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RSSChanne__id_rs__08B54D69");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.RsschannelSubscriptions)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RSSChanne__id_us__09A971A2");
            });

            modelBuilder.Entity<Rsschannels>(entity =>
            {
                entity.Property(e => e.Name).IsUnicode(false);

                entity.Property(e => e.Rss).IsUnicode(false);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.Mail).IsUnicode(false);

                entity.Property(e => e.Nick).IsUnicode(false);

                entity.Property(e => e.PassHash).IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
