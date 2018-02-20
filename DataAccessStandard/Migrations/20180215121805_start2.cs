using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace DataAccessStandard.Migrations
{
    public partial class start2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountSummary_Account_AccountID",
                table: "AccountSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountSummary_Account_AccountID1",
                table: "AccountSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_Strategy_Instruments_InstrumentID",
                table: "Strategy");

            migrationBuilder.DropIndex(
                name: "IX_AccountSummary_AccountID",
                table: "AccountSummary");

            migrationBuilder.DropIndex(
                name: "IX_AccountSummary_AccountID1",
                table: "AccountSummary");

            migrationBuilder.DropColumn(
                name: "AccountID1",
                table: "AccountSummary");

            migrationBuilder.CreateIndex(
                name: "IX_AccountSummary_AccountID",
                table: "AccountSummary",
                column: "AccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSummary_Account_AccountID",
                table: "AccountSummary",
                column: "AccountID",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Strategy_Instruments_InstrumentID",
                table: "Strategy",
                column: "InstrumentID",
                principalTable: "Instruments",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountSummary_Account_AccountID",
                table: "AccountSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_Strategy_Instruments_InstrumentID",
                table: "Strategy");

            migrationBuilder.DropIndex(
                name: "IX_AccountSummary_AccountID",
                table: "AccountSummary");

            migrationBuilder.AddColumn<int>(
                name: "AccountID1",
                table: "AccountSummary",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountSummary_AccountID",
                table: "AccountSummary",
                column: "AccountID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountSummary_AccountID1",
                table: "AccountSummary",
                column: "AccountID1");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSummary_Account_AccountID",
                table: "AccountSummary",
                column: "AccountID",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountSummary_Account_AccountID1",
                table: "AccountSummary",
                column: "AccountID1",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Strategy_Instruments_InstrumentID",
                table: "Strategy",
                column: "InstrumentID",
                principalTable: "Instruments",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
