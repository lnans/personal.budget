﻿// <auto-generated />
using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbDbContext))]
    [Migration("20220221063806_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.2");

            modelBuilder.Entity("Domain.Entities.Constant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<float>("Amount")
                        .HasColumnType("REAL");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Constants");
                });

            modelBuilder.Entity("Domain.Entities.Operation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<float>("Amount")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ReportId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("TagId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ReportId");

                    b.HasIndex("TagId");

                    b.ToTable("Operations");
                });

            modelBuilder.Entity("Domain.Entities.OperationTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Color")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("OperationTags");
                });

            modelBuilder.Entity("Domain.Entities.Report", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<float>("Budget")
                        .HasColumnType("REAL");

                    b.Property<int>("Days")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Month")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Domain.Entities.ReportConstant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<float>("Amount")
                        .HasColumnType("REAL");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ReportId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ReportId");

                    b.ToTable("ReportConstants");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Domain.Entities.Operation", b =>
                {
                    b.HasOne("Domain.Entities.Report", "Report")
                        .WithMany("Operations")
                        .HasForeignKey("ReportId");

                    b.HasOne("Domain.Entities.OperationTag", "Tag")
                        .WithMany()
                        .HasForeignKey("TagId");

                    b.Navigation("Report");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Domain.Entities.ReportConstant", b =>
                {
                    b.HasOne("Domain.Entities.Report", null)
                        .WithMany("Constants")
                        .HasForeignKey("ReportId");
                });

            modelBuilder.Entity("Domain.Entities.Report", b =>
                {
                    b.Navigation("Constants");

                    b.Navigation("Operations");
                });
#pragma warning restore 612, 618
        }
    }
}
