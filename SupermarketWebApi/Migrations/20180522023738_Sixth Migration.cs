using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SupermarketWebApi.Migrations
{
    public partial class SixthMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "StaffMembers",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "SupermarketId",
                table: "StaffMembers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_SupermarketId",
                table: "StaffMembers",
                column: "SupermarketId");

            migrationBuilder.AddForeignKey(
                name: "FK_StaffMembers_Supermarkets_SupermarketId",
                table: "StaffMembers",
                column: "SupermarketId",
                principalTable: "Supermarkets",
                principalColumn: "SupermarketId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffMembers_Supermarkets_SupermarketId",
                table: "StaffMembers");

            migrationBuilder.DropIndex(
                name: "IX_StaffMembers_SupermarketId",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "SupermarketId",
                table: "StaffMembers");

            migrationBuilder.AlterColumn<int>(
                name: "Address",
                table: "StaffMembers",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
