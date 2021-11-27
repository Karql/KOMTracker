﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace KomTracker.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(KOMDBContext))]
    [Migration("20211029205552_AddIdentityAndAthleteAndTokenTables")]
    partial class AddIdentityAndAthleteAndTokenTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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

                    b.ToTable("athlete");
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.RoleClaimEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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

                    b.ToTable("role_claim");
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

                    b.ToTable("role");
                });

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserClaimEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

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

                    b.ToTable("user_claim");
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

                    b.ToTable("user_login");
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

                    b.ToTable("user");
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

                    b.ToTable("user_role");
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

                    b.ToTable("user_token");
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Token.TokenModel", b =>
                {
                    b.Property<int>("AthleteId")
                        .HasColumnType("integer")
                        .HasColumnName("athlete_id");

                    b.Property<string>("AccessToken")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("access-token");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("timestamp without time zone")
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

                    b.ToTable("token");
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

            modelBuilder.Entity("KomTracker.Infrastructure.Identity.Entities.UserLoginEntity", b =>
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
                        .WithOne("User")
                        .HasForeignKey("KomTracker.Infrastructure.Identity.Entities.UserEntity", "AthleteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Athlete");
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

            modelBuilder.Entity("KomTracker.Domain.Entities.Token.TokenModel", b =>
                {
                    b.HasOne("KomTracker.Domain.Entities.Athlete.AthleteEntity", "Athlete")
                        .WithOne("Token")
                        .HasForeignKey("KomTracker.Domain.Entities.Token.TokenModel", "AthleteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Athlete");
                });

            modelBuilder.Entity("KomTracker.Domain.Entities.Athlete.AthleteEntity", b =>
                {
                    b.Navigation("Token");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
