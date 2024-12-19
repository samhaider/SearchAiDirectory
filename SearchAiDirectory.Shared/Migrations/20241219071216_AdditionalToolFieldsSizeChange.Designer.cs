﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SearchAiDirectory.Shared.Data;

#nullable disable

namespace SearchAiDirectory.Shared.Migrations
{
    [DbContext(typeof(ApplicationDataContext))]
    [Migration("20241219071216_AdditionalToolFieldsSizeChange")]
    partial class AdditionalToolFieldsSizeChange
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.AppLog", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("ID"));

                    b.Property<int>("Code")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Host")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Message")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("StackTrace")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Url")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("User")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("UserIp")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("ID");

                    b.ToTable("AppLogs", "dbo");
                });

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.Tool", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("ID"));

                    b.Property<long>("CategoryID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(750)
                        .HasColumnType("nvarchar(750)");

                    b.Property<bool>("IsConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("MetaDescription")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("MetaKeywords")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("Modified")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PriceModel")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("Website")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("WebsiteContent")
                        .HasMaxLength(7500)
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("CategoryID");

                    b.ToTable("Tools", "dbo");
                });

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.ToolCategory", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("ID"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("ID");

                    b.ToTable("ToolCategories", "dbo");
                });

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.User", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("ID"));

                    b.Property<string>("Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(99)
                        .HasColumnType("nvarchar(99)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(99)
                        .HasColumnType("nvarchar(99)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(99)
                        .HasColumnType("nvarchar(99)");

                    b.Property<DateTime>("Registration")
                        .HasColumnType("datetime2");

                    b.Property<string>("SaltCode")
                        .HasMaxLength(99)
                        .HasColumnType("nvarchar(99)");

                    b.Property<string>("TimeZone")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<long?>("ToolID")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.HasIndex("ToolID");

                    b.ToTable("Users", "dbo");
                });

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.UserCode", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("ID"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(99)
                        .HasColumnType("nvarchar(99)");

                    b.HasKey("ID");

                    b.ToTable("UserCodes", "dbo");
                });

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.Tool", b =>
                {
                    b.HasOne("SearchAiDirectory.Shared.Models.ToolCategory", "Category")
                        .WithMany("Tools")
                        .HasForeignKey("CategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.User", b =>
                {
                    b.HasOne("SearchAiDirectory.Shared.Models.Tool", "Tool")
                        .WithMany()
                        .HasForeignKey("ToolID");

                    b.Navigation("Tool");
                });

            modelBuilder.Entity("SearchAiDirectory.Shared.Models.ToolCategory", b =>
                {
                    b.Navigation("Tools");
                });
#pragma warning restore 612, 618
        }
    }
}
