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

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<ChannelType> ChannelTypes { get; set; }

    public virtual DbSet<CustomCommand> CustomCommands { get; set; }

    public virtual DbSet<Greeting> Greetings { get; set; }

    public virtual DbSet<Idol> Idols { get; set; }

    public virtual DbSet<IdolGroup> IdolGroups { get; set; }

    public virtual DbSet<Keyword> Keywords { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<ServerSettingChannel> ServerSettingChannels { get; set; }

    public virtual DbSet<TwitchChannel> TwitchChannels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBias> UserBiases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("PK__Channel__38C3E814956C735B");

            entity.ToTable("Channel");

            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Server).WithMany(p => p.Channels)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Channel_Server");
        });

        modelBuilder.Entity<ChannelType>(entity =>
        {
            entity.HasKey(e => e.ChannelTypeId).HasName("PK__ChannelT__5ACA2B180367F05A");

            entity.ToTable("ChannelType");

            entity.Property(e => e.ChannelTypeId).ValueGeneratedNever();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CustomCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId).HasName("PK__CustomCo__6B410B06E55CACF7");

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
            entity.HasKey(e => e.GreetingId).HasName("PK__Greeting__FCC1D6527F344E4A");

            entity.ToTable("Greeting");

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Idol>(entity =>
        {
            entity.HasKey(e => e.IdolId).HasName("PK__Idol__A041481E21438F3E");

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

        modelBuilder.Entity<IdolGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__IdolGrou__149AF36AB8098B82");

            entity.ToTable("IdolGroup");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Keyword>(entity =>
        {
            entity.HasKey(e => e.KeywordId).HasName("PK__Keyword__37C135212EDFF2DF");

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
            entity.HasKey(e => e.ReminderId).HasName("PK__Reminder__01A830875522E92B");

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
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A1334FAB2");

            entity.ToTable("Role");

            entity.HasIndex(e => e.DiscordId, "UQ__Role__4AB57A5DA315F836").IsUnique();

            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Server).WithMany(p => p.Roles)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Role_Server");
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.ServerId).HasName("PK__Server__C56AC8E61234E88D");

            entity.ToTable("Server");

            entity.HasIndex(e => e.DiscordId, "UQ__Server__4AB57A5D52BAFD40").IsUnique();

            entity.Property(e => e.DiscordId)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ServerSettingChannel>(entity =>
        {
            entity.HasKey(e => e.ServerSettingChannelId).HasName("PK__ServerSe__9799A24309A1938A");

            entity.ToTable("ServerSettingChannel");

            entity.HasOne(d => d.Channel).WithMany(p => p.ServerSettingChannels)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServerSettingChannel_Channel");

            entity.HasOne(d => d.ChannelType).WithMany(p => p.ServerSettingChannels)
                .HasForeignKey(d => d.ChannelTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServerSettingChannel_ChannelType");
        });

        modelBuilder.Entity<TwitchChannel>(entity =>
        {
            entity.HasKey(e => e.TwitchChannelId).HasName("PK__TwitchCh__908BAECF915B9E5C");

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
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C007521E0");

            entity.ToTable("User");

            entity.HasIndex(e => e.DiscordId, "UQ__User__4AB57A5D36E69D80").IsUnique();

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
            entity.HasKey(e => e.UserBiasId).HasName("PK__UserBias__0940EA0592E06A09");

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
