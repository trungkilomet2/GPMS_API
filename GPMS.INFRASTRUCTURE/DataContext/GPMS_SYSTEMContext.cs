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

    public virtual DbSet<ISSUE_STATUS> ISSUE_STATUS { get; set; }

    public virtual DbSet<LEAVE_REQUEST> LEAVE_REQUEST { get; set; }

    public virtual DbSet<LOG_EVENTS> LOG_EVENTS { get; set; }

    public virtual DbSet<LR_STATUS> LR_STATUS { get; set; }

    public virtual DbSet<ORDER> ORDER { get; set; }

    public virtual DbSet<ORDER_REJECT_REASON> ORDER_REJECT_REASON { get; set; }

    public virtual DbSet<O_HISTORY_UPDATE> O_HISTORY_UPDATE { get; set; }

    public virtual DbSet<O_MATERIAL> O_MATERIAL { get; set; }

    public virtual DbSet<O_STATUS> O_STATUS { get; set; }

    public virtual DbSet<O_TEMPLATE> O_TEMPLATE { get; set; }

    public virtual DbSet<PART_WORK_LOG> PART_WORK_LOG { get; set; }

    public virtual DbSet<PRODUCTION> PRODUCTION { get; set; }

    public virtual DbSet<PRODUCTION_ISSUE_LOG> PRODUCTION_ISSUE_LOG { get; set; }

    public virtual DbSet<PRODUCTION_REJECT_REASON> PRODUCTION_REJECT_REASON { get; set; }

    public virtual DbSet<P_PART> P_PART { get; set; }

    public virtual DbSet<P_PART_STATUS> P_PART_STATUS { get; set; }

    public virtual DbSet<P_STATUS> P_STATUS { get; set; }

    public virtual DbSet<ROLE> ROLE { get; set; }

    public virtual DbSet<TEMPLATE> TEMPLATE { get; set; }

    public virtual DbSet<TEMPLATE_STEP> TEMPLATE_STEP { get; set; }

    public virtual DbSet<UO_COMMENT> UO_COMMENT { get; set; }

    public virtual DbSet<UP_COMMENT> UP_COMMENT { get; set; }

    public virtual DbSet<USER> USER { get; set; }

    public virtual DbSet<USER_AUTHORIZE> USER_AUTHORIZE { get; set; }

    public virtual DbSet<U_STATUS> U_STATUS { get; set; }

    public virtual DbSet<WORKER_SKILL> WORKER_SKILL { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CUTTING_NOTEBOOK>(entity =>
        {
            entity.HasKey(e => e.CP_ID).HasName("PK__CUTTING___7F18CA889C8105A3");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.CUTTING_NOTEBOOK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__PRODU__7B5B524B");
        });

        modelBuilder.Entity<CUTTING_NOTEBOOK_LOG>(entity =>
        {
            entity.HasKey(e => e.CND_ID).HasName("PK__CUTTING___524390FAB66265D4");

            entity.Property(e => e.DATE_CREATE).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.CP).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__CP_ID__01142BA1");

            entity.HasOne(d => d.USER).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__USER___02084FDA");
        });

        modelBuilder.Entity<ISSUE_STATUS>(entity =>
        {
            entity.HasKey(e => e.IS_ID).HasName("PK__ISSUE_ST__C738A1B07650EC0D");
        });

        modelBuilder.Entity<LEAVE_REQUEST>(entity =>
        {
            entity.HasKey(e => e.LR_ID).HasName("PK__LEAVE_RE__266E7F0FF77B1C18");

            entity.HasOne(d => d.LRS).WithMany(p => p.LEAVE_REQUEST)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__LRS_I__4CA06362");

            entity.HasOne(d => d.USER).WithMany(p => p.LEAVE_REQUEST)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__USER___4BAC3F29");
        });

        modelBuilder.Entity<LOG_EVENTS>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<LR_STATUS>(entity =>
        {
            entity.HasKey(e => e.LRS_ID).HasName("PK__LR_STATU__0C9B456A17B417F2");
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__ORDER__460A94644FB751A4");

            entity.Property(e => e.CREATE_TIME).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SIZE).IsFixedLength();

            entity.HasOne(d => d.OS).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__OS_ID__52593CB8");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__USER_ID__534D60F1");
        });

        modelBuilder.Entity<ORDER_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__ORDER_RE__6FCBE6DEE0B2AF19");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithOne(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__ORDER__66603565");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__USER___6754599E");
        });

        modelBuilder.Entity<O_HISTORY_UPDATE>(entity =>
        {
            entity.HasKey(e => e.OHU_ID).HasName("PK__O_HISTOR__B86AF0FDC68D8F94");

            entity.Property(e => e.CHANGE_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_HISTORY_UPDATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_HISTORY__ORDER__619B8048");
        });

        modelBuilder.Entity<O_MATERIAL>(entity =>
        {
            entity.HasKey(e => e.OM_ID).HasName("PK__O_MATERI__AE0FC29BD174B97E");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_MATERIAL)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_MATERIA__ORDER__59063A47");
        });

        modelBuilder.Entity<O_STATUS>(entity =>
        {
            entity.HasKey(e => e.OS_ID).HasName("PK__O_STATUS__85A506ED3F141169");
        });

        modelBuilder.Entity<O_TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.OT_ID).HasName("PK__O_TEMPLA__A9E9ACD2B43A8499");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_TEMPLATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_TEMPLAT__ORDER__5629CD9C");
        });

        modelBuilder.Entity<PART_WORK_LOG>(entity =>
        {
            entity.HasKey(e => e.WL_ID).HasName("PK__PART_WOR__931092C2D54D5187");

            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.PP).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__PP_ID__114A936A");

            entity.HasOne(d => d.USER).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__USER___123EB7A3");
        });

        modelBuilder.Entity<PRODUCTION>(entity =>
        {
            entity.HasKey(e => e.PRODUCTION_ID).HasName("PK__PRODUCTI__4E709CAA267EA21F");

            entity.HasOne(d => d.ORDER).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__ORDER__6E01572D");

            entity.HasOne(d => d.PM).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PM_ID__6D0D32F4");

            entity.HasOne(d => d.PS).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PS_ID__6EF57B66");
        });

        modelBuilder.Entity<PRODUCTION_ISSUE_LOG>(entity =>
        {
            entity.HasKey(e => e.ISSUE_ID).HasName("PK__PRODUCTI__E67F509C510782C5");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PRIORITY).HasDefaultValue(2);

            entity.HasOne(d => d.ASSIGNED_TONavigation).WithMany(p => p.PRODUCTION_ISSUE_LOGASSIGNED_TONavigation).HasConstraintName("FK__PRODUCTIO__ASSIG__19DFD96B");

            entity.HasOne(d => d.CREATED_BYNavigation).WithMany(p => p.PRODUCTION_ISSUE_LOGCREATED_BYNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__CREAT__18EBB532");

            entity.HasOne(d => d.IS).WithMany(p => p.PRODUCTION_ISSUE_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__IS_ID__1BC821DD");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.PRODUCTION_ISSUE_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PRODU__1AD3FDA4");
        });

        modelBuilder.Entity<PRODUCTION_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__PRODUCTI__6FCBE6DE260CDCD8");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PRODUCTION).WithOne(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PRODU__73BA3083");

            entity.HasOne(d => d.USER).WithMany(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__USER___74AE54BC");
        });

        modelBuilder.Entity<P_PART>(entity =>
        {
            entity.HasKey(e => e.PP_ID).HasName("PK__P_PART__1662724A32B76BD4");

            entity.HasOne(d => d.PPS).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PPS_ID__08B54D69");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PRODUCTI__07C12930");

            entity.HasMany(d => d.USER).WithMany(p => p.PP)
                .UsingEntity<Dictionary<string, object>>(
                    "P_PART_ASSIGNEE",
                    r => r.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__USER___0B91BA14"),
                    l => l.HasOne<P_PART>().WithMany()
                        .HasForeignKey("PP_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__PP_ID__0C85DE4D"),
                    j =>
                    {
                        j.HasKey("PP_ID", "USER_ID").HasName("PK__P_PART_A__F9599CF55434989E");
                    });
        });

        modelBuilder.Entity<P_PART_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPS_ID).HasName("PK__P_PART_S__D138327A415DB756");
        });

        modelBuilder.Entity<P_STATUS>(entity =>
        {
            entity.HasKey(e => e.PS_ID).HasName("PK__P_STATUS__0119474CEABAAD3A");
        });

        modelBuilder.Entity<ROLE>(entity =>
        {
            entity.HasKey(e => e.ROLE_ID).HasName("PK__ROLE__5AC4D2227035BB42");
        });

        modelBuilder.Entity<TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.TEMPLATE_ID).HasName("PK__TEMPLATE__BACD412FB1C1AA44");
        });

        modelBuilder.Entity<TEMPLATE_STEP>(entity =>
        {
            entity.HasKey(e => e.STEP_ID).HasName("PK__TEMPLATE__A8940374DEDAE0D7");

            entity.HasOne(d => d.TEMPLATE).WithMany(p => p.TEMPLATE_STEP)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TEMPLATE___TEMPL__208CD6FA");
        });

        modelBuilder.Entity<UO_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UO_COMME__10D5574C290372BB");

            entity.Property(e => e.SEND_DATETIME).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__FROM___5CD6CB2B");

            entity.HasOne(d => d.TO_ORDERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__TO_OR__5DCAEF64");
        });

        modelBuilder.Entity<UP_COMMENT>(entity =>
        {
            entity.HasKey(e => e.UPC_ID).HasName("PK__UP_COMME__32483839795B81D7");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UP_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UP_COMMEN__FROM___778AC167");

            entity.HasOne(d => d.TO_PRODUCTIONNavigation).WithMany(p => p.UP_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UP_COMMEN__TO_PR__787EE5A0");
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity.HasKey(e => e.USER_ID).HasName("PK__USER__F3BEEBFF29B69469");

            entity.HasOne(d => d.MANAGER).WithMany(p => p.InverseMANAGER).HasConstraintName("FK__USER__MANAGER_ID__3B75D760");

            entity.HasOne(d => d.US).WithMany(p => p.USER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USER__US_ID__3A81B327");

            entity.HasMany(d => d.ROLE).WithMany(p => p.USER)
                .UsingEntity<Dictionary<string, object>>(
                    "USER_ROLE",
                    r => r.HasOne<ROLE>().WithMany()
                        .HasForeignKey("ROLE_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_ROLE__ROLE___412EB0B6"),
                    l => l.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_ROLE__USER___403A8C7D"),
                    j =>
                    {
                        j.HasKey("USER_ID", "ROLE_ID").HasName("PK__USER_ROL__C612A6DDC4E8E571");
                    });

            entity.HasMany(d => d.WS).WithMany(p => p.USER)
                .UsingEntity<Dictionary<string, object>>(
                    "USER_WORKER_SKILL",
                    r => r.HasOne<WORKER_SKILL>().WithMany()
                        .HasForeignKey("WS_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_WORK__WS_ID__46E78A0C"),
                    l => l.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__USER_WORK__USER___45F365D3"),
                    j =>
                    {
                        j.HasKey("USER_ID", "WS_ID").HasName("PK__USER_WOR__037771FD577B5F6B");
                    });
        });

        modelBuilder.Entity<USER_AUTHORIZE>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<U_STATUS>(entity =>
        {
            entity.HasKey(e => e.US_ID).HasName("PK__U_STATUS__DE473D81BDF9EBD5");
        });

        modelBuilder.Entity<WORKER_SKILL>(entity =>
        {
            entity.HasKey(e => e.WS_ID).HasName("PK__WORKER_S__0C99A02E4E404B23");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
