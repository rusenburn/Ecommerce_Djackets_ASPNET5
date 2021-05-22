using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.Shared.Migrations
{
    public partial class AddShippingInformationToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecipeURL",
                table: "Orders",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingInfoId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShippingInfoSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ShippedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingInfoSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingInfoSet_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingInfoSet_OrderId",
                table: "ShippingInfoSet",
                column: "OrderId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingInfoSet");

            migrationBuilder.DropColumn(
                name: "RecipeURL",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingInfoId",
                table: "Orders");
        }
    }
}
