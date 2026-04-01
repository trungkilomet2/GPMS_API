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
            entity.HasKey(e => e.CP_ID).HasName("PK__CUTTING___7F18CA8825A5870C");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.CUTTING_NOTEBOOK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__PRODU__7C4F7684");
        });

        modelBuilder.Entity<CUTTING_NOTEBOOK_LOG>(entity =>
        {
            entity.HasKey(e => e.CND_ID).HasName("PK__CUTTING___524390FAD33C26D9");

            entity.Property(e => e.DATE_CREATE).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.CP).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__CP_ID__02084FDA");

            entity.HasOne(d => d.USER).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__USER___02FC7413");
        });

        modelBuilder.Entity<ISSUE_STATUS>(entity =>
        {
            entity.HasKey(e => e.IS_ID).HasName("PK__ISSUE_ST__C738A1B0CF88C07A");
        });

        modelBuilder.Entity<LEAVE_REQUEST>(entity =>
        {
            entity.HasKey(e => e.LR_ID).HasName("PK__LEAVE_RE__266E7F0FCE09C2FE");

            entity.HasOne(d => d.APPROVED_BYNavigation).WithMany(p => p.LEAVE_REQUESTAPPROVED_BYNavigation).HasConstraintName("FK__LEAVE_REQ__APPRO__4CA06362");

            entity.HasOne(d => d.LRS).WithMany(p => p.LEAVE_REQUEST)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__LRS_I__4D94879B");

            entity.HasOne(d => d.USER).WithMany(p => p.LEAVE_REQUESTUSER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__USER___4BAC3F29");
        });

        modelBuilder.Entity<LOG_EVENTS>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<LR_STATUS>(entity =>
        {
            entity.HasKey(e => e.LRS_ID).HasName("PK__LR_STATU__0C9B456A010DDCD3");
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__ORDER__460A9464F59852AE");

            entity.Property(e => e.CREATE_TIME).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SIZE).IsFixedLength();

            entity.HasOne(d => d.OS).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__OS_ID__534D60F1");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__USER_ID__5441852A");
        });

        modelBuilder.Entity<ORDER_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__ORDER_RE__6FCBE6DE666D8185");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithOne(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__ORDER__6754599E");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__USER___68487DD7");
        });

        modelBuilder.Entity<O_HISTORY_UPDATE>(entity =>
        {
            entity.HasKey(e => e.OHU_ID).HasName("PK__O_HISTOR__B86AF0FDD9DEFDC0");

            entity.Property(e => e.CHANGE_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_HISTORY_UPDATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_HISTORY__ORDER__628FA481");
        });

        modelBuilder.Entity<O_MATERIAL>(entity =>
        {
            entity.HasKey(e => e.OM_ID).HasName("PK__O_MATERI__AE0FC29B3734FB87");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_MATERIAL)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_MATERIA__ORDER__59FA5E80");
        });

        modelBuilder.Entity<O_STATUS>(entity =>
        {
            entity.HasKey(e => e.OS_ID).HasName("PK__O_STATUS__85A506ED05D39B29");
        });

        modelBuilder.Entity<O_TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.OT_ID).HasName("PK__O_TEMPLA__A9E9ACD2269D9CB1");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_TEMPLATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_TEMPLAT__ORDER__571DF1D5");
        });

        modelBuilder.Entity<PART_WORK_LOG>(entity =>
        {
            entity.HasKey(e => e.WL_ID).HasName("PK__PART_WOR__931092C2D68BC210");

            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.PP).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__PP_ID__123EB7A3");

            entity.HasOne(d => d.USER).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__USER___1332DBDC");
        });

        modelBuilder.Entity<PRODUCTION>(entity =>
        {
            entity.HasKey(e => e.PRODUCTION_ID).HasName("PK__PRODUCTI__4E709CAAFC625049");

            entity.HasOne(d => d.ORDER).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__ORDER__6EF57B66");

            entity.HasOne(d => d.PM).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PM_ID__6E01572D");

            entity.HasOne(d => d.PS).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PS_ID__6FE99F9F");
        });

        modelBuilder.Entity<PRODUCTION_ISSUE_LOG>(entity =>
        {
            entity.HasKey(e => e.ISSUE_ID).HasName("PK__PRODUCTI__E67F509C74213295");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PRIORITY).HasDefaultValue(2);

            entity.HasOne(d => d.ASSIGNED_TONavigation).WithMany(p => p.PRODUCTION_ISSUE_LOGASSIGNED_TONavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__ASSIG__1AD3FDA4");

            entity.HasOne(d => d.CREATED_BYNavigation).WithMany(p => p.PRODUCTION_ISSUE_LOGCREATED_BYNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__CREAT__19DFD96B");

            entity.HasOne(d => d.IS).WithMany(p => p.PRODUCTION_ISSUE_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__IS_ID__1CBC4616");

            entity.HasOne(d => d.PART).WithMany(p => p.PRODUCTION_ISSUE_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PART___1BC821DD");
        });

        modelBuilder.Entity<PRODUCTION_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__PRODUCTI__6FCBE6DEEAB6ED4C");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PRODUCTION).WithOne(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PRODU__74AE54BC");

            entity.HasOne(d => d.USER).WithMany(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__USER___75A278F5");
        });

        modelBuilder.Entity<P_PART>(entity =>
        {
            entity.HasKey(e => e.PP_ID).HasName("PK__P_PART__1662724AFCC3294D");

            entity.HasOne(d => d.PPS).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PPS_ID__09A971A2");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PRODUCTI__08B54D69");

            entity.HasMany(d => d.USER).WithMany(p => p.PP)
                .UsingEntity<Dictionary<string, object>>(
                    "P_PART_ASSIGNEE",
                    r => r.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__USER___0C85DE4D"),
                    l => l.HasOne<P_PART>().WithMany()
                        .HasForeignKey("PP_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__PP_ID__0D7A0286"),
                    j =>
                    {
                        j.HasKey("PP_ID", "USER_ID").HasName("PK__P_PART_A__F9599CF5A72E1146");
                    });
        });

        modelBuilder.Entity<P_PART_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPS_ID).HasName("PK__P_PART_S__D138327A49E4C010");
        });

        modelBuilder.Entity<P_STATUS>(entity =>
        {
            entity.HasKey(e => e.PS_ID).HasName("PK__P_STATUS__0119474C4A696CE3");
        });

        modelBuilder.Entity<ROLE>(entity =>
        {
            entity.HasKey(e => e.ROLE_ID).HasName("PK__ROLE__5AC4D222E0D83441");
        });

        modelBuilder.Entity<TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.TEMPLATE_ID).HasName("PK__TEMPLATE__BACD412F28598E69");
        });

        modelBuilder.Entity<TEMPLATE_STEP>(entity =>
        {
            entity.HasKey(e => e.STEP_ID).HasName("PK__TEMPLATE__A89403747AF40FD7");

            entity.HasOne(d => d.TEMPLATE).WithMany(p => p.TEMPLATE_STEP)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TEMPLATE___TEMPL__2180FB33");
        });

        modelBuilder.Entity<UO_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UO_COMME__10D5574C9639E804");

            entity.Property(e => e.SEND_DATETIME).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__FROM___5DCAEF64");

            entity.HasOne(d => d.TO_ORDERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__TO_OR__5EBF139D");
        });

        modelBuilder.Entity<UP_COMMENT>(entity =>
        {
            entity.HasKey(e => e.UPC_ID).HasName("PK__UP_COMME__32483839B4416031");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UP_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UP_COMMEN__FROM___787EE5A0");

            entity.HasOne(d => d.TO_PRODUCTIONNavigation).WithMany(p => p.UP_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UP_COMMEN__TO_PR__797309D9");
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity.HasKey(e => e.USER_ID).HasName("PK__USER__F3BEEBFFE76B09E2");

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
                        j.HasKey("USER_ID", "ROLE_ID").HasName("PK__USER_ROL__C612A6DD02968B66");
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
                        j.HasKey("USER_ID", "WS_ID").HasName("PK__USER_WOR__037771FDA252305B");
                    });
        });

        modelBuilder.Entity<USER_AUTHORIZE>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<U_STATUS>(entity =>
        {
            entity.HasKey(e => e.US_ID).HasName("PK__U_STATUS__DE473D81F79D4B56");
        });

        modelBuilder.Entity<WORKER_SKILL>(entity =>
        {
            entity.HasKey(e => e.WS_ID).HasName("PK__WORKER_S__0C99A02E27EEAAE4");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
