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

    public virtual DbSet<CUTTING_NOTEBOOK> CUTTING_NOTEBOOK { get; set; }

    public virtual DbSet<CUTTING_NOTEBOOK_LOG> CUTTING_NOTEBOOK_LOG { get; set; }

    public virtual DbSet<LEAVE_REQUEST> LEAVE_REQUEST { get; set; }

    public virtual DbSet<LR_STATUS> LR_STATUS { get; set; }

    public virtual DbSet<ORDER> ORDER { get; set; }

    public virtual DbSet<ORDER_REJECT_REASON> ORDER_REJECT_REASON { get; set; }

    public virtual DbSet<O_HISTORY_UPDATE> O_HISTORY_UPDATE { get; set; }

    public virtual DbSet<O_MATERIAL> O_MATERIAL { get; set; }

    public virtual DbSet<O_STATUS> O_STATUS { get; set; }

    public virtual DbSet<O_TEMPLATE> O_TEMPLATE { get; set; }

    public virtual DbSet<PART_WORK_LOG> PART_WORK_LOG { get; set; }

    public virtual DbSet<PRODUCTION> PRODUCTION { get; set; }

    public virtual DbSet<PRODUCTION_REJECT_REASON> PRODUCTION_REJECT_REASON { get; set; }

    public virtual DbSet<P_PART> P_PART { get; set; }

    public virtual DbSet<P_PART_STATUS> P_PART_STATUS { get; set; }

    public virtual DbSet<P_STATUS> P_STATUS { get; set; }

    public virtual DbSet<ROLE> ROLE { get; set; }

    public virtual DbSet<UO_COMMENT> UO_COMMENT { get; set; }

    public virtual DbSet<UP_COMMENT> UP_COMMENT { get; set; }

    public virtual DbSet<USER> USER { get; set; }

    public virtual DbSet<U_STATUS> U_STATUS { get; set; }

    public virtual DbSet<WORKER_ROLE> WORKER_ROLE { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;uid=sa;password=123456;database=GPMS_SYSTEM;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CUTTING_NOTEBOOK>(entity =>
        {
            entity.HasKey(e => e.CP_ID).HasName("PK__CUTTING___7F18CA8897BBAF92");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.CUTTING_NOTEBOOK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__PRODU__787EE5A0");
        });

        modelBuilder.Entity<CUTTING_NOTEBOOK_LOG>(entity =>
        {
            entity.HasKey(e => e.CND_ID).HasName("PK__CUTTING___524390FAF7C1C0F4");

            entity.Property(e => e.DATE_CREATE).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.CP).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__CP_ID__7E37BEF6");

            entity.HasOne(d => d.USER).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__USER___7F2BE32F");
        });

        modelBuilder.Entity<LEAVE_REQUEST>(entity =>
        {
            entity.HasKey(e => e.LR_ID).HasName("PK__LEAVE_RE__266E7F0F6F673054");

            entity.HasOne(d => d.LRS).WithMany(p => p.LEAVE_REQUEST)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__LRS_I__4BAC3F29");

            entity.HasOne(d => d.USER).WithMany(p => p.LEAVE_REQUEST)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__USER___4AB81AF0");
        });

        modelBuilder.Entity<LR_STATUS>(entity =>
        {
            entity.HasKey(e => e.LRS_ID).HasName("PK__LR_STATU__0C9B456A8C155AB7");
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__ORDER__460A94649A9F6BE7");

            entity.Property(e => e.SIZE).IsFixedLength();

            entity.HasOne(d => d.OS).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__OS_ID__5070F446");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__USER_ID__5165187F");
        });

        modelBuilder.Entity<ORDER_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__ORDER_RE__6FCBE6DE95471563");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithOne(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__ORDER__6383C8BA");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__USER___6477ECF3");
        });

        modelBuilder.Entity<O_HISTORY_UPDATE>(entity =>
        {
            entity.HasKey(e => e.OHU_ID).HasName("PK__O_HISTOR__B86AF0FD6D0141F0");

            entity.Property(e => e.CHANGE_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_HISTORY_UPDATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_HISTORY__ORDER__5EBF139D");
        });

        modelBuilder.Entity<O_MATERIAL>(entity =>
        {
            entity.HasKey(e => e.OM_ID).HasName("PK__O_MATERI__AE0FC29BB47E06F6");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_MATERIAL)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_MATERIA__ORDER__571DF1D5");
        });

        modelBuilder.Entity<O_STATUS>(entity =>
        {
            entity.HasKey(e => e.OS_ID).HasName("PK__O_STATUS__85A506ED1568B5C5");
        });

        modelBuilder.Entity<O_TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.OT_ID).HasName("PK__O_TEMPLA__A9E9ACD2178F58AD");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_TEMPLATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_TEMPLAT__ORDER__5441852A");
        });

        modelBuilder.Entity<PART_WORK_LOG>(entity =>
        {
            entity.HasKey(e => e.WL_ID).HasName("PK__PART_WOR__931092C211893F73");

            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.PP).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__PP_ID__0F624AF8");

            entity.HasOne(d => d.USER).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__USER___10566F31");
        });

        modelBuilder.Entity<PRODUCTION>(entity =>
        {
            entity.HasKey(e => e.PRODUCTION_ID).HasName("PK__PRODUCTI__4E709CAA15E73917");

            entity.HasOne(d => d.ORDER).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__ORDER__6B24EA82");

            entity.HasOne(d => d.PM).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PM_ID__6A30C649");

            entity.HasOne(d => d.PS).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PS_ID__6C190EBB");
        });

        modelBuilder.Entity<PRODUCTION_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__PRODUCTI__6FCBE6DE5599EBBC");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PRODUCTION).WithOne(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PRODU__70DDC3D8");

            entity.HasOne(d => d.USER).WithMany(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__USER___71D1E811");
        });

        modelBuilder.Entity<P_PART>(entity =>
        {
            entity.HasKey(e => e.PP_ID).HasName("PK__P_PART__1662724AABEDB60D");

            entity.HasOne(d => d.PPS).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PPS_ID__05D8E0BE");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PRODUCTI__04E4BC85");

            entity.HasOne(d => d.TEAM_LEADER).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__TEAM_LEA__06CD04F7");

            entity.HasMany(d => d.USER).WithMany(p => p.PP)
                .UsingEntity<Dictionary<string, object>>(
                    "P_PART_ASSIGNEE",
                    r => r.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__USER___09A971A2"),
                    l => l.HasOne<P_PART>().WithMany()
                        .HasForeignKey("PP_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__PP_ID__0A9D95DB"),
                    j =>
                    {
                        j.HasKey("PP_ID", "USER_ID").HasName("PK__P_PART_A__F9599CF5F172F896");
                    });
        });

        modelBuilder.Entity<P_PART_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPS_ID).HasName("PK__P_PART_S__D138327A928256A0");
        });

        modelBuilder.Entity<P_STATUS>(entity =>
        {
            entity.HasKey(e => e.PS_ID).HasName("PK__P_STATUS__0119474C6862D9A6");
        });

        modelBuilder.Entity<ROLE>(entity =>
        {
            entity.HasKey(e => e.ROLE_ID).HasName("PK__ROLE__5AC4D22279B07CDD");
        });

        modelBuilder.Entity<UO_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UO_COMME__10D5574C3C68424B");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__FROM___59FA5E80");

            entity.HasOne(d => d.TO_ORDERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__TO_OR__5AEE82B9");
        });

        modelBuilder.Entity<UP_COMMENT>(entity =>
        {
            entity.HasKey(e => e.UPC_ID).HasName("PK__UP_COMME__3248383983E0583F");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UP_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UP_COMMEN__FROM___74AE54BC");

            entity.HasOne(d => d.TO_PRODUCTIONNavigation).WithMany(p => p.UP_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UP_COMMEN__TO_PR__75A278F5");
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity.HasKey(e => e.USER_ID).HasName("PK__USER__F3BEEBFF8ACB1262");

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
                        j.HasKey("USER_ID", "ROLE_ID").HasName("PK__USER_ROL__C612A6DDC3740950");
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
                        j.HasKey("USER_ID", "WR_ID").HasName("PK__USER_WOR__23CCFCE1BEAC033E");
                    });
        });

        modelBuilder.Entity<U_STATUS>(entity =>
        {
            entity.HasKey(e => e.US_ID).HasName("PK__U_STATUS__DE473D8111E4C5DA");
        });

        modelBuilder.Entity<WORKER_ROLE>(entity =>
        {
            entity.HasKey(e => e.WR_ID).HasName("PK__WORKER_R__072171EC526D1874");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
