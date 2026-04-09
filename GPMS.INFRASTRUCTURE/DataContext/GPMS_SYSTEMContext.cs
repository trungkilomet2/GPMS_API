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

    public virtual DbSet<GUEST_ORDER> GUEST_ORDER { get; set; }

    public virtual DbSet<ISSUE_STATUS> ISSUE_STATUS { get; set; }

    public virtual DbSet<LEAVE_REQUEST> LEAVE_REQUEST { get; set; }

    public virtual DbSet<LOG_EVENTS> LOG_EVENTS { get; set; }

    public virtual DbSet<LR_STATUS> LR_STATUS { get; set; }

    public virtual DbSet<ORDER> ORDER { get; set; }

    public virtual DbSet<ORDER_REJECT_REASON> ORDER_REJECT_REASON { get; set; }

    public virtual DbSet<ORDER_SIZE> ORDER_SIZE { get; set; }

    public virtual DbSet<ORDER_SIZE_STATUS> ORDER_SIZE_STATUS { get; set; }

    public virtual DbSet<O_HISTORY_UPDATE> O_HISTORY_UPDATE { get; set; }

    public virtual DbSet<O_MATERIAL> O_MATERIAL { get; set; }

    public virtual DbSet<O_STATUS> O_STATUS { get; set; }

    public virtual DbSet<O_TEMPLATE> O_TEMPLATE { get; set; }

    public virtual DbSet<PART_WORK_LOG> PART_WORK_LOG { get; set; }

    public virtual DbSet<PRODUCTION> PRODUCTION { get; set; }

    public virtual DbSet<PRODUCTION_ISSUE_LOG> PRODUCTION_ISSUE_LOG { get; set; }

    public virtual DbSet<PRODUCTION_REJECT_REASON> PRODUCTION_REJECT_REASON { get; set; }

    public virtual DbSet<P_PART> P_PART { get; set; }

    public virtual DbSet<P_PART_ORDER_SIZE> P_PART_ORDER_SIZE { get; set; }

    public virtual DbSet<P_PART_ORDER_SIZE_STATUS> P_PART_ORDER_SIZE_STATUS { get; set; }

    public virtual DbSet<P_PART_STATUS> P_PART_STATUS { get; set; }

    public virtual DbSet<P_STATUS> P_STATUS { get; set; }

    public virtual DbSet<ROLE> ROLE { get; set; }

    public virtual DbSet<SIZE> SIZE { get; set; }

    public virtual DbSet<TEMPLATE> TEMPLATE { get; set; }

    public virtual DbSet<TEMPLATE_STEP> TEMPLATE_STEP { get; set; }

    public virtual DbSet<UO_COMMENT> UO_COMMENT { get; set; }

    public virtual DbSet<USER> USER { get; set; }

    public virtual DbSet<USER_AUTHORIZE> USER_AUTHORIZE { get; set; }

    public virtual DbSet<U_STATUS> U_STATUS { get; set; }

    public virtual DbSet<WORKER_SKILL> WORKER_SKILL { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CUTTING_NOTEBOOK>(entity =>
        {
            entity.HasKey(e => e.CP_ID).HasName("PK__CUTTING___7F18CA88562E47AF");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.CUTTING_NOTEBOOK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__PRODU__03F0984C");
        });

        modelBuilder.Entity<CUTTING_NOTEBOOK_LOG>(entity =>
        {
            entity.HasKey(e => e.CND_ID).HasName("PK__CUTTING___524390FA4E5CB32A");

            entity.Property(e => e.DATE_CREATE).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.CP).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__CP_ID__09A971A2");

            entity.HasOne(d => d.USER).WithMany(p => p.CUTTING_NOTEBOOK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__USER___0A9D95DB");
        });

        modelBuilder.Entity<GUEST_ORDER>(entity =>
        {
            entity.HasKey(e => e.GUEST_ID).HasName("PK__GUEST_OR__798B6DF580355766");
        });

        modelBuilder.Entity<ISSUE_STATUS>(entity =>
        {
            entity.HasKey(e => e.IS_ID).HasName("PK__ISSUE_ST__C738A1B037941BE5");
        });

        modelBuilder.Entity<LEAVE_REQUEST>(entity =>
        {
            entity.HasKey(e => e.LR_ID).HasName("PK__LEAVE_RE__266E7F0F92F7429C");

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
            entity.HasKey(e => e.LRS_ID).HasName("PK__LR_STATU__0C9B456A37B6C170");
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__ORDER__460A94649D2ABC10");

            entity.Property(e => e.CREATE_TIME).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.GUEST).WithMany(p => p.ORDER).HasConstraintName("FK__ORDER__GUEST_ID__571DF1D5");

            entity.HasOne(d => d.OS).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__OS_ID__5535A963");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER).HasConstraintName("FK__ORDER__USER_ID__5629CD9C");
        });

        modelBuilder.Entity<ORDER_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__ORDER_RE__6FCBE6DED497F29F");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithOne(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__ORDER__72C60C4A");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_REJ__USER___73BA3083");
        });

        modelBuilder.Entity<ORDER_SIZE>(entity =>
        {
            entity.HasKey(e => e.OD_ID).HasName("PK__ORDER_SI__3D0CE883F1DC144B");

            entity.HasOne(d => d.ORDER).WithMany(p => p.ORDER_SIZE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_SIZ__ORDER__5DCAEF64");

            entity.HasOne(d => d.OSS).WithMany(p => p.ORDER_SIZE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_SIZ__OSS_I__5FB337D6");

            entity.HasOne(d => d.SIZE).WithMany(p => p.ORDER_SIZE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_SIZ__SIZE___5EBF139D");
        });

        modelBuilder.Entity<ORDER_SIZE_STATUS>(entity =>
        {
            entity.HasKey(e => e.OSS_ID).HasName("PK__ORDER_SI__F5B4ECDFBC8176E7");
        });

        modelBuilder.Entity<O_HISTORY_UPDATE>(entity =>
        {
            entity.HasKey(e => e.OHU_ID).HasName("PK__O_HISTOR__B86AF0FD81D7C526");

            entity.Property(e => e.CHANGE_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_HISTORY_UPDATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_HISTORY__ORDER__6E01572D");
        });

        modelBuilder.Entity<O_MATERIAL>(entity =>
        {
            entity.HasKey(e => e.OM_ID).HasName("PK__O_MATERI__AE0FC29B4F22E24E");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_MATERIAL)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_MATERIA__ORDER__656C112C");
        });

        modelBuilder.Entity<O_STATUS>(entity =>
        {
            entity.HasKey(e => e.OS_ID).HasName("PK__O_STATUS__85A506ED46AD9B88");
        });

        modelBuilder.Entity<O_TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.OT_ID).HasName("PK__O_TEMPLA__A9E9ACD2B9AD047B");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_TEMPLATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_TEMPLAT__ORDER__628FA481");
        });

        modelBuilder.Entity<PART_WORK_LOG>(entity =>
        {
            entity.HasKey(e => e.WL_ID).HasName("PK__PART_WOR__931092C216A421E7");

            entity.Property(e => e.IS_PAYMENT).HasDefaultValue(false);
            entity.Property(e => e.IS_READ_ONLY).HasDefaultValue(false);

            entity.HasOne(d => d.PPOS).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__PPOS___1F98B2C1");

            entity.HasOne(d => d.USER).WithMany(p => p.PART_WORK_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PART_WORK__USER___208CD6FA");
        });

        modelBuilder.Entity<PRODUCTION>(entity =>
        {
            entity.HasKey(e => e.PRODUCTION_ID).HasName("PK__PRODUCTI__4E709CAAF1F1F810");

            entity.HasOne(d => d.ORDER).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__ORDER__7A672E12");

            entity.HasOne(d => d.PM).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PM_ID__797309D9");

            entity.HasOne(d => d.PS).WithMany(p => p.PRODUCTION)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PS_ID__7B5B524B");
        });

        modelBuilder.Entity<PRODUCTION_ISSUE_LOG>(entity =>
        {
            entity.HasKey(e => e.ISSUE_ID).HasName("PK__PRODUCTI__E67F509CF8138CC6");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PRIORITY).HasDefaultValue(2);

            entity.HasOne(d => d.ASSIGNED_TONavigation).WithMany(p => p.PRODUCTION_ISSUE_LOGASSIGNED_TONavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__ASSIG__282DF8C2");

            entity.HasOne(d => d.CREATED_BYNavigation).WithMany(p => p.PRODUCTION_ISSUE_LOGCREATED_BYNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__CREAT__2739D489");

            entity.HasOne(d => d.IS).WithMany(p => p.PRODUCTION_ISSUE_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__IS_ID__2A164134");

            entity.HasOne(d => d.PPOS).WithMany(p => p.PRODUCTION_ISSUE_LOG)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PPOS___29221CFB");
        });

        modelBuilder.Entity<PRODUCTION_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__PRODUCTI__6FCBE6DE0BF72667");

            entity.Property(e => e.CREATED_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PRODUCTION).WithOne(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PRODU__00200768");

            entity.HasOne(d => d.USER).WithMany(p => p.PRODUCTION_REJECT_REASON)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__USER___01142BA1");
        });

        modelBuilder.Entity<P_PART>(entity =>
        {
            entity.HasKey(e => e.PP_ID).HasName("PK__P_PART__1662724A5A8CAF38");

            entity.HasOne(d => d.PPS).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PPS_ID__114A936A");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PRODUCTI__10566F31");
        });

        modelBuilder.Entity<P_PART_ORDER_SIZE>(entity =>
        {
            entity.HasKey(e => e.PPOS_ID).HasName("PK__P_PART_O__483DBE4B3BB8E206");

            entity.HasOne(d => d.PPOSS).WithMany(p => p.P_PART_ORDER_SIZE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART_OR__PPOSS__17036CC0");

            entity.HasOne(d => d.PP).WithMany(p => p.P_PART_ORDER_SIZE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART_OR__PP_ID__160F4887");

            entity.HasMany(d => d.USER).WithMany(p => p.PPOS)
                .UsingEntity<Dictionary<string, object>>(
                    "P_PART_ASSIGNEE",
                    r => r.HasOne<USER>().WithMany()
                        .HasForeignKey("USER_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__USER___19DFD96B"),
                    l => l.HasOne<P_PART_ORDER_SIZE>().WithMany()
                        .HasForeignKey("PPOS_ID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__P_PART_AS__PPOS___1AD3FDA4"),
                    j =>
                    {
                        j.HasKey("PPOS_ID", "USER_ID").HasName("PK__P_PART_A__A70650F4F1D2FE85");
                    });
        });

        modelBuilder.Entity<P_PART_ORDER_SIZE_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPOSS_ID).HasName("PK__P_PART_O__DADC95F220DB9FEF");
        });

        modelBuilder.Entity<P_PART_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPS_ID).HasName("PK__P_PART_S__D138327A26F316EB");
        });

        modelBuilder.Entity<P_STATUS>(entity =>
        {
            entity.HasKey(e => e.PS_ID).HasName("PK__P_STATUS__0119474C8778E4BA");
        });

        modelBuilder.Entity<ROLE>(entity =>
        {
            entity.HasKey(e => e.ROLE_ID).HasName("PK__ROLE__5AC4D2224A04DF25");
        });

        modelBuilder.Entity<SIZE>(entity =>
        {
            entity.HasKey(e => e.SIZE_ID).HasName("PK__SIZE__694E1A7D67430B9F");
        });

        modelBuilder.Entity<TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.TEMPLATE_ID).HasName("PK__TEMPLATE__BACD412F4BD911EB");
        });

        modelBuilder.Entity<TEMPLATE_STEP>(entity =>
        {
            entity.HasKey(e => e.STEP_ID).HasName("PK__TEMPLATE__A894037478856088");

            entity.HasOne(d => d.TEMPLATE).WithMany(p => p.TEMPLATE_STEP)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TEMPLATE___TEMPL__2EDAF651");
        });

        modelBuilder.Entity<UO_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UO_COMME__10D5574C9C7DFE97");

            entity.Property(e => e.SEND_DATETIME).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__FROM___693CA210");

            entity.HasOne(d => d.TO_ORDERNavigation).WithMany(p => p.UO_COMMENT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UO_COMMEN__TO_OR__6A30C649");
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity.HasKey(e => e.USER_ID).HasName("PK__USER__F3BEEBFF6454A139");

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
                        j.HasKey("USER_ID", "ROLE_ID").HasName("PK__USER_ROL__C612A6DDA471D031");
                        j.ToTable(tb => tb.HasTrigger("TRG_Prevent_Multiple_Active_Owner_On_InsertRole"));
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
                        j.HasKey("USER_ID", "WS_ID").HasName("PK__USER_WOR__037771FDAFF83996");
                    });
        });

        modelBuilder.Entity<USER_AUTHORIZE>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<U_STATUS>(entity =>
        {
            entity.HasKey(e => e.US_ID).HasName("PK__U_STATUS__DE473D8176744C9B");
        });

        modelBuilder.Entity<WORKER_SKILL>(entity =>
        {
            entity.HasKey(e => e.WS_ID).HasName("PK__WORKER_S__0C99A02EBAC861AD");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
