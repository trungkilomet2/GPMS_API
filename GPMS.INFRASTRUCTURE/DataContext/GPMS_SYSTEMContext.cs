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

    public virtual DbSet<DELIVERY> DELIVERY { get; set; }

    public virtual DbSet<D_STATUS> D_STATUS { get; set; }

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
            entity.HasKey(e => e.CP_ID).HasName("PK__CUTTING___7F18CA886AAEF8EB");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.CUTTING_NOTEBOOK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CUTTING_N__PRODU__03F0984C");
        });

        modelBuilder.Entity<CUTTING_NOTEBOOK_LOG>(entity =>
        {
            entity.HasKey(e => e.CND_ID).HasName("PK__CUTTING___524390FAB40BF510");

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

        modelBuilder.Entity<DELIVERY>(entity =>
        {
            entity.HasKey(e => e.DELIVERY_ID).HasName("PK__DELIVERY__7D75E88BDB361D20");

            entity.Property(e => e.SHIPPED_DATE).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.DS).WithMany(p => p.DELIVERY)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DELIVERY__DS_ID__37703C52");

            entity.HasOne(d => d.ORDER_SIZE).WithMany(p => p.DELIVERY)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DELIVERY__ORDER___367C1819");
        });

        modelBuilder.Entity<D_STATUS>(entity =>
        {
            entity.HasKey(e => e.DS_ID).HasName("PK__D_STATUS__EFC56BC6D69115C7");
        });

        modelBuilder.Entity<GUEST_ORDER>(entity =>
        {
            entity.HasKey(e => e.GUEST_ID).HasName("PK__GUEST_OR__798B6DF59F9667DA");
        });

        modelBuilder.Entity<ISSUE_STATUS>(entity =>
        {
            entity.HasKey(e => e.IS_ID).HasName("PK__ISSUE_ST__C738A1B0608A4623");
        });

        modelBuilder.Entity<LEAVE_REQUEST>(entity =>
        {
            entity.HasKey(e => e.LR_ID).HasName("PK__LEAVE_RE__266E7F0F0ACD31D5");

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
            entity.HasKey(e => e.LRS_ID).HasName("PK__LR_STATU__0C9B456A24A8FDE8");
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__ORDER__460A9464FBEE7085");

            entity.Property(e => e.CREATE_TIME).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.GUEST).WithMany(p => p.ORDER).HasConstraintName("FK__ORDER__GUEST_ID__571DF1D5");

            entity.HasOne(d => d.OS).WithMany(p => p.ORDER)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__OS_ID__5535A963");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDER).HasConstraintName("FK__ORDER__USER_ID__5629CD9C");
        });

        modelBuilder.Entity<ORDER_REJECT_REASON>(entity =>
        {
            entity.HasKey(e => e.ORR_ID).HasName("PK__ORDER_RE__6FCBE6DE35F21073");

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
            entity.HasKey(e => e.OD_ID).HasName("PK__ORDER_SI__3D0CE883E98C0FD8");

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
            entity.HasKey(e => e.OSS_ID).HasName("PK__ORDER_SI__F5B4ECDF5623995D");
        });

        modelBuilder.Entity<O_HISTORY_UPDATE>(entity =>
        {
            entity.HasKey(e => e.OHU_ID).HasName("PK__O_HISTOR__B86AF0FD58DADFA7");

            entity.Property(e => e.CHANGE_AT).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_HISTORY_UPDATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_HISTORY__ORDER__6E01572D");
        });

        modelBuilder.Entity<O_MATERIAL>(entity =>
        {
            entity.HasKey(e => e.OM_ID).HasName("PK__O_MATERI__AE0FC29B729FDCE3");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_MATERIAL)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_MATERIA__ORDER__656C112C");
        });

        modelBuilder.Entity<O_STATUS>(entity =>
        {
            entity.HasKey(e => e.OS_ID).HasName("PK__O_STATUS__85A506EDAE054EB2");
        });

        modelBuilder.Entity<O_TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.OT_ID).HasName("PK__O_TEMPLA__A9E9ACD247046DAB");

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_TEMPLATE)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_TEMPLAT__ORDER__628FA481");
        });

        modelBuilder.Entity<PART_WORK_LOG>(entity =>
        {
            entity.HasKey(e => e.WL_ID).HasName("PK__PART_WOR__931092C2275AF670");

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
            entity.HasKey(e => e.PRODUCTION_ID).HasName("PK__PRODUCTI__4E709CAA1224873C");

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
            entity.HasKey(e => e.ISSUE_ID).HasName("PK__PRODUCTI__E67F509CC122AF2C");

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
            entity.HasKey(e => e.ORR_ID).HasName("PK__PRODUCTI__6FCBE6DED81993D3");

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
            entity.HasKey(e => e.PP_ID).HasName("PK__P_PART__1662724A3831F7AE");

            entity.HasOne(d => d.PPS).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PPS_ID__114A936A");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.P_PART)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PRODUCTI__10566F31");
        });

        modelBuilder.Entity<P_PART_ORDER_SIZE>(entity =>
        {
            entity.HasKey(e => e.PPOS_ID).HasName("PK__P_PART_O__483DBE4B48D95C49");

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
                        j.HasKey("PPOS_ID", "USER_ID").HasName("PK__P_PART_A__A70650F434D68E92");
                    });
        });

        modelBuilder.Entity<P_PART_ORDER_SIZE_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPOSS_ID).HasName("PK__P_PART_O__DADC95F2ADCFB0D8");
        });

        modelBuilder.Entity<P_PART_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPS_ID).HasName("PK__P_PART_S__D138327A457B3AC9");
        });

        modelBuilder.Entity<P_STATUS>(entity =>
        {
            entity.HasKey(e => e.PS_ID).HasName("PK__P_STATUS__0119474C8E1D7910");
        });

        modelBuilder.Entity<ROLE>(entity =>
        {
            entity.HasKey(e => e.ROLE_ID).HasName("PK__ROLE__5AC4D2224D475890");
        });

        modelBuilder.Entity<SIZE>(entity =>
        {
            entity.HasKey(e => e.SIZE_ID).HasName("PK__SIZE__694E1A7DF5EAFB58");
        });

        modelBuilder.Entity<TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.TEMPLATE_ID).HasName("PK__TEMPLATE__BACD412F979F8BFC");
        });

        modelBuilder.Entity<TEMPLATE_STEP>(entity =>
        {
            entity.HasKey(e => e.STEP_ID).HasName("PK__TEMPLATE__A894037449432128");

            entity.HasOne(d => d.TEMPLATE).WithMany(p => p.TEMPLATE_STEP)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TEMPLATE___TEMPL__2EDAF651");
        });

        modelBuilder.Entity<UO_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UO_COMME__10D5574C681341FE");

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
            entity.HasKey(e => e.USER_ID).HasName("PK__USER__F3BEEBFFE45B5CA9");

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
                        j.HasKey("USER_ID", "ROLE_ID").HasName("PK__USER_ROL__C612A6DD2843C6DF");
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
                        j.HasKey("USER_ID", "WS_ID").HasName("PK__USER_WOR__037771FDCED5ED7A");
                    });
        });

        modelBuilder.Entity<USER_AUTHORIZE>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<U_STATUS>(entity =>
        {
            entity.HasKey(e => e.US_ID).HasName("PK__U_STATUS__DE473D816E8C9A0F");
        });

        modelBuilder.Entity<WORKER_SKILL>(entity =>
        {
            entity.HasKey(e => e.WS_ID).HasName("PK__WORKER_S__0C99A02EB49E892C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
