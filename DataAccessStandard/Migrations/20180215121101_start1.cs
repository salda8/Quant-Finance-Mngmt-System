using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DataAccessStandard.Migrations
{
    public partial class start1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommissionMessage",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Commission = table.Column<decimal>(nullable: false),
                    ExecutionId = table.Column<string>(nullable: true),
                    RealizedPnL = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionMessage", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Datasources",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datasources", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LongName = table.Column<string>(maxLength: 255, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Timezone = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ExpirationRule",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DaysBefore = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpirationRule", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MqServers",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    HostIp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Port = table.Column<int>(nullable: false),
                    ServerType = table.Column<int>(nullable: false),
                    TransportProtocol = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MqServers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusMessage",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AverageFillPrice = table.Column<decimal>(nullable: false),
                    ClientId = table.Column<int>(nullable: false),
                    Filled = table.Column<int>(nullable: false),
                    LastFillPrice = table.Column<decimal>(nullable: false),
                    OrderId = table.Column<int>(nullable: false),
                    ParentId = table.Column<int>(nullable: false),
                    PermanentId = table.Column<int>(nullable: false),
                    Remaining = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    WhyHeld = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusMessage", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SessionTemplates",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTemplates", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Instruments",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Currency = table.Column<string>(maxLength: 25, nullable: true),
                    DatasourceID = table.Column<int>(nullable: true),
                    ExchangeID = table.Column<int>(nullable: false),
                    Expiration = table.Column<DateTime>(nullable: false),
                    ExpirationRuleID = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    SessionTemplateID = table.Column<int>(nullable: false),
                    SessionsSource = table.Column<int>(nullable: false),
                    Symbol = table.Column<string>(maxLength: 100, nullable: true),
                    Type = table.Column<int>(nullable: false),
                    UnderlyingSymbol = table.Column<string>(maxLength: 255, nullable: true),
                    ValidExchanges = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Instruments_Datasources_DatasourceID",
                        column: x => x.DatasourceID,
                        principalTable: "Datasources",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instruments_Exchanges_ExchangeID",
                        column: x => x.ExchangeID,
                        principalTable: "Exchanges",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Instruments_ExpirationRule_ExpirationRuleID",
                        column: x => x.ExpirationRuleID,
                        principalTable: "ExpirationRule",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateSessions",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClosingDay = table.Column<int>(nullable: false),
                    ClosingTime = table.Column<TimeSpan>(nullable: false),
                    IsSessionEnd = table.Column<bool>(nullable: false),
                    OpeningDay = table.Column<int>(nullable: false),
                    OpeningTime = table.Column<TimeSpan>(nullable: false),
                    TemplateID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateSessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TemplateSessions_SessionTemplates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "SessionTemplates",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentSessions",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClosingDay = table.Column<int>(nullable: false),
                    ClosingTime = table.Column<TimeSpan>(nullable: false),
                    InstrumentID = table.Column<int>(nullable: false),
                    IsSessionEnd = table.Column<bool>(nullable: false),
                    OpeningDay = table.Column<int>(nullable: false),
                    OpeningTime = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentSessions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InstrumentSessions_Instruments_InstrumentID",
                        column: x => x.InstrumentID,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strategy",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BacktestDrawDown = table.Column<decimal>(nullable: false),
                    BacktestPeriodInYears = table.Column<decimal>(nullable: false),
                    BacktestProfit = table.Column<decimal>(nullable: false),
                    InstrumentID = table.Column<int>(nullable: false),
                    StrategyName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategy", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Strategy_Instruments_InstrumentID",
                        column: x => x.InstrumentID,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InstrumentID = table.Column<int>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tags_Instruments_InstrumentID",
                        column: x => x.InstrumentID,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountNumber = table.Column<string>(nullable: true),
                    BrokerName = table.Column<string>(nullable: true),
                    InitialBalance = table.Column<decimal>(nullable: false),
                    StrategyID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Account_Strategy_StrategyID",
                        column: x => x.StrategyID,
                        principalTable: "Strategy",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountSummary",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountID = table.Column<int>(nullable: false),
                    AccountID1 = table.Column<int>(nullable: true),
                    CashBalance = table.Column<decimal>(nullable: false),
                    NetLiquidation = table.Column<decimal>(nullable: false),
                    UnrealizedPnL = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSummary", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AccountSummary_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountSummary_Account_AccountID1",
                        column: x => x.AccountID1,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equity",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountID = table.Column<int>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false),
                    Value = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equity", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Equity_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionMessage",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountID = table.Column<int>(nullable: false),
                    ExecutionId = table.Column<string>(nullable: true),
                    InstrumentID = table.Column<int>(nullable: false),
                    InstrumentID1 = table.Column<int>(nullable: true),
                    OrderId = table.Column<int>(nullable: false),
                    PermanentId = table.Column<int>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    RequestId = table.Column<int>(nullable: false),
                    Side = table.Column<string>(nullable: true),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionMessage", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExecutionMessage_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExecutionMessage_Instruments_InstrumentID",
                        column: x => x.InstrumentID,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExecutionMessage_Instruments_InstrumentID1",
                        column: x => x.InstrumentID1,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveTrade",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountID = table.Column<int>(nullable: false),
                    AveragePrice = table.Column<decimal>(nullable: false),
                    InstrumentID = table.Column<int>(nullable: false),
                    MarketPrice = table.Column<decimal>(nullable: false),
                    Port = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    RealizedPnl = table.Column<decimal>(nullable: false),
                    TradeDirection = table.Column<byte>(nullable: false),
                    UnrealizedPnL = table.Column<decimal>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveTrade", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LiveTrade_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveTrade_Instruments_InstrumentID",
                        column: x => x.InstrumentID,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenOrder",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountID = table.Column<int>(nullable: false),
                    InstrumentID = table.Column<int>(nullable: false),
                    LimitPrice = table.Column<decimal>(nullable: false),
                    PermanentId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenOrder", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OpenOrder_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpenOrder_Instruments_InstrumentID",
                        column: x => x.InstrumentID,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TradeHistory",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountID = table.Column<int>(nullable: false),
                    Commission = table.Column<decimal>(nullable: false),
                    ExecutionID = table.Column<string>(nullable: true),
                    ExecutionTime = table.Column<DateTime>(nullable: false),
                    InstrumentID = table.Column<int>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    RealizedPnL = table.Column<decimal>(nullable: false),
                    Side = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeHistory", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TradeHistory_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeHistory_Instruments_InstrumentID",
                        column: x => x.InstrumentID,
                        principalTable: "Instruments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_StrategyID",
                table: "Account",
                column: "StrategyID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSummary_AccountID",
                table: "AccountSummary",
                column: "AccountID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountSummary_AccountID1",
                table: "AccountSummary",
                column: "AccountID1");

            migrationBuilder.CreateIndex(
                name: "IX_Equity_AccountID",
                table: "Equity",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionMessage_AccountID",
                table: "ExecutionMessage",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionMessage_InstrumentID",
                table: "ExecutionMessage",
                column: "InstrumentID");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionMessage_InstrumentID1",
                table: "ExecutionMessage",
                column: "InstrumentID1");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_DatasourceID",
                table: "Instruments",
                column: "DatasourceID");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_ExchangeID",
                table: "Instruments",
                column: "ExchangeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Expiration",
                table: "Instruments",
                column: "Expiration",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_ExpirationRuleID",
                table: "Instruments",
                column: "ExpirationRuleID");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Symbol",
                table: "Instruments",
                column: "Symbol",
                unique: true,
                filter: "[Symbol] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Type",
                table: "Instruments",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentSessions_InstrumentID",
                table: "InstrumentSessions",
                column: "InstrumentID");

            migrationBuilder.CreateIndex(
                name: "IX_LiveTrade_AccountID",
                table: "LiveTrade",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_LiveTrade_InstrumentID",
                table: "LiveTrade",
                column: "InstrumentID");

            migrationBuilder.CreateIndex(
                name: "IX_OpenOrder_AccountID",
                table: "OpenOrder",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_OpenOrder_InstrumentID",
                table: "OpenOrder",
                column: "InstrumentID");

            migrationBuilder.CreateIndex(
                name: "IX_Strategy_InstrumentID",
                table: "Strategy",
                column: "InstrumentID");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_InstrumentID",
                table: "Tags",
                column: "InstrumentID");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateSessions_TemplateID",
                table: "TemplateSessions",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_AccountID",
                table: "TradeHistory",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_TradeHistory_InstrumentID",
                table: "TradeHistory",
                column: "InstrumentID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSummary");

            migrationBuilder.DropTable(
                name: "CommissionMessage");

            migrationBuilder.DropTable(
                name: "Equity");

            migrationBuilder.DropTable(
                name: "ExecutionMessage");

            migrationBuilder.DropTable(
                name: "InstrumentSessions");

            migrationBuilder.DropTable(
                name: "LiveTrade");

            migrationBuilder.DropTable(
                name: "MqServers");

            migrationBuilder.DropTable(
                name: "OpenOrder");

            migrationBuilder.DropTable(
                name: "OrderStatusMessage");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TemplateSessions");

            migrationBuilder.DropTable(
                name: "TradeHistory");

            migrationBuilder.DropTable(
                name: "SessionTemplates");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Strategy");

            migrationBuilder.DropTable(
                name: "Instruments");

            migrationBuilder.DropTable(
                name: "Datasources");

            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropTable(
                name: "ExpirationRule");
        }
    }
}
