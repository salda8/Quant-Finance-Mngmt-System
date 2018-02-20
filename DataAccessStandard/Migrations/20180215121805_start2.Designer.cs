﻿// <auto-generated />
using Common.Enums;
using DataAccess;
using DataAccess.EntityConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace DataAccessStandard.Migrations
{
    [DbContext(typeof(MyDBContext))]
    [Migration("20180215121805_start2")]
    partial class start2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Common.EntityModels.Account", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumber");

                    b.Property<string>("BrokerName");

                    b.Property<decimal>("InitialBalance");

                    b.Property<int>("StrategyID");

                    b.HasKey("ID");

                    b.HasIndex("StrategyID");

                    b.ToTable("Account");
                });

            modelBuilder.Entity("Common.EntityModels.AccountSummary", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountID");

                    b.Property<decimal>("CashBalance");

                    b.Property<decimal>("NetLiquidation");

                    b.Property<decimal>("UnrealizedPnL");

                    b.HasKey("ID");

                    b.HasIndex("AccountID");

                    b.ToTable("AccountSummary");
                });

            modelBuilder.Entity("Common.EntityModels.CommissionMessage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Commission");

                    b.Property<string>("ExecutionId");

                    b.Property<double>("RealizedPnL");

                    b.HasKey("ID");

                    b.ToTable("CommissionMessage");
                });

            modelBuilder.Entity("Common.EntityModels.Datasource", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .HasMaxLength(100);

                    b.HasKey("ID");

                    b.ToTable("Datasources");
                });

            modelBuilder.Entity("Common.EntityModels.Equity", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountID");

                    b.Property<DateTime>("UpdateTime");

                    b.Property<decimal>("Value");

                    b.HasKey("ID");

                    b.HasIndex("AccountID");

                    b.ToTable("Equity");
                });

            modelBuilder.Entity("Common.EntityModels.Exchange", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("LongName")
                        .HasMaxLength(255);

                    b.Property<string>("Name")
                        .HasMaxLength(100);

                    b.Property<string>("Timezone")
                        .HasMaxLength(255);

                    b.HasKey("ID");

                    b.ToTable("Exchanges");
                });

            modelBuilder.Entity("Common.EntityModels.ExecutionMessage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountID");

                    b.Property<string>("ExecutionId");

                    b.Property<int>("InstrumentID");

                    b.Property<int?>("InstrumentID1");

                    b.Property<int>("OrderId");

                    b.Property<int>("PermanentId");

                    b.Property<decimal>("Price");

                    b.Property<int>("Quantity");

                    b.Property<int>("RequestId");

                    b.Property<string>("Side");

                    b.Property<DateTime>("Time");

                    b.HasKey("ID");

                    b.HasIndex("AccountID");

                    b.HasIndex("InstrumentID");

                    b.HasIndex("InstrumentID1");

                    b.ToTable("ExecutionMessage");
                });

            modelBuilder.Entity("Common.EntityModels.ExpirationRule", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DaysBefore");

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("ExpirationRule");
                });

            modelBuilder.Entity("Common.EntityModels.Instrument", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Currency")
                        .HasMaxLength(25);

                    b.Property<int?>("DatasourceID");

                    b.Property<int>("ExchangeID");

                    b.Property<DateTime>("Expiration");

                    b.Property<int>("ExpirationRuleID");

                    b.Property<string>("Name")
                        .HasMaxLength(255);

                    b.Property<int>("SessionTemplateID");

                    b.Property<int>("SessionsSource");

                    b.Property<string>("Symbol")
                        .HasMaxLength(100);

                    b.Property<int>("Type");

                    b.Property<string>("UnderlyingSymbol")
                        .HasMaxLength(255);

                    b.Property<string>("ValidExchanges");

                    b.HasKey("ID");

                    b.HasIndex("DatasourceID");

                    b.HasIndex("ExchangeID")
                        .IsUnique();

                    b.HasIndex("Expiration")
                        .IsUnique();

                    b.HasIndex("ExpirationRuleID");

                    b.HasIndex("Symbol")
                        .IsUnique()
                        .HasFilter("[Symbol] IS NOT NULL");

                    b.HasIndex("Type")
                        .IsUnique();

                    b.ToTable("Instruments");
                });

            modelBuilder.Entity("Common.EntityModels.InstrumentSession", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClosingDay");

                    b.Property<TimeSpan>("ClosingTime");

                    b.Property<int>("InstrumentID");

                    b.Property<bool>("IsSessionEnd");

                    b.Property<int>("OpeningDay");

                    b.Property<TimeSpan>("OpeningTime");

                    b.HasKey("ID");

                    b.HasIndex("InstrumentID");

                    b.ToTable("InstrumentSessions");
                });

            modelBuilder.Entity("Common.EntityModels.LiveTrade", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountID");

                    b.Property<decimal>("AveragePrice");

                    b.Property<int>("InstrumentID");

                    b.Property<decimal>("MarketPrice");

                    b.Property<int>("Port");

                    b.Property<decimal>("Quantity");

                    b.Property<decimal>("RealizedPnl");

                    b.Property<byte>("TradeDirection");

                    b.Property<decimal>("UnrealizedPnL");

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("ID");

                    b.HasIndex("AccountID");

                    b.HasIndex("InstrumentID");

                    b.ToTable("LiveTrade");
                });

            modelBuilder.Entity("Common.EntityModels.OpenOrder", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountID");

                    b.Property<int>("InstrumentID");

                    b.Property<decimal>("LimitPrice");

                    b.Property<int>("PermanentId");

                    b.Property<decimal>("Quantity");

                    b.Property<string>("Status");

                    b.Property<string>("Type");

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("ID");

                    b.HasIndex("AccountID");

                    b.HasIndex("InstrumentID");

                    b.ToTable("OpenOrder");
                });

            modelBuilder.Entity("Common.EntityModels.OrderStatusMessage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("AverageFillPrice");

                    b.Property<int>("ClientId");

                    b.Property<int>("Filled");

                    b.Property<decimal>("LastFillPrice");

                    b.Property<int>("OrderId");

                    b.Property<int>("ParentId");

                    b.Property<int>("PermanentId");

                    b.Property<int>("Remaining");

                    b.Property<string>("Status");

                    b.Property<string>("WhyHeld");

                    b.HasKey("ID");

                    b.ToTable("OrderStatusMessage");
                });

            modelBuilder.Entity("Common.EntityModels.SessionTemplate", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .HasMaxLength(255);

                    b.HasKey("ID");

                    b.ToTable("SessionTemplates");
                });

            modelBuilder.Entity("Common.EntityModels.Strategy", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("BacktestDrawDown");

                    b.Property<decimal>("BacktestPeriodInYears");

                    b.Property<decimal>("BacktestProfit");

                    b.Property<int>("InstrumentID");

                    b.Property<string>("StrategyName");

                    b.HasKey("ID");

                    b.HasIndex("InstrumentID");

                    b.ToTable("Strategy");
                });

            modelBuilder.Entity("Common.EntityModels.Tag", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("InstrumentID");

                    b.Property<string>("Name")
                        .HasMaxLength(255);

                    b.HasKey("ID");

                    b.HasIndex("InstrumentID");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Common.EntityModels.TemplateSession", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ClosingDay");

                    b.Property<TimeSpan>("ClosingTime");

                    b.Property<bool>("IsSessionEnd");

                    b.Property<int>("OpeningDay");

                    b.Property<TimeSpan>("OpeningTime");

                    b.Property<int>("TemplateID");

                    b.HasKey("ID");

                    b.HasIndex("TemplateID");

                    b.ToTable("TemplateSessions");
                });

            modelBuilder.Entity("Common.EntityModels.TradeHistory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountID");

                    b.Property<decimal>("Commission");

                    b.Property<string>("ExecutionID");

                    b.Property<DateTime>("ExecutionTime");

                    b.Property<int>("InstrumentID");

                    b.Property<decimal>("Price");

                    b.Property<decimal>("Quantity");

                    b.Property<decimal>("RealizedPnL");

                    b.Property<byte>("Side");

                    b.HasKey("ID");

                    b.HasIndex("AccountID");

                    b.HasIndex("InstrumentID");

                    b.ToTable("TradeHistory");
                });

            modelBuilder.Entity("DataAccess.EntityConfigs.MqServer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("HostIp");

                    b.Property<string>("Name");

                    b.Property<int>("Port");

                    b.Property<int>("ServerType");

                    b.Property<int>("TransportProtocol");

                    b.HasKey("ID");

                    b.ToTable("MqServers");
                });

            modelBuilder.Entity("Common.EntityModels.Account", b =>
                {
                    b.HasOne("Common.EntityModels.Strategy", "Strategy")
                        .WithMany()
                        .HasForeignKey("StrategyID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Common.EntityModels.AccountSummary", b =>
                {
                    b.HasOne("Common.EntityModels.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Common.EntityModels.Equity", b =>
                {
                    b.HasOne("Common.EntityModels.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Common.EntityModels.ExecutionMessage", b =>
                {
                    b.HasOne("Common.EntityModels.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Common.EntityModels.Instrument")
                        .WithMany()
                        .HasForeignKey("InstrumentID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Common.EntityModels.Instrument", "Instrument")
                        .WithMany()
                        .HasForeignKey("InstrumentID1");
                });

            modelBuilder.Entity("Common.EntityModels.Instrument", b =>
                {
                    b.HasOne("Common.EntityModels.Datasource", "Datasource")
                        .WithMany()
                        .HasForeignKey("DatasourceID");

                    b.HasOne("Common.EntityModels.Exchange", "Exchange")
                        .WithMany()
                        .HasForeignKey("ExchangeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Common.EntityModels.ExpirationRule", "ExpirationRule")
                        .WithMany()
                        .HasForeignKey("ExpirationRuleID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Common.EntityModels.InstrumentSession", b =>
                {
                    b.HasOne("Common.EntityModels.Instrument", "Instrument")
                        .WithMany("Sessions")
                        .HasForeignKey("InstrumentID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Common.EntityModels.LiveTrade", b =>
                {
                    b.HasOne("Common.EntityModels.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Common.EntityModels.Instrument", "Instrument")
                        .WithMany()
                        .HasForeignKey("InstrumentID")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Common.EntityModels.OpenOrder", b =>
                {
                    b.HasOne("Common.EntityModels.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Common.EntityModels.Instrument", "Instrument")
                        .WithMany()
                        .HasForeignKey("InstrumentID")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Common.EntityModels.Strategy", b =>
                {
                    b.HasOne("Common.EntityModels.Instrument", "Instrument")
                        .WithMany()
                        .HasForeignKey("InstrumentID")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Common.EntityModels.Tag", b =>
                {
                    b.HasOne("Common.EntityModels.Instrument")
                        .WithMany("Tags")
                        .HasForeignKey("InstrumentID");
                });

            modelBuilder.Entity("Common.EntityModels.TemplateSession", b =>
                {
                    b.HasOne("Common.EntityModels.SessionTemplate", "Template")
                        .WithMany("Sessions")
                        .HasForeignKey("TemplateID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Common.EntityModels.TradeHistory", b =>
                {
                    b.HasOne("Common.EntityModels.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Common.EntityModels.Instrument", "Instrument")
                        .WithMany()
                        .HasForeignKey("InstrumentID")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}
