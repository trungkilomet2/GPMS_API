using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GPMS.INFRASTRUCTURE.DataContext;

public partial class GPMS_SYSTEMContext : DbContext
{
    public GPMS_SYSTEMContext()
    {
    }

    public GPMS_SYSTEMContext(DbContextOptions<GPMS_SYSTEMContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LEAVE_REQUEST> LEAVE_REQUEST { get; set; }

    public virtual DbSet<LR_STATUS> LR_STATUS { get; set; }

    public virtual DbSet<ORDER> ORDER { get; set; }

    public virtual DbSet<O_HISTORY_UPDATE> O_HISTORY_UPDATE { get; set; }

    public virtual DbSet<O_MATERIAL> O_MATERIAL { get; set; }

    public virtual DbSet<O_STATUS> O_STATUS { get; set; }

    public virtual DbSet<O_TEMPLATE> O_TEMPLATE { get; set; }

    public virtual DbSet<ROLE> ROLE { get; set; }

    public virtual DbSet<UO_COMMENT> UO_COMMENT { get; set; }

    public virtual DbSet<USER> USER { get; set; }

    public virtual DbSet<U_STATUS> U_STATUS { get; set; }

    public virtual DbSet<WORKER_ROLE> WORKER_ROLE { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LEAVE_REQUEST>(entity =>
        {
            entity.HasKey(e => e.LR_ID).HasName("PK__LEAVE_RE__266E7F0F4C69B9E6");

            entity.HasOne(d => d.LRS).WithMany(p => p.LEAVE_REQUEST)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__LRS_I__4BAC3F29");

            entity.HasOne(d => d.USER).WithMany(p => p.LEAVE_REQUEST)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__USER___4AB81AF0");
        });

        modelBuilder.Entity<LR_STATUS>(entity =>
        {
            entity.HasKey(e => e.LRS_ID).HasName("PK__LR_STATU__0C9B456ADF304AEC");
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__ORDER__460A94648E88066A");

            entity.Property(e => e.SIZE).IsFixedLength();

            entity.HasOne(d => d.OS).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__OS_ID__5070F446");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__USER_ID__5165187F");
        });

        modelBuilder.Entity<O_HISTORY_UPDATE>(entity =>
        {
            entity.HasKey(e => e.OHU_ID).HasName("PK__O_HISTOR__B86AF0FDBD870C27");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_HISTORY_UPDATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_HISTORY__ORDER__5DCAEF64");
        });

        modelBuilder.Entity<O_MATERIAL>(entity =>
        {
            entity.HasKey(e => e.OM_ID).HasName("PK__O_MATERI__AE0FC29B121EDB12");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_MATERIAL)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_MATERIA__ORDER__571DF1D5");
        });

        modelBuilder.Entity<O_STATUS>(entity =>
        {
            entity.HasKey(e => e.OS_ID).HasName("PK__O_STATUS__85A506ED02FDAE8B");
        });

        modelBuilder.Entity<O_TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.OT_ID).HasName("PK__O_TEMPLA__A9E9ACD2BC70E1DF");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_TEMPLATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_TEMPLAT__ORDER__5441852A");
        });

        modelBuilder.Entity<ROLE>(entity =>
        {
            entity.HasKey(e => e.ROLE_ID).HasName("PK__ROLE__5AC4D222BA47D720");
        });

        modelBuilder.Entity<UO_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UO_COMME__10D5574C3B64084D");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UO_COMMENT).HasConstraintName("FK__UO_COMMEN__FROM___59FA5E80");

            entity.HasOne(d => d.TO_ORDERNavigation).WithMany(p => p.UO_COMMENT).HasConstraintName("FK__UO_COMMEN__TO_OR__5AEE82B9");
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity.HasKey(e => e.USER_ID).HasName("PK__USER__F3BEEBFFFB62AF41");

            entity.HasOne(d => d.US).WithMany(p => p.USER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USER__US_ID__3A81B327");

            entity.HasMany(d => d.ROLE).WithMany(p => p.USER)
                .UsingEntity<Dictionary<string, object>>(
                    "USER_ROLE",
                    r => r.HasOne<ROLE>().WithMany()
                        .HasForeignKey("ROLE_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_ROLE__ROLE___403A8C7D"),
                    l => l.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_ROLE__USER___3F466844"),
                    j =>
                    {
                        j.HasKey("USER_ID", "ROLE_ID").HasName("PK__USER_ROL__C612A6DDE19B3A3F");
                    });

            entity.HasMany(d => d.WR).WithMany(p => p.USER)
                .UsingEntity<Dictionary<string, object>>(
                    "USER_WORKER_ROLE",
                    r => r.HasOne<WORKER_ROLE>().WithMany()
                        .HasForeignKey("WR_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_WORK__WR_ID__45F365D3"),
                    l => l.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_WORK__USER___44FF419A"),
                    j =>
                    {
                        j.HasKey("USER_ID", "WR_ID").HasName("PK__USER_WOR__23CCFCE14B76A170");
                    });
        });

        modelBuilder.Entity<U_STATUS>(entity =>
        {
            entity.HasKey(e => e.US_ID).HasName("PK__U_STATUS__DE473D81033280C3");
        });

        modelBuilder.Entity<WORKER_ROLE>(entity =>
        {
            entity.HasKey(e => e.WR_ID).HasName("PK__WORKER_R__072171EC7C7F4C64");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
