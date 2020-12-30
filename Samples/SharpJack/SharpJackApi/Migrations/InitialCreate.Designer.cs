﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharpJackApi.Data;

namespace SharpJackApi.Migrations
{
    [DbContext(typeof(GameContext))]
    [Migration("20201230191030_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("SharpJackApi.Models.Answer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("GameId")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("QuestionId")
                        .HasColumnType("int");

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.Property<DateTime>("SubmitTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Answers");
                });

            modelBuilder.Entity("SharpJackApi.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("ActivePlayer")
                        .HasColumnType("int");

                    b.Property<int?>("ActiveQuestionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ActiveUntil")
                        .HasColumnType("datetime2");

                    b.Property<int?>("BoardId")
                        .HasColumnType("int");

                    b.Property<int>("CurrentRound")
                        .HasColumnType("int");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ActiveQuestionId");

                    b.HasIndex("BoardId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("SharpJackApi.Models.LeaderBoard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.HasKey("Id");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("SharpJackApi.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("GameId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("SharpJackApi.Models.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int>("Answer")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("SharpJackApi.Models.Row", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("LeaderBoardId")
                        .HasColumnType("int");

                    b.Property<int?>("PlayerId")
                        .HasColumnType("int");

                    b.Property<int>("PlayerScore")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LeaderBoardId");

                    b.HasIndex("PlayerId");

                    b.ToTable("Rows");
                });

            modelBuilder.Entity("SharpJackApi.Models.Answer", b =>
                {
                    b.HasOne("SharpJackApi.Models.Game", null)
                        .WithMany("Answers")
                        .HasForeignKey("GameId");
                });

            modelBuilder.Entity("SharpJackApi.Models.Game", b =>
                {
                    b.HasOne("SharpJackApi.Models.Question", "ActiveQuestion")
                        .WithMany()
                        .HasForeignKey("ActiveQuestionId");

                    b.HasOne("SharpJackApi.Models.LeaderBoard", "Board")
                        .WithMany()
                        .HasForeignKey("BoardId");

                    b.OwnsOne("SharpJackApi.Contracts.GameOptions", "Options", b1 =>
                        {
                            b1.Property<int>("GameId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .UseIdentityColumn();

                            b1.Property<int>("MaxActiveTime")
                                .HasColumnType("int");

                            b1.Property<int>("MaxAnswerTime")
                                .HasColumnType("int");

                            b1.Property<int>("MaxPlayers")
                                .HasColumnType("int");

                            b1.Property<int>("MaxQuestionTime")
                                .HasColumnType("int");

                            b1.Property<int>("MaxRounds")
                                .HasColumnType("int");

                            b1.Property<int>("PlayerId")
                                .HasColumnType("int");

                            b1.HasKey("GameId");

                            b1.ToTable("Games");

                            b1.WithOwner()
                                .HasForeignKey("GameId");
                        });

                    b.Navigation("ActiveQuestion");

                    b.Navigation("Board");

                    b.Navigation("Options");
                });

            modelBuilder.Entity("SharpJackApi.Models.Player", b =>
                {
                    b.HasOne("SharpJackApi.Models.Game", null)
                        .WithMany("Players")
                        .HasForeignKey("GameId");
                });

            modelBuilder.Entity("SharpJackApi.Models.Row", b =>
                {
                    b.HasOne("SharpJackApi.Models.LeaderBoard", null)
                        .WithMany("Rows")
                        .HasForeignKey("LeaderBoardId");

                    b.HasOne("SharpJackApi.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("SharpJackApi.Models.Game", b =>
                {
                    b.Navigation("Answers");

                    b.Navigation("Players");
                });

            modelBuilder.Entity("SharpJackApi.Models.LeaderBoard", b =>
                {
                    b.Navigation("Rows");
                });
#pragma warning restore 612, 618
        }
    }
}