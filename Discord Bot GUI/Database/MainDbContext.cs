using System;
using System.Collections.Generic;
using Discord_Bot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Discord_Bot.Database;

public partial class MainDbContext : DbContext
{
    public MainDbContext(DbContextOptions<MainDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Birthday> Birthdays { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<ChannelType> ChannelTypes { get; set; }

    public virtual DbSet<CustomCommand> CustomCommands { get; set; }

    public virtual DbSet<Greeting> Greetings { get; set; }

    public virtual DbSet<Idol> Idols { get; set; }

    public virtual DbSet<IdolAlias> IdolAliases { get; set; }

    public virtual DbSet<IdolGroup> IdolGroups { get; set; }

    public virtual DbSet<IdolImage> IdolImages { get; set; }

    public virtual DbSet<Keyword> Keywords { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<ServerChannelView> ServerChannelViews { get; set; }

    public virtual DbSet<TwitchChannel> TwitchChannels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBias> UserBiases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Birthday>(entity =>
        {
            entity.HasKey(e => e.BirthdayId).HasName("PK_BirthdayId");

            entity.ToTable("Birthday");

            entity.Property(e => e.Date).HasColumnType("date");

            entity.HasOne(d => d.Server).WithMany(p => p.Birthdays)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Birthday_Server");

            entity.HasOne(d => d.User).WithMany(p => p.Birthdays)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Birthday_User");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("PK_ChannelId");

            entity.ToTable("Channel");

            entity.HasIndex(e => e.DiscordId, "UQ_ChannelDiscordId").IsUnique();

            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Server).WithMany(p => p.Channels)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Channel_Server");

            entity.HasMany(d => d.ChannelTypes).WithMany(p => p.Channels)
                .UsingEntity<Dictionary<string, object>>(
                    "ServerSettingChannel",
                    r => r.HasOne<ChannelType>().WithMany()
                        .HasForeignKey("ChannelTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ServerSettingChannel_ChannelType"),
                    l => l.HasOne<Channel>().WithMany()
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ServerSettingChannel_Channel"),
                    j =>
                    {
                        j.HasKey("ChannelId", "ChannelTypeId").HasName("PK_ChannelId_ChannelTypeId");
                        j.ToTable("ServerSettingChannel");
                    });
        });

        modelBuilder.Entity<ChannelType>(entity =>
        {
            entity.HasKey(e => e.ChannelTypeId).HasName("PK_ChannelTypeId");

            entity.ToTable("ChannelType");

            entity.Property(e => e.ChannelTypeId).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CustomCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId).HasName("PK_CommandId");

            entity.ToTable("CustomCommand");

            entity.Property(e => e.Command)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.Server).WithMany(p => p.CustomCommands)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomCommand_Server");
        });

        modelBuilder.Entity<Greeting>(entity =>
        {
            entity.HasKey(e => e.GreetingId).HasName("PK_GreetingId");

            entity.ToTable("Greeting");

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Idol>(entity =>
        {
            entity.HasKey(e => e.IdolId).HasName("PK_IdolId");

            entity.ToTable("Idol");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Group).WithMany(p => p.Idols)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Idol_IdolGroup");
        });

        modelBuilder.Entity<IdolAlias>(entity =>
        {
            entity.HasKey(e => e.IdolAliasId).HasName("PK_IdolAliasId");

            entity.ToTable("IdolAlias");

            entity.Property(e => e.Alias)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Idol).WithMany(p => p.IdolAliases)
                .HasForeignKey(d => d.IdolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IdolAlias_Idol");
        });

        modelBuilder.Entity<IdolGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK_GroupId");

            entity.ToTable("IdolGroup");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<IdolImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK_ImageId");

            entity.ToTable("IdolImage");

            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ImageURL");

            entity.HasOne(d => d.Idol).WithMany(p => p.IdolImages)
                .HasForeignKey(d => d.IdolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IdolImage_Idol");
        });

        modelBuilder.Entity<Keyword>(entity =>
        {
            entity.HasKey(e => e.KeywordId).HasName("PK_KeywordId");

            entity.ToTable("Keyword");

            entity.Property(e => e.Response)
                .IsRequired()
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Trigger)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Server).WithMany(p => p.Keywords)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Keyword_Server");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.ReminderId).HasName("PK_ReminderId");

            entity.ToTable("Reminder");

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reminder_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK_RoleId");

            entity.ToTable("Role");

            entity.HasIndex(e => e.DiscordId, "UQ_RoleDiscordId").IsUnique();

            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Server).WithMany(p => p.Roles)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Role_Server");
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.ServerId).HasName("PK_ServerId");

            entity.ToTable("Server");

            entity.HasIndex(e => e.DiscordId, "UQ_ServerDiscordId").IsUnique();

            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ServerChannelView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ServerChannelView");

            entity.Property(e => e.ChannelDiscordId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ChannelTypeName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ServerDiscordId)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TwitchChannel>(entity =>
        {
            entity.HasKey(e => e.TwitchChannelId).HasName("PK_TwitchChannelId");

            entity.ToTable("TwitchChannel");

            entity.Property(e => e.TwitchId)
                .IsRequired()
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.TwitchLink)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.TwitchChannels)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_TwitchChannel_Role");

            entity.HasOne(d => d.Server).WithMany(p => p.TwitchChannels)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TwitchChannel_Server");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_UserId");

            entity.ToTable("User");

            entity.HasIndex(e => e.DiscordId, "UQ_UserDiscordId").IsUnique();

            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LastFmusername)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("LastFMUsername");
        });

        modelBuilder.Entity<UserBias>(entity =>
        {
            entity.HasKey(e => e.UserBiasId).HasName("PK_UserBiasId");

            entity.ToTable("UserBias");

            entity.HasOne(d => d.Idol).WithMany(p => p.UserBiases)
                .HasForeignKey(d => d.IdolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserBias_Idol");

            entity.HasOne(d => d.User).WithMany(p => p.UserBiases)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserBias_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
