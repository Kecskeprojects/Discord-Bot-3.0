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

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<ServerMutedUser> ServerMutedUsers { get; set; }

    public virtual DbSet<TwitchChannel> TwitchChannels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserIdolStatistic> UserIdolStatistics { get; set; }

    public virtual DbSet<WeeklyPoll> WeeklyPolls { get; set; }

    public virtual DbSet<WeeklyPollOption> WeeklyPollOptions { get; set; }

    public virtual DbSet<WeeklyPollOptionPreset> WeeklyPollOptionPresets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Birthday>(entity =>
        {
            entity.HasKey(e => e.BirthdayId).HasName("PK_BirthdayId");

            entity.ToTable("Birthday");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

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

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
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
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
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
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
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

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Idol>(entity =>
        {
            entity.HasKey(e => e.IdolId).HasName("PK_IdolId");

            entity.ToTable("Idol");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.KoreanFullName).HasMaxLength(100);
            entity.Property(e => e.KoreanStageName).HasMaxLength(100);
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ProfileUrl).HasMaxLength(200);
            entity.Property(e => e.StageName).HasMaxLength(100);

            entity.HasOne(d => d.Group).WithMany(p => p.Idols)
                .HasForeignKey(d => d.GroupId)
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
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Idol).WithMany(p => p.IdolAliases)
                .HasForeignKey(d => d.IdolId)
                .HasConstraintName("FK_IdolAlias_Idol");
        });

        modelBuilder.Entity<IdolGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK_GroupId");

            entity.ToTable("IdolGroup");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FullKoreanName).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<IdolImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK_ImageId");

            entity.ToTable("IdolImage");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("ImageURL");
            entity.Property(e => e.OverriddenUrl)
                .HasMaxLength(200)
                .HasColumnName("OverriddenURL");

            entity.HasOne(d => d.Idol).WithMany(p => p.IdolImages)
                .HasForeignKey(d => d.IdolId)
                .HasConstraintName("FK_IdolImage_Idol");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.ReminderId).HasName("PK_ReminderId");

            entity.ToTable("Reminder");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
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

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
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

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RoleMessageDiscordId)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.MuteRole).WithMany(p => p.ServerMuteRoles)
                .HasForeignKey(d => d.MuteRoleId)
                .HasConstraintName("FK_Server_MuteRole");

            entity.HasOne(d => d.NotificationRole).WithMany(p => p.ServerNotificationRoles)
                .HasForeignKey(d => d.NotificationRoleId)
                .HasConstraintName("FK_Server_NotificationRole");
        });

        modelBuilder.Entity<ServerMutedUser>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ServerId }).HasName("PK_Muted_ServerId_UserId");

            entity.Property(e => e.MutedUntil).HasColumnType("datetime");
            entity.Property(e => e.RemovedRoleDiscordIds).IsRequired();

            entity.HasOne(d => d.Server).WithMany(p => p.ServerMutedUsers)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServerMutedUsers_Server");

            entity.HasOne(d => d.User).WithMany(p => p.ServerMutedUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServerMutedUsers_User");
        });

        modelBuilder.Entity<TwitchChannel>(entity =>
        {
            entity.HasKey(e => e.TwitchChannelId).HasName("PK_TwitchChannelId");

            entity.ToTable("TwitchChannel");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TwitchId)
                .IsRequired()
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.TwitchLink)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TwitchName)
                .IsRequired()
                .HasMaxLength(75)
                .IsUnicode(false);

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

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LastFmusername)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("LastFMUsername");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasMany(d => d.Idols).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserBias",
                    r => r.HasOne<Idol>().WithMany()
                        .HasForeignKey("IdolId")
                        .HasConstraintName("FK_UserBias_Idol"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserBias_User"),
                    j =>
                    {
                        j.HasKey("UserId", "IdolId").HasName("PK_UserId_IdolId");
                        j.ToTable("UserBias");
                    });
        });

        modelBuilder.Entity<UserIdolStatistic>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.IdolId }).HasName("PK_UserId_IdolId_UserIdolStatistic");

            entity.ToTable("UserIdolStatistic");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Idol).WithMany(p => p.UserIdolStatistics)
                .HasForeignKey(d => d.IdolId)
                .HasConstraintName("FK_UserIdolStatistic_Idol");

            entity.HasOne(d => d.User).WithMany(p => p.UserIdolStatistics)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserIdolStatistic_User");
        });

        modelBuilder.Entity<WeeklyPoll>(entity =>
        {
            entity.HasKey(e => e.WeeklyPollId).HasName("PK_WeeklyPollId");

            entity.ToTable("WeeklyPoll");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.RepeatOnDayOfWeek)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.HasOne(d => d.Channel).WithMany(p => p.WeeklyPolls)
                .HasForeignKey(d => d.ChannelId)
                .HasConstraintName("FK_WeeklyPoll_ChannelId");

            entity.HasOne(d => d.OptionPreset).WithMany(p => p.WeeklyPolls)
                .HasForeignKey(d => d.OptionPresetId)
                .HasConstraintName("FK_WeeklyPoll_WeeklyPollOptionPresetId");

            entity.HasOne(d => d.Role).WithMany(p => p.WeeklyPolls)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_WeeklyPoll_RoleId");

            entity.HasOne(d => d.Server).WithMany(p => p.WeeklyPolls)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WeeklyPoll_ServerId");
        });

        modelBuilder.Entity<WeeklyPollOption>(entity =>
        {
            entity.HasKey(e => e.WeeklyPollOptionId).HasName("PK_WeeklyPollOptionId");

            entity.ToTable("WeeklyPollOption");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(55);

            entity.HasOne(d => d.WeeklyPoll).WithMany(p => p.WeeklyPollOptions)
                .HasForeignKey(d => d.WeeklyPollId)
                .HasConstraintName("FK_WeeklyPollOption_WeeklyPollId");

            entity.HasOne(d => d.WeeklyPollOptionPreset).WithMany(p => p.WeeklyPollOptions)
                .HasForeignKey(d => d.WeeklyPollOptionPresetId)
                .HasConstraintName("FK_WeeklyPollOption_WeeklyPollOptionPresetId");
        });

        modelBuilder.Entity<WeeklyPollOptionPreset>(entity =>
        {
            entity.HasKey(e => e.WeeklyPollOptionPresetId).HasName("PK_WeeklyPollOptionPresetId");

            entity.ToTable("WeeklyPollOptionPreset");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
