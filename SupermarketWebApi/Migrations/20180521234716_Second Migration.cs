using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SupermarketWebApi.Migrations
{
    public partial class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberInStock",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Supermarkets",
                newName: "SupermarketId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Products",
                newName: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupermarketId",
                table: "Supermarkets",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Products",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "NumberInStock",
                table: "Products",
                nullable: false,
                defaultValue: 0);
        }
    }
}
