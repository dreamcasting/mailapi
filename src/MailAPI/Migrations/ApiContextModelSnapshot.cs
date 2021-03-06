﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MailAPI.Models;

namespace MailAPI.Migrations
{
    [DbContext(typeof(ApiContext))]
    partial class ApiContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MailAPI.Models.ApiKey", b =>
                {
                    b.Property<int>("ApiKeyId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("ActiveStatus");

                    b.Property<string>("AssociatedApplication");

                    b.Property<string>("Key");

                    b.Property<string>("Salt");

                    b.Property<DateTime>("TimeStamp");

                    b.Property<long>("Uses");

                    b.HasKey("ApiKeyId");

                    b.ToTable("ApiKeys");
                });
        }
    }
}
