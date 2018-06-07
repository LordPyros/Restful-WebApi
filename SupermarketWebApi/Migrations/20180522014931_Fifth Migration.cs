using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SupermarketWebApi.Migrations
{
    public partial class FifthMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupermarketStocks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NumberInStock = table.Column<int>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    SupermarketId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupermarketStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupermarketStocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupermarketStocks_Supermarkets_SupermarketId",
                        column: x => x.SupermarketId,
                        principalTable: "Supermarkets",
                        principalColumn: "SupermarketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupermarketStocks_ProductId",
                table: "SupermarketStocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SupermarketStocks_SupermarketId",
                table: "SupermarketStocks",
                column: "SupermarketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupermarketStocks");
        }
    }
}
