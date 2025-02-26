﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using phoneCaseReworked.Models;

#nullable disable

namespace phoneCaseReworked.Migrations
{
    [DbContext(typeof(PhoneCaseDbContext))]
    [Migration("20250225041408_updated dateonly to datetime")]
    partial class updateddateonlytodatetime
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("phoneCaseReworked.Models.CaseManufacturer", b =>
                {
                    b.Property<int>("CaseManufacturerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CaseManufacturerId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("CaseManufacturerId");

                    b.ToTable("CaseManufacturers");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PaymentId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(10,2)");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("date");

                    b.Property<int>("VendorId")
                        .HasColumnType("int");

                    b.HasKey("PaymentId");

                    b.HasIndex("VendorId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.PhoneModel", b =>
                {
                    b.Property<int>("ModelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ModelId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("ModelId");

                    b.ToTable("PhoneModels");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductId"));

                    b.Property<int?>("CaseManufacturerId")
                        .HasColumnType("int");

                    b.Property<string>("CaseName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("ModelId")
                        .HasColumnType("int");

                    b.HasKey("ProductId");

                    b.HasIndex("CaseManufacturerId");

                    b.HasIndex("ModelId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.Purchase", b =>
                {
                    b.Property<int>("PurchaseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PurchaseId"));

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PurchaseDate")
                        .HasColumnType("date");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(10,2)");

                    b.Property<int>("VendorId")
                        .HasColumnType("int");

                    b.HasKey("PurchaseId");

                    b.HasIndex("ProductId");

                    b.HasIndex("VendorId");

                    b.ToTable("Purchases");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.Vendor", b =>
                {
                    b.Property<int>("VendorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("VendorId"));

                    b.Property<string>("ContactInfo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("TotalCredit")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("VendorId");

                    b.ToTable("Vendors");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.Payment", b =>
                {
                    b.HasOne("phoneCaseReworked.Models.Vendor", "Vendor")
                        .WithMany()
                        .HasForeignKey("VendorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Vendor");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.Product", b =>
                {
                    b.HasOne("phoneCaseReworked.Models.CaseManufacturer", "CaseManufacturer")
                        .WithMany()
                        .HasForeignKey("CaseManufacturerId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("phoneCaseReworked.Models.PhoneModel", "Model")
                        .WithMany()
                        .HasForeignKey("ModelId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("CaseManufacturer");

                    b.Navigation("Model");
                });

            modelBuilder.Entity("phoneCaseReworked.Models.Purchase", b =>
                {
                    b.HasOne("phoneCaseReworked.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("phoneCaseReworked.Models.Vendor", "Vendor")
                        .WithMany()
                        .HasForeignKey("VendorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Vendor");
                });
#pragma warning restore 612, 618
        }
    }
}
