﻿// <auto-generated />
using System;
using DataAccess.DataContext_Class;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230214101438_spellingCorrectionInMessageTable")]
    partial class spellingCorrectionInMessageTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Business_Core.Entities.Contact", b =>
                {
                    b.Property<int>("ContactID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ContactID"), 1L, 1);

                    b.Property<bool>("Block_Contact")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("Created_At")
                        .HasColumnType("datetime2");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserGroupPrivateConnectionId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<bool>("Verified_Contact")
                        .HasColumnType("bit");

                    b.HasKey("ContactID");

                    b.HasIndex("UserId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("Business_Core.Entities.Message", b =>
                {
                    b.Property<int>("MessageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageID"), 1L, 1);

                    b.Property<DateTime?>("Created_At")
                        .HasColumnType("datetime2");

                    b.Property<bool>("MessageSeen")
                        .HasColumnType("bit");

                    b.Property<int?>("ReceiverId")
                        .HasColumnType("int");

                    b.Property<int?>("SenderId")
                        .HasColumnType("int");

                    b.Property<string>("UserMessage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MessageID");

                    b.HasIndex("ReceiverId");

                    b.HasIndex("SenderId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Business_Core.Entities.MessageAttachment", b =>
                {
                    b.Property<int>("MessageAttachmentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageAttachmentID"), 1L, 1);

                    b.Property<int>("MessageId")
                        .HasColumnType("int");

                    b.Property<string>("PublicId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("URL")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MessageAttachmentID");

                    b.HasIndex("MessageId");

                    b.ToTable("MessageAttachments");
                });

            modelBuilder.Entity("Business_Core.Entities.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserID"), 1L, 1);

                    b.Property<string>("About")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ContactNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("Created_At")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailVerification")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("Last_Online")
                        .HasColumnType("datetime2");

                    b.Property<string>("ProfilePhotoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PublicId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VerificationPassword")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isActive")
                        .HasColumnType("bit");

                    b.HasKey("UserID");

                    b.HasIndex("ContactNumber")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Business_Core.Entities.Contact", b =>
                {
                    b.HasOne("Business_Core.Entities.User", "User")
                        .WithMany("Contacts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Business_Core.Entities.Message", b =>
                {
                    b.HasOne("Business_Core.Entities.User", "ReciverUser")
                        .WithMany("ReciverMessage")
                        .HasForeignKey("ReceiverId");

                    b.HasOne("Business_Core.Entities.User", "SenderUser")
                        .WithMany("SenderMessages")
                        .HasForeignKey("SenderId");

                    b.Navigation("ReciverUser");

                    b.Navigation("SenderUser");
                });

            modelBuilder.Entity("Business_Core.Entities.MessageAttachment", b =>
                {
                    b.HasOne("Business_Core.Entities.Message", "Message")
                        .WithMany("MessageAttachments")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Message");
                });

            modelBuilder.Entity("Business_Core.Entities.Message", b =>
                {
                    b.Navigation("MessageAttachments");
                });

            modelBuilder.Entity("Business_Core.Entities.User", b =>
                {
                    b.Navigation("Contacts");

                    b.Navigation("ReciverMessage");

                    b.Navigation("SenderMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
