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

    public virtual DbSet<LEAVE_REQUEST> LEAVE_REQUESTs { get; set; }

    public virtual DbSet<LOG_EVENT> LOG_EVENTs { get; set; }

    public virtual DbSet<LR_STATUS> LR_STATUSes { get; set; }

    public virtual DbSet<ORDER> ORDERs { get; set; }

    public virtual DbSet<O_HISTORY_UPDATE> O_HISTORY_UPDATEs { get; set; }

    public virtual DbSet<O_MATERIAL> O_MATERIALs { get; set; }

    public virtual DbSet<O_STATUS> O_STATUSes { get; set; }

    public virtual DbSet<O_TEMPLATE> O_TEMPLATEs { get; set; }

    public virtual DbSet<PP_ASSIGNEE> PP_ASSIGNEEs { get; set; }

    public virtual DbSet<PP_ASSIGNEE_STATUS> PP_ASSIGNEE_STATUSes { get; set; }

    public virtual DbSet<PRODUCTION> PRODUCTIONs { get; set; }

    public virtual DbSet<P_PART> P_PARTs { get; set; }

    public virtual DbSet<P_PART_OUTPUT> P_PART_OUTPUTs { get; set; }

    public virtual DbSet<P_PART_OUTPUT_STATUS> P_PART_OUTPUT_STATUSes { get; set; }

    public virtual DbSet<P_PLAN> P_PLANs { get; set; }

    public virtual DbSet<P_PLAN_STATUS> P_PLAN_STATUSes { get; set; }

    public virtual DbSet<P_STATUS> P_STATUSes { get; set; }

    public virtual DbSet<ROLE> ROLEs { get; set; }

    public virtual DbSet<UO_COMMENT> UO_COMMENTs { get; set; }

    public virtual DbSet<UP_COMMENT> UP_COMMENTs { get; set; }

    public virtual DbSet<USER> USERs { get; set; }

    public virtual DbSet<U_PAYROLL_OUTPUT> U_PAYROLL_OUTPUTs { get; set; }

    public virtual DbSet<U_PAYROLL_OUTPUT_STATUS> U_PAYROLL_OUTPUT_STATUSes { get; set; }

    public virtual DbSet<U_STATUS> U_STATUSes { get; set; }

    public virtual DbSet<WORKER_ROLE> WORKER_ROLEs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;User Id=sa;Password=123456;Database=GPMS_SYSTEM;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LEAVE_REQUEST>(entity =>
        {
            entity.HasKey(e => e.LR_ID).HasName("PK__LEAVE_RE__266E7F0FC2591072");

            entity.ToTable("LEAVE_REQUEST");

            entity.Property(e => e.CONTENT).HasMaxLength(500);
            entity.Property(e => e.DATE_CREATE).HasColumnType("datetime");
            entity.Property(e => e.DATE_REPLY).HasColumnType("datetime");
            entity.Property(e => e.DENY_CONTENT).HasMaxLength(500);

            entity.HasOne(d => d.LRS).WithMany(p => p.LEAVE_REQUESTs)
                .HasForeignKey(d => d.LRS_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__LRS_I__4BAC3F29");

            entity.HasOne(d => d.USER).WithMany(p => p.LEAVE_REQUESTs)
                .HasForeignKey(d => d.USER_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LEAVE_REQ__USER___4AB81AF0");
        });

        modelBuilder.Entity<LOG_EVENT>(entity =>
        {
            entity.HasKey(e => e.LOG_ID).HasName("PK__LOG_EVEN__4364C88264596FDB");

            entity.ToTable("LOG_EVENTS");

            entity.Property(e => e.LEVEL).HasMaxLength(50);
            entity.Property(e => e.MESSAGE).HasMaxLength(500);
            entity.Property(e => e.MESSAGE_TEMPLATE).HasMaxLength(500);
            entity.Property(e => e.TIMESTAMP).HasColumnType("datetime");
        });

        modelBuilder.Entity<LR_STATUS>(entity =>
        {
            entity.HasKey(e => e.LRS_ID).HasName("PK__LR_STATU__0C9B456A5667B098");

            entity.ToTable("LR_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<ORDER>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__ORDER__460A9464A9392A97");

            entity.ToTable("ORDER");

            entity.Property(e => e.COLOR).HasMaxLength(50);
            entity.Property(e => e.CPU).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NOTE).HasMaxLength(500);
            entity.Property(e => e.SIZE).HasMaxLength(50);
            entity.Property(e => e.TYPE).HasMaxLength(50);

            entity.HasOne(d => d.OS).WithMany(p => p.ORDERs)
                .HasForeignKey(d => d.OS_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__OS_ID__5070F446");

            entity.HasOne(d => d.USER).WithMany(p => p.ORDERs)
                .HasForeignKey(d => d.USER_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER__USER_ID__5165187F");
        });

        modelBuilder.Entity<O_HISTORY_UPDATE>(entity =>
        {
            entity.HasKey(e => e.OHU_ID).HasName("PK__O_HISTOR__B86AF0FDD128B0CD");

            entity.ToTable("O_HISTORY_UPDATE");

            entity.Property(e => e.FIELD_NAME).HasMaxLength(100);
            entity.Property(e => e.NEW_VALUE).HasMaxLength(255);
            entity.Property(e => e.OLD_VALUE).HasMaxLength(255);

            entity.HasOne(d => d.ORDER).WithMany(p => p.O_HISTORY_UPDATEs)
                .HasForeignKey(d => d.ORDER_ID)
                .HasConstraintName("FK__O_HISTORY__ORDER__5DCAEF64");
        });

        modelBuilder.Entity<O_MATERIAL>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__O_MATERI__460A9464C832FCF6");

            entity.ToTable("O_MATERIAL");

            entity.Property(e => e.ORDER_ID).ValueGeneratedNever();
            entity.Property(e => e.IMAGE).HasMaxLength(255);
            entity.Property(e => e.NAME).HasMaxLength(150);
            entity.Property(e => e.NOTE).HasMaxLength(255);
            entity.Property(e => e.UOM).HasMaxLength(50);
            entity.Property(e => e.VALUE).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.ORDER).WithOne(p => p.O_MATERIAL)
                .HasForeignKey<O_MATERIAL>(d => d.ORDER_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_MATERIAL__NOTE__571DF1D5");
        });

        modelBuilder.Entity<O_STATUS>(entity =>
        {
            entity.HasKey(e => e.OS_ID).HasName("PK__O_STATUS__85A506ED26F197C4");

            entity.ToTable("O_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<O_TEMPLATE>(entity =>
        {
            entity.HasKey(e => e.ORDER_ID).HasName("PK__O_TEMPLA__460A9464E63F3895");

            entity.ToTable("O_TEMPLATE");

            entity.Property(e => e.ORDER_ID).ValueGeneratedNever();
            entity.Property(e => e.FILE).HasMaxLength(255);
            entity.Property(e => e.TYPE).HasMaxLength(50);

            entity.HasOne(d => d.ORDER).WithOne(p => p.O_TEMPLATE)
                .HasForeignKey<O_TEMPLATE>(d => d.ORDER_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__O_TEMPLAT__ORDER__5441852A");
        });

        modelBuilder.Entity<PP_ASSIGNEE>(entity =>
        {
            entity.HasKey(e => e.PPA_ID).HasName("PK__PP_ASSIG__59749F3FC9407E92");

            entity.ToTable("PP_ASSIGNEE");

            entity.Property(e => e.DATE_ASSIGN).HasColumnType("datetime");

            entity.HasOne(d => d.PPAS).WithMany(p => p.PP_ASSIGNEEs)
                .HasForeignKey(d => d.PPAS_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PP_ASSIGN__PPAS___71D1E811");

            entity.HasOne(d => d.PP).WithMany(p => p.PP_ASSIGNEEs)
                .HasForeignKey(d => d.PP_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PP_ASSIGN__PP_ID__72C60C4A");

            entity.HasOne(d => d.USER).WithMany(p => p.PP_ASSIGNEEs)
                .HasForeignKey(d => d.USER_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PP_ASSIGN__USER___73BA3083");
        });

        modelBuilder.Entity<PP_ASSIGNEE_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPAS_ID).HasName("PK__PP_ASSIG__C315D90E05915D1C");

            entity.ToTable("PP_ASSIGNEE_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<PRODUCTION>(entity =>
        {
            entity.HasKey(e => e.PRODUCTION_ID).HasName("PK__PRODUCTI__4E709CAA79A4DAE1");

            entity.ToTable("PRODUCTION");

            entity.HasOne(d => d.ORDER).WithMany(p => p.PRODUCTIONs)
                .HasForeignKey(d => d.ORDER_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__ORDER__6477ECF3");

            entity.HasOne(d => d.PM).WithMany(p => p.PRODUCTIONs)
                .HasForeignKey(d => d.PM_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PM_ID__6383C8BA");

            entity.HasOne(d => d.PS).WithMany(p => p.PRODUCTIONs)
                .HasForeignKey(d => d.PS_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTIO__PS_ID__628FA481");
        });

        modelBuilder.Entity<P_PART>(entity =>
        {
            entity.HasKey(e => e.PP_ID).HasName("PK__P_PART__1662724A6DB612FC");

            entity.ToTable("P_PART");

            entity.Property(e => e.PP_ID).ValueGeneratedNever();
            entity.Property(e => e.CPU).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.END_TIME).HasColumnType("datetime");
            entity.Property(e => e.NAME).HasMaxLength(150);
            entity.Property(e => e.START_TIME).HasColumnType("datetime");

            entity.HasOne(d => d.PP).WithOne(p => p.P_PART)
                .HasForeignKey<P_PART>(d => d.PP_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART__PP_ID__6D0D32F4");
        });

        modelBuilder.Entity<P_PART_OUTPUT>(entity =>
        {
            entity.HasKey(e => e.PPO_ID).HasName("PK__P_PART_O__3C74DCA9E9DB49E7");

            entity.ToTable("P_PART_OUTPUT");

            entity.Property(e => e.DATETIME_SUBMIT).HasColumnType("datetime");
            entity.Property(e => e.DATETIME_VALIDATE).HasColumnType("datetime");
            entity.Property(e => e.FALSE_VALUE).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TRUE_VALUE).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UOM).HasMaxLength(50);
            entity.Property(e => e.VALUE).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.PPOS).WithMany(p => p.P_PART_OUTPUTs)
                .HasForeignKey(d => d.PPOS_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART_OU__PPOS___787EE5A0");

            entity.HasOne(d => d.PP).WithMany(p => p.P_PART_OUTPUTs)
                .HasForeignKey(d => d.PP_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PART_OU__PP_ID__797309D9");
        });

        modelBuilder.Entity<P_PART_OUTPUT_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPOS_ID).HasName("PK__P_PART_O__483DBE4B96040760");

            entity.ToTable("P_PART_OUTPUT_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<P_PLAN>(entity =>
        {
            entity.HasKey(e => e.PP_ID).HasName("PK__P_PLAN__1662724A2F5688EC");

            entity.ToTable("P_PLAN");

            entity.HasOne(d => d.PRODUCTION).WithMany(p => p.P_PLANs)
                .HasForeignKey(d => d.PRODUCTION_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PLAN__PRODUCTI__6A30C649");

            entity.HasOne(d => d.P_PLAN_STATUSNavigation).WithMany(p => p.P_PLANs)
                .HasForeignKey(d => d.P_PLAN_STATUS)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__P_PLAN__P_PLAN_S__693CA210");
        });

        modelBuilder.Entity<P_PLAN_STATUS>(entity =>
        {
            entity.HasKey(e => e.PPS_ID).HasName("PK__P_PLAN_S__D138327A77A77BC8");

            entity.ToTable("P_PLAN_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<P_STATUS>(entity =>
        {
            entity.HasKey(e => e.PS_ID).HasName("PK__P_STATUS__0119474CFAB4F410");

            entity.ToTable("P_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<ROLE>(entity =>
        {
            entity.HasKey(e => e.ROLE_ID).HasName("PK__ROLE__5AC4D222D9FBCD0C");

            entity.ToTable("ROLE");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<UO_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UO_COMME__10D5574C655C6FCB");

            entity.ToTable("UO_COMMENT");

            entity.Property(e => e.CONTENT).HasMaxLength(500);
            entity.Property(e => e.SEND_DATETIME).HasColumnType("datetime");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UO_COMMENTs)
                .HasForeignKey(d => d.FROM_USER)
                .HasConstraintName("FK__UO_COMMEN__FROM___59FA5E80");

            entity.HasOne(d => d.TO_ORDERNavigation).WithMany(p => p.UO_COMMENTs)
                .HasForeignKey(d => d.TO_ORDER)
                .HasConstraintName("FK__UO_COMMEN__TO_OR__5AEE82B9");
        });

        modelBuilder.Entity<UP_COMMENT>(entity =>
        {
            entity.HasKey(e => e.OC_ID).HasName("PK__UP_COMME__10D5574C31FB4CE9");

            entity.ToTable("UP_COMMENT");

            entity.Property(e => e.CONTENT).HasMaxLength(500);
            entity.Property(e => e.SEND_DATETIME).HasColumnType("datetime");

            entity.HasOne(d => d.FROM_USERNavigation).WithMany(p => p.UP_COMMENTs)
                .HasForeignKey(d => d.FROM_USER)
                .HasConstraintName("FK__UP_COMMEN__FROM___02084FDA");

            entity.HasOne(d => d.TO_PONavigation).WithMany(p => p.UP_COMMENTs)
                .HasForeignKey(d => d.TO_PO)
                .HasConstraintName("FK__UP_COMMEN__TO_PO__02FC7413");
        });

        modelBuilder.Entity<USER>(entity =>
        {
            entity.HasKey(e => e.USER_ID).HasName("PK__USER__F3BEEBFFA8B7CC62");

            entity.ToTable("USER");

            entity.HasIndex(e => e.FULLNAME, "UQ__USER__B6AFA08AA4401DA9").IsUnique();

            entity.Property(e => e.EMAIL).HasMaxLength(150);
            entity.Property(e => e.FULLNAME).HasMaxLength(100);
            entity.Property(e => e.LOCATION).HasMaxLength(255);
            entity.Property(e => e.PASSWORD).HasMaxLength(255);
            entity.Property(e => e.PHONE_NUMBER).HasMaxLength(20);

            entity.HasOne(d => d.US).WithMany(p => p.USERs)
                .HasForeignKey(d => d.US_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USER__US_ID__3A81B327");

            entity.HasMany(d => d.ROLEs).WithMany(p => p.USERs)
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
                        j.HasKey("USER_ID", "ROLE_ID").HasName("PK__USER_ROL__C612A6DDBDF8E00B");
                        j.ToTable("USER_ROLE");
                    });

            entity.HasMany(d => d.WRs).WithMany(p => p.USERs)
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
                        j.HasKey("USER_ID", "WR_ID").HasName("PK__USER_WOR__23CCFCE1AF71D1D5");
                        j.ToTable("USER_WORKER_ROLE");
                    });
        });

        modelBuilder.Entity<U_PAYROLL_OUTPUT>(entity =>
        {
            entity.HasKey(e => e.UP_ID).HasName("PK__U_PAYROL__86DC95D03F5E86AC");

            entity.ToTable("U_PAYROLL_OUTPUT");

            entity.Property(e => e.NOTE).HasMaxLength(255);
            entity.Property(e => e.TOTAL_PRICE).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.UPOS).WithMany(p => p.U_PAYROLL_OUTPUTs)
                .HasForeignKey(d => d.UPOS_ID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__U_PAYROLL__UPOS___7E37BEF6");

            entity.HasOne(d => d.USER).WithMany(p => p.U_PAYROLL_OUTPUTs)
                .HasForeignKey(d => d.USER_ID)
                .HasConstraintName("FK__U_PAYROLL__USER___7F2BE32F");
        });

        modelBuilder.Entity<U_PAYROLL_OUTPUT_STATUS>(entity =>
        {
            entity.HasKey(e => e.UPOS_ID).HasName("PK__U_PAYROL__066D5F55C8491BB6");

            entity.ToTable("U_PAYROLL_OUTPUT_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<U_STATUS>(entity =>
        {
            entity.HasKey(e => e.US_ID).HasName("PK__U_STATUS__DE473D81D6B963E2");

            entity.ToTable("U_STATUS");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        modelBuilder.Entity<WORKER_ROLE>(entity =>
        {
            entity.HasKey(e => e.WR_ID).HasName("PK__WORKER_R__072171ECC2A1A599");

            entity.ToTable("WORKER_ROLE");

            entity.Property(e => e.NAME).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
