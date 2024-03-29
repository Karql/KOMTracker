﻿// <auto-generated />
using System;
using KomTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(KOMDBContext))]
    [Migration("20211220160147_FixNullColumns")]
    partial class FixNullColumns
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("KomTracker.Domain.Entities.Athlete.AthleteEntity", b =>
                {
                    b.Property<int>("AthleteId")
                        .HasColumnType("integer")
                        .HasColumnName("athlete_id");

                    b.Property<string>("Bio")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)")
                        .HasColumnName("bio");

                    b.Property<string>("City")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("city");

                    b.Property<string>("Country")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("country");

                    b.Property<string>("FirstName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("first_name");

                    b.Property<string>("LastName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("last_name");

                    b.Property<string>("Profile")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("profile");

                    b.Property<string>("ProfileMedium")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("profile_medium");

                    b.Property<string>("Sex")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("sex");

                    b.Property<string>("Username")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("username");

                    b.Property<float>("Weight")
                        .HasColumnType("real")
                        .HasColumnName("weight");

                    b.HasKey("AthleteId");

                    b.ToTable("athlete", (string)null);
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Segment.KomsSummaryEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AthleteId")
                        .HasColumnType("integer")
                        .HasColumnName("athlete_id");

                    b.Property<int>("ImprovedKoms")
                        .HasColumnType("integer")
                        .HasColumnName("improved_koms");

                    b.Property<int>("Koms")
                        .HasColumnType("integer")
                        .HasColumnName("koms");

                    b.Property<int>("LostKoms")
                        .HasColumnType("integer")
                        .HasColumnName("lost_koms");

                    b.Property<int>("NewKoms")
                        .HasColumnType("integer")
                        .HasColumnName("new_koms");

                    b.Property<DateTime>("TrackDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("track_date");

                    b.HasKey("Id");

                    b.HasIndex("AthleteId", "TrackDate");

                    b.ToTable("koms_summary", (string)null);
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Segment.KomsSummarySegmentEffortEntity", b =>
                {
                    b.Property<int>("KomSummaryId")
                        .HasColumnType("integer")
                        .HasColumnName("koms_summary_id");

                    b.Property<long>("SegmentEffortId")
                        .HasColumnType("bigint")
                        .HasColumnName("segment_effort_id");

                    b.Property<bool>("ImprovedKom")
                        .HasColumnType("boolean")
                        .HasColumnName("improved_kom");

                    b.Property<bool>("Kom")
                        .HasColumnType("boolean")
                        .HasColumnName("kom");

                    b.Property<bool>("LostKom")
                        .HasColumnType("boolean")
                        .HasColumnName("lost_kom");

                    b.Property<bool>("NewKom")
                        .HasColumnType("boolean")
                        .HasColumnName("new_kom");

                    b.HasKey("KomSummaryId", "SegmentEffortId");

                    b.HasIndex("SegmentEffortId");

                    b.ToTable("koms_summary_segment_effort", (string)null);
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Segment.SegmentEffortEntity", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    b.Property<long>("ActivityId")
                        .HasColumnType("bigint")
                        .HasColumnName("activity_id");

                    b.Property<int>("AthleteId")
                        .HasColumnType("integer")
                        .HasColumnName("athlete_id");

                    b.Property<float?>("AverageCadence")
                        .HasColumnType("real")
                        .HasColumnName("average_cadence");

                    b.Property<float?>("AverageHeartrate")
                        .HasColumnType("real")
                        .HasColumnName("average_heartrate");

                    b.Property<float?>("AverageWatts")
                        .HasColumnType("real")
                        .HasColumnName("average_watts");

                    b.Property<bool>("DeviceWatts")
                        .HasColumnType("boolean")
                        .HasColumnName("device_watts");

                    b.Property<float>("Distance")
                        .HasColumnType("real")
                        .HasColumnName("distance");

                    b.Property<int>("ElapsedTime")
                        .HasColumnType("integer")
                        .HasColumnName("elapsed_time");

                    b.Property<int>("EndIndex")
                        .HasColumnType("integer")
                        .HasColumnName("end_index");

                    b.Property<int?>("KomRank")
                        .HasColumnType("integer")
                        .HasColumnName("kom_rank");

                    b.Property<float?>("MaxHeartrate")
                        .HasColumnType("real")
                        .HasColumnName("max_heartrate");

                    b.Property<int>("MovingTime")
                        .HasColumnType("integer")
                        .HasColumnName("moving_time");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<int?>("PrRank")
                        .HasColumnType("integer")
                        .HasColumnName("pr_rank");

                    b.Property<long>("SegmentId")
                        .HasColumnType("bigint")
                        .HasColumnName("segment_id");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_date");

                    b.Property<DateTime>("StartDateLocal")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_date_local");

                    b.Property<int>("StartIndex")
                        .HasColumnType("integer")
                        .HasColumnName("start_index");

                    b.HasKey("Id");

                    b.HasIndex("AthleteId");

                    b.HasIndex("SegmentId");

                    b.ToTable("segment_effort", (string)null);
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Segment.SegmentEntity", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    b.Property<string>("ActivityType")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("activity_type");

                    b.Property<float>("AverageGrade")
                        .HasColumnType("real")
                        .HasColumnName("average_grade");

                    b.Property<string>("City")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("city");

                    b.Property<int>("ClimbCategory")
                        .HasColumnType("integer")
                        .HasColumnName("climb_category");

                    b.Property<string>("Country")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("country");

                    b.Property<float>("Distance")
                        .HasColumnType("real")
                        .HasColumnName("distance");

                    b.Property<float>("ElevationHigh")
                        .HasColumnType("real")
                        .HasColumnName("elevation_high");

                    b.Property<float>("ElevationLow")
                        .HasColumnType("real")
                        .HasColumnName("elevation_low");

                    b.Property<float>("EndLatitude")
                        .HasColumnType("real")
                        .HasColumnName("end_latitude");

                    b.Property<float>("EndLongitude")
                        .HasColumnType("real")
                        .HasColumnName("end_longitude");

                    b.Property<bool>("Hazardous")
                        .HasColumnType("boolean")
                        .HasColumnName("hazardous");

                    b.Property<float>("MaximumGrade")
                        .HasColumnType("real")
                        .HasColumnName("maximum_grade");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<bool>("Private")
                        .HasColumnType("boolean")
                        .HasColumnName("private");

                    b.Property<bool>("Starred")
                        .HasColumnType("boolean")
                        .HasColumnName("starred");

                    b.Property<float>("StartLatitude")
                        .HasColumnType("real")
                        .HasColumnName("start_latitude");

                    b.Property<float>("StartLongitude")
                        .HasColumnType("real")
                        .HasColumnName("start_longitude");

                    b.HasKey("Id");

                    b.ToTable("segment", (string)null);
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Token.TokenEntity", b =>
                {
                    b.Property<int>("AthleteId")
                        .HasColumnType("integer")
                        .HasColumnName("athlete_id");

                    b.Property<string>("AccessToken")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("access-token");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires_at");

                    b.Property<string>("RefreshToken")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("refresh-token");

                    b.Property<string>("Scope")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("scope");

                    b.Property<string>("TokenType")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("token_type");

                    b.HasKey("AthleteId");

                    b.ToTable("token", (string)null);
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.RoleClaimEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text")
                        .HasColumnName("claim_value");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("role_id");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("role_claim", (string)null);
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.RoleEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text")
                        .HasColumnName("concurrency_stamp");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_name");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("role", (string)null);
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserClaimEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text")
                        .HasColumnName("claim_value");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("user_claim", (string)null);
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer")
                        .HasColumnName("access_failed_count");

                    b.Property<int>("AthleteId")
                        .HasColumnType("integer")
                        .HasColumnName("athlete_id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text")
                        .HasColumnName("concurrency_stamp");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("email");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("email_confirmed");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("lockout_enabled");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("lockout_end");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_email");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_username");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text")
                        .HasColumnName("password_hash");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text")
                        .HasColumnName("phone_number");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("phone_number_confirmed");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text")
                        .HasColumnName("security_stamp");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("two_factor_enabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.HasIndex("AthleteId")
                        .IsUnique();

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("user", (string)null);
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserLoginEntity", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text")
                        .HasColumnName("login_provider");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text")
                        .HasColumnName("provider_key");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text")
                        .HasColumnName("provider_display_name");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("user_login", (string)null);
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserRoleEntity", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.Property<string>("RoleId")
                        .HasColumnType("text")
                        .HasColumnName("role_id");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("user_role", (string)null);
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserTokenEntity", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text")
                        .HasColumnName("login_provider");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Value")
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("user_token", (string)null);
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Segment.KomsSummarySegmentEffortEntity", b =>
                {
                    b.HasOne("KomTracker.Domain.Entities.Segment.KomsSummaryEntity", "KomsSummary")
                        .WithMany()
                        .HasForeignKey("KomSummaryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KomTracker.Domain.Entities.Segment.SegmentEffortEntity", "SegmentEffort")
                        .WithMany()
                        .HasForeignKey("SegmentEffortId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("KomsSummary");

                    b.Navigation("SegmentEffort");
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Segment.SegmentEffortEntity", b =>
                {
                    b.HasOne("KomTracker.Domain.Entities.Athlete.AthleteEntity", null)
                        .WithMany()
                        .HasForeignKey("AthleteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KomTracker.Domain.Entities.Segment.SegmentEntity", null)
                        .WithMany()
                        .HasForeignKey("SegmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Token.TokenEntity", b =>
                {
                    b.HasOne("KomTracker.Domain.Entities.Athlete.AthleteEntity", "Athlete")
                        .WithOne("Token")
                        .HasForeignKey("KomTracker.Domain.Entities.Token.TokenEntity", "AthleteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Athlete");
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.RoleClaimEntity", b =>
                {
                    b.HasOne("KomTracker.Infrastructure.Identity.Entities.RoleEntity", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserClaimEntity", b =>
                {
                    b.HasOne("KomTracker.Infrastructure.Identity.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserEntity", b =>
                {
                    b.HasOne("KomTracker.Domain.Entities.Athlete.AthleteEntity", "Athlete")
                        .WithOne()
                        .HasForeignKey("KomTracker.Infrastructure.Identity.Entities.UserEntity", "AthleteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Athlete");
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserLoginEntity", b =>
                {
                    b.HasOne("KomTracker.Infrastructure.Identity.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserRoleEntity", b =>
                {
                    b.HasOne("KomTracker.Infrastructure.Identity.Entities.RoleEntity", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KomTracker.Infrastructure.Identity.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserTokenEntity", b =>
                {
                    b.HasOne("KomTracker.Infrastructure.Identity.Entities.UserEntity", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Athlete.AthleteEntity", b =>
                {
                    b.Navigation("Token");
                });
#pragma warning restore 612, 618
        }
    }
}
